using EventoTickets.Abstracciones.Configuracion;
using EventoTickets.Abstracciones.Interfaces.Servicios;
using EventoTickets.Abstracciones.Modelos;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace EventoTickets.Servicios;

/// <summary>
/// Guarda los comprobantes fuera de wwwroot (no son públicos) y calcula su SHA-256,
/// que la base de datos usa para detectar comprobantes reutilizados.
/// </summary>
public class ServicioArchivosComprobantes : IServicioArchivosComprobantes
{
    private readonly string _raiz;
    private readonly IServicioHash _hash;

    public ServicioArchivosComprobantes(IOptions<ConfiguracionAlmacenamiento> opciones, IHostEnvironment entorno, IServicioHash hash)
    {
        _hash = hash;
        var ruta = opciones.Value.RutaComprobantes;
        _raiz = Path.GetFullPath(Path.IsPathRooted(ruta) ? ruta : Path.Combine(entorno.ContentRootPath, ruta));
    }

    public async Task<ComprobanteGuardado> GuardarAsync(IFormFile archivo)
    {
        await using var memoria = new MemoryStream();
        await archivo.CopyToAsync(memoria);
        var bytes = memoria.ToArray();
        var hash = _hash.CalcularSha256(bytes);

        var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
        var carpeta = DateTime.UtcNow.ToString("yyyy/MM");
        var relativa = $"{carpeta}/{Guid.NewGuid():N}{extension}";
        var completa = RutaSegura(relativa);

        Directory.CreateDirectory(Path.GetDirectoryName(completa)!);
        await File.WriteAllBytesAsync(completa, bytes);
        return new ComprobanteGuardado { RutaRelativa = relativa, Hash = hash };
    }

    public Task<ArchivoComprobante?> ObtenerAsync(string rutaRelativa)
    {
        var completa = RutaSegura(rutaRelativa);
        if (!File.Exists(completa)) return Task.FromResult<ArchivoComprobante?>(null);

        var tipo = Path.GetExtension(completa).ToLowerInvariant() switch
        {
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".pdf" => "application/pdf",
            _ => "application/octet-stream"
        };
        return Task.FromResult<ArchivoComprobante?>(new ArchivoComprobante
        {
            Contenido = File.OpenRead(completa),
            TipoContenido = tipo,
            NombreArchivo = Path.GetFileName(completa)
        });
    }

    public Task EliminarAsync(string rutaRelativa)
    {
        var completa = RutaSegura(rutaRelativa);
        if (File.Exists(completa)) File.Delete(completa);
        return Task.CompletedTask;
    }

    /// <summary>Evita rutas fuera de la carpeta raíz (path traversal).</summary>
    private string RutaSegura(string rutaRelativa)
    {
        var completa = Path.GetFullPath(Path.Combine(_raiz, rutaRelativa.Replace('/', Path.DirectorySeparatorChar)));
        var raizConSeparador = _raiz.EndsWith(Path.DirectorySeparatorChar) ? _raiz : _raiz + Path.DirectorySeparatorChar;
        if (!completa.StartsWith(raizConSeparador, StringComparison.Ordinal))
            throw new UnauthorizedAccessException("Ruta de comprobante no válida.");
        return completa;
    }
}

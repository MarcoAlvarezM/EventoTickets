using EventoTickets.Web.Abstracciones.Interfaces;
using EventoTickets.Web.Abstracciones.Modelos;
using EventoTickets.Web.Infraestructura;
using Microsoft.AspNetCore.Mvc;

namespace EventoTickets.Web.Pages.MisEntradas;

/// <summary>
/// MÓDULO 4 · HU-18 Boleto digital: muestra la entrada con su QR (PNG generado por el API) y permite descargarlo.
/// El API solo entrega la entrada a su titular o al personal autorizado.
/// </summary>
public class DetalleModel : PaginaBase
{
    private readonly IEntradaServicio _entradas;

    public DetalleModel(IEntradaServicio entradas) => _entradas = entradas;

    [BindProperty(SupportsGet = true)] public int Id { get; set; }
    public EntradaResponse Entrada { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var resultado = await _entradas.ObtenerAsync(Id);
        if (!resultado.Exito || resultado.Datos is null) return await ResultadoFallidoAsync(resultado, "/MisEntradas/Index");
        Entrada = resultado.Datos;
        return Page();
    }

    /// <summary>Proxy autenticado de la imagen QR (el navegador nunca recibe el token).</summary>
    public async Task<IActionResult> OnGetQrAsync(bool descargar = false)
    {
        var archivo = await _entradas.DescargarQrAsync(Id);
        if (!archivo.Exito || archivo.Datos is null) return NotFound();
        Response.Headers.CacheControl = "private, no-store";
        return descargar
            ? File(archivo.Datos.Contenido, "image/png", $"entrada-{Id}.png")
            : File(archivo.Datos.Contenido, "image/png");
    }
}

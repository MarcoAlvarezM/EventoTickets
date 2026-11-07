using EventoTickets.Abstracciones.Configuracion;
using EventoTickets.Abstracciones.Excepciones;
using EventoTickets.Abstracciones.Interfaces.Reglas;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace EventoTickets.Reglas;


public class PagoReglas : IPagoReglas
{
    private readonly ConfiguracionAlmacenamiento _almacenamiento;

    public PagoReglas(IOptions<ConfiguracionAlmacenamiento> almacenamiento) => _almacenamiento = almacenamiento.Value;

    public void ValidarArchivo(IFormFile? archivo)
    {
        if (archivo is null || archivo.Length == 0)
            throw new ReglaNegocioException("Adjunte el comprobante de pago.");
        if (archivo.Length > _almacenamiento.TamanoMaximoMB * 1024L * 1024L)
            throw new ReglaNegocioException($"El comprobante no puede superar {_almacenamiento.TamanoMaximoMB} MB.");

        var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
        if (!_almacenamiento.ExtensionesPermitidas.Contains(extension))
            throw new ReglaNegocioException($"Formato no permitido. Use: {string.Join(", ", _almacenamiento.ExtensionesPermitidas)}.");
    }

    public void ValidarMonto(decimal montoReportado, decimal importeTotal)
    {
        if (montoReportado < importeTotal)
            throw new ReglaNegocioException($"El monto reportado no cubre el total del pedido (₡{importeTotal:N2}).");
    }
}

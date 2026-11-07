using EventoTickets.Abstracciones.Modelos;
using Microsoft.AspNetCore.Http;

namespace EventoTickets.Abstracciones.Interfaces.Servicios;

/// <summary>Gestión de archivos de comprobantes fuera de wwwroot, con huella SHA-256 (HU-15).</summary>
public interface IServicioArchivosComprobantes
{
    Task<ComprobanteGuardado> GuardarAsync(IFormFile archivo);
    Task<ArchivoComprobante?> ObtenerAsync(string rutaRelativa);
    Task EliminarAsync(string rutaRelativa);
}

/// <summary>Generación de la imagen QR de cada entrada digital (HU-18).</summary>
public interface IServicioQr
{
    byte[] GenerarPng(string contenido, int pixelesPorModulo = 10);
}

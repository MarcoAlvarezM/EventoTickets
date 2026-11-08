using EventoTickets.Abstracciones.Interfaces.Servicios;
using QRCoder;

namespace EventoTickets.Servicios;

/// <summary>QR en PNG con QRCoder (PngByteQRCode no depende de System.Drawing: funciona en Windows, macOS y Linux).</summary>
public class ServicioQr : IServicioQr
{
    public byte[] GenerarPng(string contenido, int pixelesPorModulo = 10)
    {
        using var generador = new QRCodeGenerator();
        using var datos = generador.CreateQrCode(contenido, QRCodeGenerator.ECCLevel.Q);
        var png = new PngByteQRCode(datos);
        return png.GetGraphic(pixelesPorModulo);
    }
}

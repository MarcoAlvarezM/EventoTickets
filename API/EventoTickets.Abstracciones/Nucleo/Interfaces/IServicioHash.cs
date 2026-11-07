namespace EventoTickets.Abstracciones.Interfaces.Servicios;

/// <summary>Huellas SHA-256 (comprobantes de pago y datos criptográficos del proyecto).</summary>
public interface IServicioHash
{
    string CalcularSha256(byte[] contenido);
    string CalcularSha256(string texto);
}

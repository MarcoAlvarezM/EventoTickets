using System.Security.Cryptography;
using System.Text;
using EventoTickets.Abstracciones.Interfaces.Servicios;

namespace EventoTickets.Servicios;

public class ServicioHash : IServicioHash
{
    public string CalcularSha256(byte[] contenido) => Convert.ToHexString(SHA256.HashData(contenido)).ToLowerInvariant();

    public string CalcularSha256(string texto) => CalcularSha256(Encoding.UTF8.GetBytes(texto));
}

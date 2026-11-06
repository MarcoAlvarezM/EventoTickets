using System.Security.Cryptography;
using EventoTickets.Abstracciones.Interfaces.Reglas;

namespace EventoTickets.Reglas;


/// <summary>
/// PBKDF2-SHA256 con sal aleatoria de 16 bytes y 100 000 iteraciones.
/// Formato almacenado: PBKDF2-SHA256$iteraciones$salBase64$hashBase64
/// </summary>
public class PasswordReglas : IPasswordReglas
{
    private const string Prefijo = "PBKDF2-SHA256";
    private const int Iteraciones = 100_000;
    private const int TamanoSal = 16;
    private const int TamanoHash = 32;

    public string GenerarHash(string password)
    {
        var sal = RandomNumberGenerator.GetBytes(TamanoSal);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, sal, Iteraciones, HashAlgorithmName.SHA256, TamanoHash);
        return $"{Prefijo}${Iteraciones}${Convert.ToBase64String(sal)}${Convert.ToBase64String(hash)}";
    }

    public bool Verificar(string password, string hashAlmacenado)
    {
        var partes = hashAlmacenado.Split('$');
        if (partes.Length != 4 || partes[0] != Prefijo || !int.TryParse(partes[1], out var iteraciones))
            return false;

        try
        {
            var sal = Convert.FromBase64String(partes[2]);
            var esperado = Convert.FromBase64String(partes[3]);
            var calculado = Rfc2898DeriveBytes.Pbkdf2(password, sal, iteraciones, HashAlgorithmName.SHA256, esperado.Length);
            return CryptographicOperations.FixedTimeEquals(calculado, esperado);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}

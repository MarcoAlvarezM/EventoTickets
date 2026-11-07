using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EventoTickets.Abstracciones.Configuracion;
using EventoTickets.Abstracciones.Interfaces.Servicios;
using EventoTickets.Abstracciones.Modelos;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace EventoTickets.Servicios;

public class ServicioTokenJwt : IServicioTokenJwt
{
    private readonly ConfiguracionJwt _jwt;

    public ServicioTokenJwt(IOptions<ConfiguracionJwt> jwt) => _jwt = jwt.Value;

    public LoginResponse Generar(UsuarioResponse usuario)
    {
        var expira = DateTime.UtcNow.AddMinutes(_jwt.MinutosExpiracion);
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.UsuarioId.ToString()),
            new Claim(ClaimTypes.Name, usuario.NombreCompleto),
            new Claim(ClaimTypes.Email, usuario.CorreoElectronico),
            new Claim(ClaimTypes.Role, usuario.Rol),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        var credenciales = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Clave)), SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(_jwt.Emisor, _jwt.Audiencia, claims, expires: expira, signingCredentials: credenciales);

        return new LoginResponse
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            ExpiraUtc = expira,
            UsuarioId = usuario.UsuarioId,
            NombreCompleto = usuario.NombreCompleto,
            CorreoElectronico = usuario.CorreoElectronico,
            Rol = usuario.Rol
        };
    }
}

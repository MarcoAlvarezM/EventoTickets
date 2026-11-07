using System.Security.Claims;
using EventoTickets.Web.Abstracciones.Modelos;
using EventoTickets.Web.Infraestructura;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace EventoTickets.Web.Pages.Cuenta;

/// <summary>Crea la cookie de sesión a partir de la respuesta de login del API.</summary>
internal static class SesionHelper
{
    public static Task IniciarAsync(HttpContext contexto, LoginResponse sesion, bool recordar)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, sesion.UsuarioId.ToString()),
            new(ClaimTypes.Name, sesion.NombreCompleto),
            new(ClaimTypes.Email, sesion.CorreoElectronico),
            new(ClaimTypes.Role, sesion.Rol),
            new(ReclamosSesion.Token, sesion.Token)
        };
        var identidad = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        return contexto.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identidad),
            new AuthenticationProperties { IsPersistent = recordar, ExpiresUtc = sesion.ExpiraUtc });
    }
}

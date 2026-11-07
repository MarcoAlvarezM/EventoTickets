using System.Security.Claims;
using EventoTickets.Abstracciones.Constantes;
using EventoTickets.Abstracciones.Excepciones;

namespace EventoTickets.API.Seguridad;


public static class ClaimsPrincipalExtensiones
{
    public static int ObtenerUsuarioId(this ClaimsPrincipal usuario) =>
        int.TryParse(usuario.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
            ? id
            : throw new AutenticacionException("El token no contiene el identificador del usuario.");

    public static bool EsPersonal(this ClaimsPrincipal usuario) =>
        usuario.Identity?.IsAuthenticated == true && Roles.EsPersonal(usuario.FindFirstValue(ClaimTypes.Role));
}

using EventoTickets.Web.Abstracciones.Modelos;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EventoTickets.Web.Infraestructura;

/// <summary>Base de las páginas: mensajes flash y traslado de errores del API al ModelState.</summary>
public abstract class PaginaBase : PageModel
{
    [TempData] public string? MensajeExito { get; set; }
    [TempData] public string? MensajeError { get; set; }

    /// <summary>Copia los errores de validación del API al ModelState (con prefijo del modelo enlazado).</summary>
    protected void AgregarErrores<T>(ResultadoApi<T> resultado, string? prefijo = null)
    {
        foreach (var (campo, mensajes) in resultado.Errores)
        {
            var clave = string.IsNullOrEmpty(prefijo) ? campo : $"{prefijo}.{campo}";
            foreach (var mensaje in mensajes) ModelState.AddModelError(clave, mensaje);
        }
        if (resultado.Errores.Count == 0 && !string.IsNullOrEmpty(resultado.Mensaje))
            ModelState.AddModelError(string.Empty, resultado.Mensaje);
    }

    /// <summary>Respuesta estándar cuando una lectura del API falla.</summary>
    protected async Task<IActionResult> ResultadoFallidoAsync<T>(ResultadoApi<T> resultado, string paginaRetorno = "/Index")
    {
        if (resultado.NoAutenticado)
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            MensajeError = resultado.Mensaje;
            return RedirectToPage("/Cuenta/Login", new { returnUrl = Request.Path + Request.QueryString });
        }
        if (resultado.CodigoEstado == 403) return RedirectToPage("/Cuenta/AccesoDenegado");
        if (resultado.NoEncontrado) return NotFound();
        MensajeError = resultado.Mensaje;
        return RedirectToPage(paginaRetorno);
    }

    protected int UsuarioActualId =>
        int.TryParse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out var id) ? id : 0;
}

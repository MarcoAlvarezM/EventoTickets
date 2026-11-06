using EventoTickets.Web.Infraestructura;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace EventoTickets.Web.Pages.Cuenta;

public class LogoutModel : PaginaBase
{
    public IActionResult OnGet() => RedirectToPage("/Index");

    public async Task<IActionResult> OnPostAsync()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        MensajeExito = "Cerraste sesión correctamente.";
        return RedirectToPage("/Index");
    }
}

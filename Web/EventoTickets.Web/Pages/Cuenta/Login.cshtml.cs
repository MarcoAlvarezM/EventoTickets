using EventoTickets.Web.Abstracciones.Constantes;
using EventoTickets.Web.Abstracciones.Interfaces;
using EventoTickets.Web.Abstracciones.Modelos;
using EventoTickets.Web.Infraestructura;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventoTickets.Web.Pages.Cuenta;

[AllowAnonymous]
public class LoginModel : PaginaBase
{
    private readonly IAutenticacionServicio _autenticacion;

    public LoginModel(IAutenticacionServicio autenticacion) => _autenticacion = autenticacion;

    [BindProperty] public LoginRequest Entrada { get; set; } = new();
    [BindProperty] public bool Recordar { get; set; }
    [BindProperty(SupportsGet = true)] public string? ReturnUrl { get; set; }

    public IActionResult OnGet() => User.Identity?.IsAuthenticated == true ? Redirigir(null) : Page();

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var resultado = await _autenticacion.LoginAsync(Entrada);
        if (!resultado.Exito || resultado.Datos is null)
        {
            AgregarErrores(resultado, nameof(Entrada));
            return Page();
        }

        await SesionHelper.IniciarAsync(HttpContext, resultado.Datos, Recordar);
        return Redirigir(resultado.Datos.Rol);
    }

    private IActionResult Redirigir(string? rol)
    {
        if (!string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl)) return LocalRedirect(ReturnUrl);
        rol ??= User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
        return Roles.EsPersonal(rol) ? RedirectToPage("/Admin/Index") : RedirectToPage("/Index");
    }
}

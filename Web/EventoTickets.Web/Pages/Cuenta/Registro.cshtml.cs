using EventoTickets.Web.Abstracciones.Interfaces;
using EventoTickets.Web.Abstracciones.Modelos;
using EventoTickets.Web.Infraestructura;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventoTickets.Web.Pages.Cuenta;

[AllowAnonymous]
public class RegistroModel : PaginaBase
{
    private readonly IAutenticacionServicio _autenticacion;

    public RegistroModel(IAutenticacionServicio autenticacion) => _autenticacion = autenticacion;

    [BindProperty] public RegistroCompradorRequest Entrada { get; set; } = new();
    [BindProperty(SupportsGet = true)] public string? ReturnUrl { get; set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var resultado = await _autenticacion.RegistrarAsync(Entrada);
        if (!resultado.Exito || resultado.Datos is null)
        {
            AgregarErrores(resultado, nameof(Entrada));
            return Page();
        }

        await SesionHelper.IniciarAsync(HttpContext, resultado.Datos, false);
        MensajeExito = $"¡Bienvenido(a), {resultado.Datos.NombreCompleto}! Tu cuenta de comprador está lista.";
        return !string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl) ? LocalRedirect(ReturnUrl) : RedirectToPage("/Index");
    }
}

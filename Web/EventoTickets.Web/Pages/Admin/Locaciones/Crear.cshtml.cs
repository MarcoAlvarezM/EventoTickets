using EventoTickets.Web.Abstracciones.Interfaces;
using EventoTickets.Web.Abstracciones.Modelos;
using EventoTickets.Web.Infraestructura;
using Microsoft.AspNetCore.Mvc;

namespace EventoTickets.Web.Pages.Admin.Locaciones;

public class CrearModel : PaginaBase
{
    private readonly ILocacionServicio _servicio;

    public CrearModel(ILocacionServicio servicio) => _servicio = servicio;

    [BindProperty] public LocacionRequest Entrada { get; set; } = new();

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        var resultado = await _servicio.CrearAsync(Entrada);
        if (!resultado.Exito)
        {
            AgregarErrores(resultado, nameof(Entrada));
            return Page();
        }
        MensajeExito = $"«{resultado.Datos?.Nombre}» se creó correctamente.";
        return RedirectToPage("Index");
    }
}

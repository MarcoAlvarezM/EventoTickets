using EventoTickets.Web.Abstracciones.Interfaces;
using EventoTickets.Web.Abstracciones.Modelos;
using EventoTickets.Web.Infraestructura;
using Microsoft.AspNetCore.Mvc;

namespace EventoTickets.Web.Pages.Admin.Categorias;

public class CrearModel : PaginaBase
{
    private readonly ICategoriaServicio _servicio;

    public CrearModel(ICategoriaServicio servicio) => _servicio = servicio;

    [BindProperty] public CategoriaRequest Entrada { get; set; } = new();

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

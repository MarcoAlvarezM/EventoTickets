using EventoTickets.Web.Abstracciones.Interfaces;
using EventoTickets.Web.Abstracciones.Modelos;
using EventoTickets.Web.Infraestructura;
using Microsoft.AspNetCore.Mvc;

namespace EventoTickets.Web.Pages.Admin.Categorias;

public class EditarModel : PaginaBase
{
    private readonly ICategoriaServicio _servicio;

    public EditarModel(ICategoriaServicio servicio) => _servicio = servicio;

    [BindProperty(SupportsGet = true)] public int Id { get; set; }
    [BindProperty] public CategoriaRequest Entrada { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var resultado = await _servicio.ObtenerAsync(Id);
        if (!resultado.Exito || resultado.Datos is null) return await ResultadoFallidoAsync(resultado, "/Admin/Categorias/Index");
        var r = resultado.Datos;
        Entrada = new CategoriaRequest { Nombre = r.Nombre, Descripcion = r.Descripcion, Activo = r.Activo };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        var resultado = await _servicio.ActualizarAsync(Id, Entrada);
        if (!resultado.Exito)
        {
            if (resultado.NoEncontrado) return NotFound();
            AgregarErrores(resultado, nameof(Entrada));
            return Page();
        }
        MensajeExito = "Cambios guardados.";
        return RedirectToPage("Index");
    }
}

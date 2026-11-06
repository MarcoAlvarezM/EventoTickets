using EventoTickets.Web.Abstracciones.Interfaces;
using EventoTickets.Web.Abstracciones.Modelos;
using EventoTickets.Web.Infraestructura;
using Microsoft.AspNetCore.Mvc;

namespace EventoTickets.Web.Pages.Admin.Categorias;

public class EliminarModel : PaginaBase
{
    private readonly ICategoriaServicio _servicio;

    public EliminarModel(ICategoriaServicio servicio) => _servicio = servicio;

    [BindProperty(SupportsGet = true)] public int Id { get; set; }
    public CategoriaResponse Registro { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var resultado = await _servicio.ObtenerAsync(Id);
        if (!resultado.Exito || resultado.Datos is null) return await ResultadoFallidoAsync(resultado, "/Admin/Categorias/Index");
        Registro = resultado.Datos;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var resultado = await _servicio.EliminarAsync(Id);
        if (resultado.Exito) MensajeExito = resultado.Datos?.Mensaje;
        else MensajeError = resultado.Mensaje;
        return RedirectToPage("Index");
    }
}

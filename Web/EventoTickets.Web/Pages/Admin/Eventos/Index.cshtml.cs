using EventoTickets.Web.Abstracciones.Interfaces;
using EventoTickets.Web.Abstracciones.Modelos;
using EventoTickets.Web.Infraestructura;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EventoTickets.Web.Pages.Admin.Eventos;

public class IndexModel : PaginaBase
{
    private readonly IEventoServicio _eventos;
    private readonly ICategoriaServicio _categorias;

    public IndexModel(IEventoServicio eventos, ICategoriaServicio categorias)
    {
        _eventos = eventos;
        _categorias = categorias;
    }

    [BindProperty(SupportsGet = true)] public FiltroEventos Filtro { get; set; } = new();
    [BindProperty(SupportsGet = true)] public string Orden { get; set; } = "fecha";
    public List<EventoResponse> Eventos { get; private set; } = new();
    public SelectList Categorias { get; private set; } = new(Array.Empty<object>());

    public async Task<IActionResult> OnGetAsync()
    {
        var resultado = await _eventos.ListarAsync(Filtro);
        if (!resultado.Exito) return await ResultadoFallidoAsync(resultado, "/Admin/Index");
        var lista = resultado.Datos ?? new();
        Eventos = (Orden switch
        {
            "nombre" => lista.OrderBy(e => e.Nombre),
            "ventas" => lista.OrderByDescending(e => e.EntradasVendidas),
            _ => lista.OrderBy(e => e.FechaEvento)
        }).ToList();
        var categorias = await _categorias.ListarAsync(false);
        Categorias = new SelectList(categorias.Datos ?? new(), nameof(CategoriaResponse.CategoriaId), nameof(CategoriaResponse.Nombre));
        return Page();
    }

    public async Task<IActionResult> OnPostEstadoAsync(int id, string estado)
    {
        var resultado = await _eventos.CambiarEstadoAsync(id, estado);
        if (resultado.Exito) MensajeExito = $"«{resultado.Datos?.Nombre}» ahora está {estado}.";
        else MensajeError = resultado.Mensaje;
        return RedirectToPage();
    }
}

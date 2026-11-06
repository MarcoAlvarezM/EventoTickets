using EventoTickets.Web.Abstracciones.Interfaces;
using EventoTickets.Web.Abstracciones.Modelos;
using EventoTickets.Web.Infraestructura;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EventoTickets.Web.Pages.Admin.Reportes;

public class IndexModel : PaginaBase
{
    private readonly IReporteServicio _reportes;
    private readonly ICategoriaServicio _categorias;

    public IndexModel(IReporteServicio reportes, ICategoriaServicio categorias)
    {
        _reportes = reportes;
        _categorias = categorias;
    }

    [BindProperty(SupportsGet = true)] public FiltroReporteVentas Filtro { get; set; } = new();
    public List<VentaEventoResponse> Ventas { get; private set; } = new();
    public SelectList Categorias { get; private set; } = new(Array.Empty<object>());

    public async Task<IActionResult> OnGetAsync()
    {
        var resultado = await _reportes.VentasPorEventoAsync(Filtro);
        if (!resultado.Exito)
        {
            if (resultado.NoAutenticado) return await ResultadoFallidoAsync(resultado);
            AgregarErrores(resultado);
        }
        Ventas = resultado.Datos ?? new();
        Categorias = new SelectList((await _categorias.ListarAsync(false)).Datos ?? new(), nameof(CategoriaResponse.CategoriaId), nameof(CategoriaResponse.Nombre));
        return Page();
    }
}

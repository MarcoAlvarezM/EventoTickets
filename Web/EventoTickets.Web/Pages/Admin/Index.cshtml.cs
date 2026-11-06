using EventoTickets.Web.Abstracciones.Interfaces;
using EventoTickets.Web.Abstracciones.Modelos;
using EventoTickets.Web.Infraestructura;
using Microsoft.AspNetCore.Mvc;

namespace EventoTickets.Web.Pages.Admin;

public class IndexModel : PaginaBase
{
    private readonly IReporteServicio _reportes;

    public IndexModel(IReporteServicio reportes) => _reportes = reportes;

    public DashboardResponse Panel { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var resultado = await _reportes.DashboardAsync();
        if (!resultado.Exito || resultado.Datos is null) return await ResultadoFallidoAsync(resultado, "/Index");
        Panel = resultado.Datos;
        return Page();
    }
}

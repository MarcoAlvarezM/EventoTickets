using EventoTickets.Web.Abstracciones.Interfaces;
using EventoTickets.Web.Abstracciones.Modelos;
using EventoTickets.Web.Infraestructura;
using Microsoft.AspNetCore.Mvc;

namespace EventoTickets.Web.Pages.Admin.Entradas;

/// <summary>MÓDULO 4 · Entradas emitidas por evento (consulta del personal): estado, titular e ingreso.</summary>
public class IndexModel : PaginaBase
{
    private readonly IEntradaServicio _entradas;
    private readonly IEventoServicio _eventos;

    public IndexModel(IEntradaServicio entradas, IEventoServicio eventos)
    {
        _entradas = entradas;
        _eventos = eventos;
    }

    [BindProperty(SupportsGet = true)] public int? EventoId { get; set; }
    [BindProperty(SupportsGet = true)] public string? Estado { get; set; }
    public List<EventoResponse> Eventos { get; private set; } = new();
    public List<EntradaResponse> Entradas { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var eventos = await _eventos.ListarAsync(new FiltroEventos());
        if (!eventos.Exito) return await ResultadoFallidoAsync(eventos, "/Admin/Index");
        Eventos = (eventos.Datos ?? new()).OrderByDescending(e => e.FechaEvento).ToList();

        if (EventoId is > 0)
        {
            var resultado = await _entradas.ListarPorEventoAsync(EventoId.Value, string.IsNullOrWhiteSpace(Estado) ? null : Estado);
            if (!resultado.Exito) return await ResultadoFallidoAsync(resultado, "/Admin/Index");
            Entradas = resultado.Datos ?? new();
        }
        return Page();
    }
}

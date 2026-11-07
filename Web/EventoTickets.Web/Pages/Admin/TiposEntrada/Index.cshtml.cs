using EventoTickets.Web.Abstracciones.Interfaces;
using EventoTickets.Web.Abstracciones.Modelos;
using EventoTickets.Web.Infraestructura;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EventoTickets.Web.Pages.Admin.TiposEntrada;

public class IndexModel : PaginaBase
{
    private readonly IEventoServicio _eventos;
    private readonly ITipoEntradaServicio _tipos;

    public IndexModel(IEventoServicio eventos, ITipoEntradaServicio tipos)
    {
        _eventos = eventos;
        _tipos = tipos;
    }

    [BindProperty(SupportsGet = true)] public int? EventoId { get; set; }
    public SelectList Eventos { get; private set; } = new(Array.Empty<object>());
    public EventoResponse? Evento { get; private set; }
    public List<TipoEntradaResponse> Tipos { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var eventos = await _eventos.ListarAsync(new FiltroEventos());
        if (!eventos.Exito) return await ResultadoFallidoAsync(eventos, "/Admin/Index");
        var lista = (eventos.Datos ?? new()).OrderBy(e => e.FechaEvento).ToList();
        Eventos = new SelectList(lista.Select(e => new { e.EventoId, Texto = $"{e.Nombre} · {Formato.Fecha(e.FechaEvento)} ({e.Estado})" }), "EventoId", "Texto", EventoId);

        EventoId ??= lista.FirstOrDefault(e => e.FechaEvento >= DateTime.Today)?.EventoId ?? lista.FirstOrDefault()?.EventoId;
        if (EventoId is int id)
        {
            Evento = lista.FirstOrDefault(e => e.EventoId == id);
            Tipos = (await _tipos.ListarPorEventoAsync(id, false)).Datos ?? new();
        }
        return Page();
    }
}

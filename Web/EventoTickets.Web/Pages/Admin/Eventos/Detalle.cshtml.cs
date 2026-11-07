using EventoTickets.Web.Abstracciones.Interfaces;
using EventoTickets.Web.Abstracciones.Modelos;
using EventoTickets.Web.Infraestructura;
using Microsoft.AspNetCore.Mvc;

namespace EventoTickets.Web.Pages.Admin.Eventos;

public class DetalleModel : PaginaBase
{
    private readonly IEventoServicio _eventos;
    private readonly ITipoEntradaServicio _tipos;

    public DetalleModel(IEventoServicio eventos, ITipoEntradaServicio tipos)
    {
        _eventos = eventos;
        _tipos = tipos;
    }

    [BindProperty(SupportsGet = true)] public int Id { get; set; }
    public EventoResponse Evento { get; private set; } = new();
    public List<TipoEntradaResponse> Tipos { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var evento = await _eventos.ObtenerAsync(Id);
        if (!evento.Exito || evento.Datos is null) return await ResultadoFallidoAsync(evento, "/Admin/Eventos/Index");
        Evento = evento.Datos;
        Tipos = (await _tipos.ListarPorEventoAsync(Id, false)).Datos ?? new();
        return Page();
    }

    public async Task<IActionResult> OnPostEstadoAsync(string estado)
    {
        var resultado = await _eventos.CambiarEstadoAsync(Id, estado);
        if (resultado.Exito) MensajeExito = $"El evento ahora está {estado}.";
        else MensajeError = resultado.Mensaje;
        return RedirectToPage(new { id = Id });
    }
}

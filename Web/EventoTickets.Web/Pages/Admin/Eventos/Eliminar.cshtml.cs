using EventoTickets.Web.Abstracciones.Interfaces;
using EventoTickets.Web.Abstracciones.Modelos;
using EventoTickets.Web.Infraestructura;
using Microsoft.AspNetCore.Mvc;

namespace EventoTickets.Web.Pages.Admin.Eventos;

public class EliminarModel : PaginaBase
{
    private readonly IEventoServicio _eventos;

    public EliminarModel(IEventoServicio eventos) => _eventos = eventos;

    [BindProperty(SupportsGet = true)] public int Id { get; set; }
    public EventoResponse Evento { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var resultado = await _eventos.ObtenerAsync(Id);
        if (!resultado.Exito || resultado.Datos is null) return await ResultadoFallidoAsync(resultado, "/Admin/Eventos/Index");
        Evento = resultado.Datos;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var resultado = await _eventos.EliminarAsync(Id);
        if (resultado.Exito)
        {
            MensajeExito = resultado.Datos?.Mensaje;
            return RedirectToPage("Index");
        }
        // El evento tiene pedidos: la alternativa es cancelarlo (desde el detalle)
        MensajeError = resultado.Mensaje;
        return RedirectToPage("Detalle", new { id = Id });
    }
}

using EventoTickets.Web.Abstracciones.Interfaces;
using EventoTickets.Web.Abstracciones.Modelos;
using EventoTickets.Web.Infraestructura;
using Microsoft.AspNetCore.Mvc;

namespace EventoTickets.Web.Pages.Admin.TiposEntrada;

public class CrearModel : PaginaBase
{
    private readonly IEventoServicio _eventos;
    private readonly ITipoEntradaServicio _tipos;

    public CrearModel(IEventoServicio eventos, ITipoEntradaServicio tipos)
    {
        _eventos = eventos;
        _tipos = tipos;
    }

    [BindProperty(SupportsGet = true)] public int EventoId { get; set; }
    [BindProperty] public TipoEntradaRequest Entrada { get; set; } = new() { ColorHex = "#0051D5", CantidadInicial = 100 };
    public EventoResponse Evento { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync() => await CargarAsync() ?? Page();

    public async Task<IActionResult> OnPostAsync()
    {
        var carga = await CargarAsync();
        if (carga is not null) return carga;
        Entrada.EventoId = EventoId;
        ModelState.Remove($"{nameof(Entrada)}.{nameof(Entrada.EventoId)}");
        if (!ModelState.IsValid) return Page();

        var resultado = await _tipos.CrearAsync(Entrada);
        if (!resultado.Exito)
        {
            AgregarErrores(resultado, nameof(Entrada));
            return Page();
        }
        MensajeExito = $"Tipo de entrada «{resultado.Datos?.Nombre}» creado.";
        return RedirectToPage("Index", new { eventoId = EventoId });
    }

    private async Task<IActionResult?> CargarAsync()
    {
        var evento = await _eventos.ObtenerAsync(EventoId);
        if (!evento.Exito || evento.Datos is null) return await ResultadoFallidoAsync(evento, "/Admin/TiposEntrada/Index");
        Evento = evento.Datos;
        return null;
    }
}

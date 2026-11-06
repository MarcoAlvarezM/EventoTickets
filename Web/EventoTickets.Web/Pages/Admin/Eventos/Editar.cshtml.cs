using EventoTickets.Web.Abstracciones.Interfaces;
using EventoTickets.Web.Abstracciones.Modelos;
using EventoTickets.Web.Infraestructura;
using Microsoft.AspNetCore.Mvc;

namespace EventoTickets.Web.Pages.Admin.Eventos;

public class EditarModel : PaginaBase
{
    private readonly IEventoServicio _eventos;
    private readonly ICategoriaServicio _categorias;
    private readonly ILocacionServicio _locaciones;

    public EditarModel(IEventoServicio eventos, ICategoriaServicio categorias, ILocacionServicio locaciones)
    {
        _eventos = eventos;
        _categorias = categorias;
        _locaciones = locaciones;
    }

    [BindProperty(SupportsGet = true)] public int Id { get; set; }
    [BindProperty] public EventoRequest Entrada { get; set; } = new();
    public EventoResponse Evento { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var resultado = await _eventos.ObtenerAsync(Id);
        if (!resultado.Exito || resultado.Datos is null) return await ResultadoFallidoAsync(resultado, "/Admin/Eventos/Index");
        Evento = resultado.Datos;
        Entrada = new EventoRequest
        {
            CodigoReferencia = Evento.CodigoReferencia, CategoriaId = Evento.CategoriaId, LocacionId = Evento.LocacionId,
            Nombre = Evento.Nombre, DescripcionCorta = Evento.DescripcionCorta, DescripcionCompleta = Evento.DescripcionCompleta,
            Organizador = Evento.Organizador, ImagenUrl = Evento.ImagenUrl, FechaEvento = Evento.FechaEvento, HoraEvento = Evento.HoraEvento
        };
        await Catalogos.CargarAsync(ViewData, _categorias, _locaciones);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await Catalogos.CargarAsync(ViewData, _categorias, _locaciones);
        var actual = await _eventos.ObtenerAsync(Id);
        if (!actual.Exito || actual.Datos is null) return await ResultadoFallidoAsync(actual, "/Admin/Eventos/Index");
        Evento = actual.Datos;
        if (!ModelState.IsValid) return Page();

        Entrada.CodigoReferencia = string.IsNullOrWhiteSpace(Entrada.CodigoReferencia) ? null : Entrada.CodigoReferencia.Trim().ToUpperInvariant();
        var resultado = await _eventos.ActualizarAsync(Id, Entrada);
        if (!resultado.Exito)
        {
            AgregarErrores(resultado, nameof(Entrada));
            return Page();
        }
        MensajeExito = "Evento actualizado.";
        return RedirectToPage("Detalle", new { id = Id });
    }
}

using EventoTickets.Web.Abstracciones.Interfaces;
using EventoTickets.Web.Abstracciones.Modelos;
using EventoTickets.Web.Infraestructura;
using Microsoft.AspNetCore.Mvc;

namespace EventoTickets.Web.Pages.Admin.Eventos;

public class CrearModel : PaginaBase
{
    private readonly IEventoServicio _eventos;
    private readonly ICategoriaServicio _categorias;
    private readonly ILocacionServicio _locaciones;

    public CrearModel(IEventoServicio eventos, ICategoriaServicio categorias, ILocacionServicio locaciones)
    {
        _eventos = eventos;
        _categorias = categorias;
        _locaciones = locaciones;
    }

    [BindProperty] public EventoRequest Entrada { get; set; } = new() { FechaEvento = DateTime.Today.AddDays(30), HoraEvento = new TimeSpan(19, 0, 0) };

    public Task OnGetAsync() => Catalogos.CargarAsync(ViewData, _categorias, _locaciones);

    public async Task<IActionResult> OnPostAsync()
    {
        await Catalogos.CargarAsync(ViewData, _categorias, _locaciones);
        if (!ModelState.IsValid) return Page();
        Entrada.CodigoReferencia = string.IsNullOrWhiteSpace(Entrada.CodigoReferencia) ? null : Entrada.CodigoReferencia.Trim().ToUpperInvariant();
        var resultado = await _eventos.CrearAsync(Entrada);
        if (!resultado.Exito || resultado.Datos is null)
        {
            AgregarErrores(resultado, nameof(Entrada));
            return Page();
        }
        MensajeExito = "Evento creado como Borrador. Agregue los tipos de entrada y luego publíquelo.";
        return RedirectToPage("/Admin/TiposEntrada/Index", new { eventoId = resultado.Datos.EventoId });
    }
}

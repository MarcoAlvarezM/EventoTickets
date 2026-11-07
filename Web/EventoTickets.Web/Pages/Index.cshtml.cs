using EventoTickets.Web.Abstracciones.Constantes;
using EventoTickets.Web.Abstracciones.Interfaces;
using EventoTickets.Web.Abstracciones.Modelos;
using EventoTickets.Web.Infraestructura;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EventoTickets.Web.Pages;

[AllowAnonymous]
public class IndexModel : PaginaBase
{
    private const int TamanoPagina = 6;
    private readonly IEventoServicio _eventos;
    private readonly ICategoriaServicio _categorias;
    private readonly ILocacionServicio _locaciones;

    public IndexModel(IEventoServicio eventos, ICategoriaServicio categorias, ILocacionServicio locaciones)
    {
        _eventos = eventos;
        _categorias = categorias;
        _locaciones = locaciones;
    }

    [BindProperty(SupportsGet = true)] public string? Busqueda { get; set; }
    [BindProperty(SupportsGet = true)] public int? CategoriaId { get; set; }
    [BindProperty(SupportsGet = true)] public string? Ciudad { get; set; }
    [BindProperty(SupportsGet = true)] public DateTime? FechaDesde { get; set; }
    [BindProperty(SupportsGet = true)] public DateTime? FechaHasta { get; set; }
    [BindProperty(SupportsGet = true)] public string Orden { get; set; } = "fecha";
    [BindProperty(SupportsGet = true)] public int Pagina { get; set; } = 1;

    public List<EventoResponse> Eventos { get; private set; } = new();
    public int TotalEventos { get; private set; }
    public int TotalPaginas { get; private set; }
    public SelectList Categorias { get; private set; } = new(Array.Empty<object>());
    public SelectList Ciudades { get; private set; } = new(Array.Empty<object>());
    public string? ErrorCarga { get; private set; }

    public async Task OnGetAsync()
    {
        var eventosTarea = _eventos.ListarAsync(new FiltroEventos
        {
            // HU-06: cartelera = Publicado o Agotado, vigentes; filtros por nombre, categoría, ciudad y fecha
            Busqueda = Busqueda, CategoriaId = CategoriaId, Ciudad = Ciudad, FechaDesde = FechaDesde, FechaHasta = FechaHasta,
            SoloCartelera = true, SoloVigentes = true
        });
        var categoriasTarea = _categorias.ListarAsync(true);
        var locacionesTarea = _locaciones.ListarAsync(true);
        await Task.WhenAll(eventosTarea, categoriasTarea, locacionesTarea);

        Categorias = new SelectList(categoriasTarea.Result.Datos ?? new(), nameof(CategoriaResponse.CategoriaId), nameof(CategoriaResponse.Nombre));
        Ciudades = new SelectList((locacionesTarea.Result.Datos ?? new()).Select(l => l.Ciudad).Distinct().OrderBy(c => c));

        if (!eventosTarea.Result.Exito)
        {
            ErrorCarga = eventosTarea.Result.Mensaje;
            return;
        }

        // Orden y paginación son de presentación; los datos vienen tal cual del API
        var lista = eventosTarea.Result.Datos ?? new();
        IEnumerable<EventoResponse> ordenados = Orden switch
        {
            "precio" => lista.OrderBy(e => e.PrecioDesde ?? decimal.MaxValue),
            "nombre" => lista.OrderBy(e => e.Nombre),
            _ => lista.OrderBy(e => e.FechaEvento).ThenBy(e => e.HoraEvento)
        };
        TotalEventos = lista.Count;
        TotalPaginas = Math.Max(1, (int)Math.Ceiling(TotalEventos / (double)TamanoPagina));
        Pagina = Math.Clamp(Pagina, 1, TotalPaginas);
        Eventos = ordenados.Skip((Pagina - 1) * TamanoPagina).Take(TamanoPagina).ToList();
    }
}

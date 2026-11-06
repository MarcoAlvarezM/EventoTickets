using EventoTickets.Web.Abstracciones.Constantes;
using EventoTickets.Web.Abstracciones.Interfaces;
using EventoTickets.Web.Abstracciones.Modelos;
using EventoTickets.Web.Infraestructura;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventoTickets.Web.Pages.Eventos;

/// <summary>
/// MÓDULO 2 · Pantallas 2 y 3: detalle del evento y selección de tipos de entrada con disponibilidad
/// en tiempo real (HU-06, HU-11). El formulario se envía a /Compras/Resumen (Módulo 3, HU-12) con el
/// CONTRATO DE FORMULARIO: Pedido.EventoId, Pedido.MetodoPago, Pedido.Lineas[i].TipoEntradaId, Pedido.Lineas[i].Cantidad.
/// </summary>
[AllowAnonymous]
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
    public bool DisponibleParaVenta => Evento.Estado == EstadosEvento.Publicado && Evento.FechaEvento.Date >= DateTime.Today;

    public async Task<IActionResult> OnGetAsync()
    {
        var evento = await _eventos.ObtenerAsync(Id);
        if (!evento.Exito || evento.Datos is null) return await ResultadoFallidoAsync(evento);
        Evento = evento.Datos;
        Tipos = (await _tipos.ListarPorEventoAsync(Id, true)).Datos ?? new();
        return Page();
    }
}

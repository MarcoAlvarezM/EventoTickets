using EventoTickets.Web.Abstracciones.Interfaces;
using EventoTickets.Web.Abstracciones.Modelos;
using EventoTickets.Web.Infraestructura;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventoTickets.Web.Pages.Compras;

/// <summary>
/// MÓDULO 3 · Pantalla 4 — Resumen de orden (HU-12). Recibe la selección de la pantalla 3 (Módulo 2),
/// muestra el desglose calculado por el API (Subtotal, Servicio, IVA, Total) y, al confirmar,
/// crea el pedido que reserva el cupo de forma atómica.
/// </summary>
[AllowAnonymous]
public class ResumenModel : PaginaBase
{
    private readonly IPedidoServicio _pedidos;
    private readonly IEventoServicio _eventos;

    public ResumenModel(IPedidoServicio pedidos, IEventoServicio eventos)
    {
        _pedidos = pedidos;
        _eventos = eventos;
    }

    [BindProperty] public PedidoRequest Pedido { get; set; } = new();
    public CotizacionResponse? Cotizacion { get; private set; }
    public EventoResponse Evento { get; private set; } = new();

    public IActionResult OnGet() => RedirectToPage("/Index");

    /// <summary>Calcula el desglose (sin reservar).</summary>
    public async Task<IActionResult> OnPostAsync()
    {
        var carga = await CargarEventoAsync();
        if (carga is not null) return carga;
        if (!ModelState.IsValid) return Page();

        var resultado = await _pedidos.CotizarAsync(Pedido);
        if (resultado.Exito) Cotizacion = resultado.Datos;
        else AgregarErrores(resultado, nameof(Pedido));
        return Page();
    }

    /// <summary>Confirma: crea el pedido (incrementa CantidadComprometida y fija la reserva).</summary>
    public async Task<IActionResult> OnPostConfirmarAsync()
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            MensajeError = "Inicia sesión o crea una cuenta para reservar tus entradas.";
            return RedirectToPage("/Cuenta/Login", new { returnUrl = Url.Page("/Eventos/Detalle", new { id = Pedido.EventoId }) });
        }
        var carga = await CargarEventoAsync();
        if (carga is not null) return carga;
        if (!ModelState.IsValid) return Page();

        var resultado = await _pedidos.CrearAsync(Pedido);
        if (!resultado.Exito || resultado.Datos is null)
        {
            if (resultado.NoAutenticado) return await ResultadoFallidoAsync(resultado);
            AgregarErrores(resultado, nameof(Pedido));
            var cotizacion = await _pedidos.CotizarAsync(Pedido);
            Cotizacion = cotizacion.Datos;
            return Page();
        }
        MensajeExito = $"Reservamos tus entradas en el pedido {resultado.Datos.CodigoPedido}. Completa el pago antes de que venza la reserva.";
        return RedirectToPage("/Compras/Checkout", new { pedidoId = resultado.Datos.PedidoId });
    }

    private async Task<IActionResult?> CargarEventoAsync()
    {
        var evento = await _eventos.ObtenerAsync(Pedido.EventoId);
        if (!evento.Exito || evento.Datos is null) return await ResultadoFallidoAsync(evento);
        Evento = evento.Datos;
        return null;
    }
}

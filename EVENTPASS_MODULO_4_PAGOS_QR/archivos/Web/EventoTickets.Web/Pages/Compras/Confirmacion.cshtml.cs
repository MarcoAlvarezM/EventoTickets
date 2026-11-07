using EventoTickets.Web.Abstracciones.Interfaces;
using EventoTickets.Web.Abstracciones.Modelos;
using EventoTickets.Web.Infraestructura;
using Microsoft.AspNetCore.Mvc;

namespace EventoTickets.Web.Pages.Compras;

/// <summary>MÓDULO 4 · Estado del pedido: confirmación de pago, verificación pendiente o pedido cerrado.</summary>
public class ConfirmacionModel : PaginaBase
{
    private readonly IPedidoServicio _pedidos;
    private readonly IEntradaServicio _entradas;
    private readonly IPagoServicio _pagos;

    public ConfirmacionModel(IPedidoServicio pedidos, IEntradaServicio entradas, IPagoServicio pagos)
    {
        _pedidos = pedidos;
        _entradas = entradas;
        _pagos = pagos;
    }

    [BindProperty(SupportsGet = true)] public int PedidoId { get; set; }
    public PedidoCompletoResponse Pedido { get; private set; } = new();
    public List<PagoResponse> Pagos { get; private set; } = new();
    public List<EntradaResponse> Entradas { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var pedido = await _pedidos.ObtenerAsync(PedidoId);
        if (!pedido.Exito || pedido.Datos is null) return await ResultadoFallidoAsync(pedido, "/MisPedidos/Index");
        Pedido = pedido.Datos;

        var pagos = await _pagos.ListarPorPedidoAsync(PedidoId);
        Pagos = pagos.Datos ?? new();

        if (Pedido.Estado == Abstracciones.Constantes.EstadosPedido.Pagado)
        {
            var entradas = await _entradas.ListarMiasAsync();
            Entradas = (entradas.Datos ?? new()).Where(e => e.PedidoId == PedidoId).ToList();
        }
        return Page();
    }
}

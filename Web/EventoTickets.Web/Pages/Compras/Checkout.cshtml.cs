using EventoTickets.Web.Abstracciones.Configuracion;
using EventoTickets.Web.Abstracciones.Constantes;
using EventoTickets.Web.Abstracciones.Interfaces;
using EventoTickets.Web.Abstracciones.Modelos;
using EventoTickets.Web.Infraestructura;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace EventoTickets.Web.Pages.Compras;

public class CheckoutModel : PaginaBase
{
    private readonly IPedidoServicio _pedidos;

    public CheckoutModel(IPedidoServicio pedidos, IOptions<ConfiguracionCuentaRecaudadora> cuenta)
    {
        _pedidos = pedidos;
        Cuenta = cuenta.Value;
    }

    [BindProperty(SupportsGet = true)] public int PedidoId { get; set; }
    public PedidoCompletoResponse Pedido { get; private set; } = new();
    public ConfiguracionCuentaRecaudadora Cuenta { get; }

    public async Task<IActionResult> OnGetAsync()
    {
        var resultado = await _pedidos.ObtenerAsync(PedidoId);
        if (!resultado.Exito || resultado.Datos is null) return await ResultadoFallidoAsync(resultado, "/MisPedidos/Index");
        if (resultado.Datos.Estado != EstadosPedido.PendientePago)
            return RedirectToPage("/Compras/Confirmacion", new { pedidoId = PedidoId });
        Pedido = resultado.Datos;
        return Page();
    }

    public async Task<IActionResult> OnPostCancelarAsync()
    {
        var resultado = await _pedidos.CancelarAsync(PedidoId);
        if (resultado.Exito) MensajeExito = "Pedido cancelado. Las entradas reservadas se liberaron.";
        else MensajeError = resultado.Mensaje;
        return RedirectToPage("/MisPedidos/Index");
    }
}

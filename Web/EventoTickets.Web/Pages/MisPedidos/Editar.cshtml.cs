using EventoTickets.Web.Abstracciones.Constantes;
using EventoTickets.Web.Abstracciones.Interfaces;
using EventoTickets.Web.Abstracciones.Modelos;
using EventoTickets.Web.Infraestructura;
using Microsoft.AspNetCore.Mvc;

namespace EventoTickets.Web.Pages.MisPedidos;

/// <summary>UPDATE del CRUD de pedidos: método de pago mientras el pedido está PendientePago.</summary>
public class EditarModel : PaginaBase
{
    private readonly IPedidoServicio _pedidos;

    public EditarModel(IPedidoServicio pedidos) => _pedidos = pedidos;

    [BindProperty(SupportsGet = true)] public int Id { get; set; }
    [BindProperty] public ActualizarMetodoPagoRequest Entrada { get; set; } = new();
    public PedidoCompletoResponse Pedido { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var carga = await CargarAsync();
        if (carga is not null) return carga;
        Entrada.MetodoPago = Pedido.MetodoPagoSeleccionado;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var carga = await CargarAsync();
        if (carga is not null) return carga;
        if (!ModelState.IsValid) return Page();
        var resultado = await _pedidos.ActualizarMetodoPagoAsync(Id, Entrada);
        if (!resultado.Exito)
        {
            AgregarErrores(resultado, nameof(Entrada));
            return Page();
        }
        MensajeExito = "Método de pago actualizado.";
        return RedirectToPage("Detalle", new { id = Id });
    }

    private async Task<IActionResult?> CargarAsync()
    {
        var resultado = await _pedidos.ObtenerAsync(Id);
        if (!resultado.Exito || resultado.Datos is null) return await ResultadoFallidoAsync(resultado, "/MisPedidos/Index");
        Pedido = resultado.Datos;
        if (Pedido.Estado != EstadosPedido.PendientePago)
        {
            MensajeError = "Solo se puede modificar un pedido pendiente de pago.";
            return RedirectToPage("Detalle", new { id = Id });
        }
        return null;
    }
}

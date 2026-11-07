using EventoTickets.Web.Abstracciones.Interfaces;
using EventoTickets.Web.Abstracciones.Modelos;
using EventoTickets.Web.Infraestructura;
using Microsoft.AspNetCore.Mvc;

namespace EventoTickets.Web.Pages.MisPedidos;

/// <summary>HU-13 · Consulta del pedido: código, referencia bancaria, montos, estado y líneas con precio congelado.</summary>
public class DetalleModel : PaginaBase
{
    private readonly IPedidoServicio _pedidos;

    public DetalleModel(IPedidoServicio pedidos) => _pedidos = pedidos;

    [BindProperty(SupportsGet = true)] public int Id { get; set; }
    public PedidoCompletoResponse Pedido { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var resultado = await _pedidos.ObtenerAsync(Id);
        if (!resultado.Exito || resultado.Datos is null) return await ResultadoFallidoAsync(resultado, "/MisPedidos/Index");
        Pedido = resultado.Datos;
        return Page();
    }

    /// <summary>HU-14 · Cancelación manual con confirmación.</summary>
    public async Task<IActionResult> OnPostCancelarAsync()
    {
        var resultado = await _pedidos.CancelarAsync(Id);
        if (resultado.Exito) MensajeExito = resultado.Mensaje ?? "Pedido cancelado.";
        else MensajeError = resultado.Mensaje;
        return RedirectToPage(new { id = Id });
    }
}

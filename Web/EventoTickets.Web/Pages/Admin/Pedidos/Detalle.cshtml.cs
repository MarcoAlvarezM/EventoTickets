using EventoTickets.Web.Abstracciones.Interfaces;
using EventoTickets.Web.Abstracciones.Modelos;
using EventoTickets.Web.Infraestructura;
using Microsoft.AspNetCore.Mvc;

namespace EventoTickets.Web.Pages.Admin.Pedidos;

public class DetalleModel : PaginaBase
{
    private readonly IPedidoServicio _pedidos;

    public DetalleModel(IPedidoServicio pedidos) => _pedidos = pedidos;

    [BindProperty(SupportsGet = true)] public int Id { get; set; }
    public PedidoCompletoResponse Pedido { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var resultado = await _pedidos.ObtenerAsync(Id);
        if (!resultado.Exito || resultado.Datos is null) return await ResultadoFallidoAsync(resultado, "/Admin/Pedidos/Index");
        Pedido = resultado.Datos;
        return Page();
    }

    public async Task<IActionResult> OnPostCancelarAsync()
    {
        var resultado = await _pedidos.CancelarAsync(Id);
        if (resultado.Exito) MensajeExito = "Pedido cancelado; el cupo reservado volvió al inventario.";
        else MensajeError = resultado.Mensaje;
        return RedirectToPage(new { id = Id });
    }
}

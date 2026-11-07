using EventoTickets.Web.Abstracciones.Interfaces;
using EventoTickets.Web.Abstracciones.Modelos;
using EventoTickets.Web.Infraestructura;
using Microsoft.AspNetCore.Mvc;

namespace EventoTickets.Web.Pages.MisPedidos;

public class IndexModel : PaginaBase
{
    private readonly IPedidoServicio _pedidos;

    public IndexModel(IPedidoServicio pedidos) => _pedidos = pedidos;

    [BindProperty(SupportsGet = true)] public string? Estado { get; set; }
    [BindProperty(SupportsGet = true)] public string? Busqueda { get; set; }
    public List<PedidoResponse> Pedidos { get; private set; } = new();
    public Dictionary<string, int> Conteo { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var resultado = await _pedidos.ListarMiosAsync(null);
        if (!resultado.Exito) return await ResultadoFallidoAsync(resultado);

        var todos = resultado.Datos ?? new();
        Conteo = todos.GroupBy(p => p.Estado).ToDictionary(g => g.Key, g => g.Count());
        Conteo[""] = todos.Count;
        Pedidos = todos
            .Where(p => string.IsNullOrEmpty(Estado) || p.Estado == Estado)
            .Where(p => string.IsNullOrWhiteSpace(Busqueda)
                        || p.CodigoPedido.Contains(Busqueda, StringComparison.OrdinalIgnoreCase)
                        || p.EventoNombre.Contains(Busqueda, StringComparison.OrdinalIgnoreCase))
            .ToList();
        return Page();
    }

    public async Task<IActionResult> OnPostCancelarAsync(int id)
    {
        var resultado = await _pedidos.CancelarAsync(id);
        if (resultado.Exito) MensajeExito = "Pedido cancelado y entradas liberadas.";
        else MensajeError = resultado.Mensaje;
        return RedirectToPage(new { Estado });
    }
}

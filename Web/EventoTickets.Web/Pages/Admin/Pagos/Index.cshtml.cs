using EventoTickets.Web.Abstracciones.Interfaces;
using EventoTickets.Web.Abstracciones.Modelos;
using EventoTickets.Web.Infraestructura;
using Microsoft.AspNetCore.Mvc;

namespace EventoTickets.Web.Pages.Admin.Pagos;

public class IndexModel : PaginaBase
{
    private readonly IPagoServicio _pagos;

    public IndexModel(IPagoServicio pagos) => _pagos = pagos;

    [BindProperty(SupportsGet = true)] public string? Estado { get; set; }
    [BindProperty(SupportsGet = true)] public string? Busqueda { get; set; }
    [BindProperty(SupportsGet = true)] public int? PedidoId { get; set; }
    public List<PagoResponse> Pagos { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var resultado = PedidoId is > 0 ? await _pagos.ListarPorPedidoAsync(PedidoId.Value) : await _pagos.ListarAsync(Estado, Busqueda);
        if (!resultado.Exito) return await ResultadoFallidoAsync(resultado, "/Admin/Index");
        Pagos = resultado.Datos ?? new();
        return Page();
    }
}

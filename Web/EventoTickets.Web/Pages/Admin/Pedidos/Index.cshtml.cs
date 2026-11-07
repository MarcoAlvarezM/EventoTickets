using EventoTickets.Web.Abstracciones.Interfaces;
using EventoTickets.Web.Abstracciones.Modelos;
using EventoTickets.Web.Infraestructura;
using Microsoft.AspNetCore.Mvc;

namespace EventoTickets.Web.Pages.Admin.Pedidos;

public class IndexModel : PaginaBase
{
    private readonly IPedidoServicio _pedidos;

    public IndexModel(IPedidoServicio pedidos) => _pedidos = pedidos;

    [BindProperty(SupportsGet = true)] public FiltroPedidos Filtro { get; set; } = new();
    [BindProperty(SupportsGet = true)] public string? Busqueda { get; set; }
    public List<PedidoResponse> Pedidos { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        // La búsqueda rápida de la cabecera llega como ?busqueda=
        Filtro.Busqueda ??= Busqueda;
        var resultado = await _pedidos.ListarAsync(Filtro);
        if (!resultado.Exito) return await ResultadoFallidoAsync(resultado, "/Admin/Index");
        Pedidos = resultado.Datos ?? new();
        return Page();
    }

    /// <summary>HU-14 · Ejecución manual del vencimiento de reservas (además del proceso automático del API).</summary>
    public async Task<IActionResult> OnPostLiberarAsync()
    {
        var resultado = await _pedidos.LiberarVencidosAsync();
        if (resultado.Exito) MensajeExito = $"Reservas vencidas liberadas: {resultado.Datos}.";
        else MensajeError = resultado.Mensaje;
        return RedirectToPage();
    }
}

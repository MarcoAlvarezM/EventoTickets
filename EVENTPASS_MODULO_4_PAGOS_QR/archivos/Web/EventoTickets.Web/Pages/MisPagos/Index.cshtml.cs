using EventoTickets.Web.Abstracciones.Interfaces;
using EventoTickets.Web.Abstracciones.Modelos;
using EventoTickets.Web.Infraestructura;
using Microsoft.AspNetCore.Mvc;

namespace EventoTickets.Web.Pages.MisPagos;

public class IndexModel : PaginaBase
{
    private readonly IPagoServicio _pagos;

    public IndexModel(IPagoServicio pagos) => _pagos = pagos;

    public List<PagoResponse> Pagos { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var resultado = await _pagos.ListarMiosAsync();
        if (!resultado.Exito) return await ResultadoFallidoAsync(resultado);
        Pagos = resultado.Datos ?? new();
        return Page();
    }

    /// <summary>Descarga el comprobante propio a través del API (no hay acceso directo al archivo).</summary>
    public async Task<IActionResult> OnGetComprobanteAsync(int id)
    {
        var archivo = await _pagos.DescargarComprobanteAsync(id);
        if (!archivo.Exito || archivo.Datos is null) return await ResultadoFallidoAsync(archivo, "/MisPagos/Index");
        return File(archivo.Datos.Contenido, archivo.Datos.TipoContenido);
    }
}

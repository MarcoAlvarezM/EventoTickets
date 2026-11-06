using EventoTickets.Web.Abstracciones.Interfaces;
using EventoTickets.Web.Abstracciones.Modelos;
using EventoTickets.Web.Infraestructura;
using Microsoft.AspNetCore.Mvc;

namespace EventoTickets.Web.Pages.Admin.TiposEntrada;

/// <summary>HU-11 · Detalle de un tipo de entrada con su inventario (inicial, comprometido, vendido, disponible).</summary>
public class DetalleModel : PaginaBase
{
    private readonly ITipoEntradaServicio _tipos;

    public DetalleModel(ITipoEntradaServicio tipos) => _tipos = tipos;

    [BindProperty(SupportsGet = true)] public int Id { get; set; }
    public TipoEntradaResponse Tipo { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var resultado = await _tipos.ObtenerAsync(Id);
        if (!resultado.Exito || resultado.Datos is null) return await ResultadoFallidoAsync(resultado, "/Admin/TiposEntrada/Index");
        Tipo = resultado.Datos;
        return Page();
    }
}

using EventoTickets.Web.Abstracciones.Interfaces;
using EventoTickets.Web.Abstracciones.Modelos;
using EventoTickets.Web.Infraestructura;
using Microsoft.AspNetCore.Mvc;

namespace EventoTickets.Web.Pages.MisEntradas;

public class IndexModel : PaginaBase
{
    private readonly IEntradaServicio _entradas;

    public IndexModel(IEntradaServicio entradas) => _entradas = entradas;

    [BindProperty(SupportsGet = true)] public string Vista { get; set; } = "proximos";
    [BindProperty(SupportsGet = true)] public string? Busqueda { get; set; }
    public List<EntradaResponse> Entradas { get; private set; } = new();
    public int Proximas { get; private set; }
    public int Pasadas { get; private set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var resultado = await _entradas.ListarMiasAsync();
        if (!resultado.Exito) return await ResultadoFallidoAsync(resultado);

        var todas = resultado.Datos ?? new();
        bool EsProxima(EntradaResponse e) => e.FechaEvento.Date >= DateTime.Today;
        Proximas = todas.Count(EsProxima);
        Pasadas = todas.Count - Proximas;
        Entradas = todas
            .Where(e => Vista == "pasados" ? !EsProxima(e) : EsProxima(e))
            .Where(e => string.IsNullOrWhiteSpace(Busqueda)
                        || e.EventoNombre.Contains(Busqueda, StringComparison.OrdinalIgnoreCase)
                        || e.CodigoTicket.Contains(Busqueda, StringComparison.OrdinalIgnoreCase))
            .ToList();
        return Page();
    }
}

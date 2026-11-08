using EventoTickets.Web.Abstracciones.Interfaces;
using EventoTickets.Web.Abstracciones.Modelos;
using EventoTickets.Web.Infraestructura;
using Microsoft.AspNetCore.Mvc;

namespace EventoTickets.Web.Pages.Admin.Accesos;

/// <summary>Control de acceso en puerta: registra el ingreso de una entrada (una sola vez).</summary>
public class IndexModel : PaginaBase
{
    private readonly IEntradaServicio _entradas;

    public IndexModel(IEntradaServicio entradas) => _entradas = entradas;

    [BindProperty] public ValidacionAccesoRequest Entrada { get; set; } = new();
    public EntradaResponse? Validada { get; private set; }
    public string? Rechazo { get; private set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        var resultado = await _entradas.RegistrarIngresoAsync(Entrada);
        if (resultado.NoAutenticado) return await ResultadoFallidoAsync(resultado);
        if (resultado.Exito) Validada = resultado.Datos;
        else Rechazo = resultado.Mensaje;

        var puerta = Entrada.PuertaAcceso;
        ModelState.Clear();
        Entrada = new ValidacionAccesoRequest { PuertaAcceso = puerta };
        return Page();
    }
}

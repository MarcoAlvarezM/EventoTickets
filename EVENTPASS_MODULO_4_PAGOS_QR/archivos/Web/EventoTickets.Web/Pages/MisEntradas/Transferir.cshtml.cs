using System.ComponentModel.DataAnnotations;
using EventoTickets.Web.Abstracciones.Constantes;
using EventoTickets.Web.Abstracciones.Interfaces;
using EventoTickets.Web.Abstracciones.Modelos;
using EventoTickets.Web.Infraestructura;
using Microsoft.AspNetCore.Mvc;

namespace EventoTickets.Web.Pages.MisEntradas;

public class TransferirModel : PaginaBase
{
    private readonly IEntradaServicio _entradas;

    public TransferirModel(IEntradaServicio entradas) => _entradas = entradas;

    [BindProperty(SupportsGet = true)] public int Id { get; set; }
    [BindProperty] public TransferenciaEntradaRequest Entrada { get; set; } = new();

    [BindProperty]
    [Range(typeof(bool), "true", "true", ErrorMessage = "Confirme que entiende que el QR actual dejará de ser válido.")]
    public bool Confirmacion { get; set; }

    public EntradaResponse Actual { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var carga = await CargarAsync();
        return carga ?? Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var carga = await CargarAsync();
        if (carga is not null) return carga;
        if (!ModelState.IsValid) return Page();

        var resultado = await _entradas.TransferirAsync(Id, Entrada);
        if (!resultado.Exito)
        {
            if (resultado.NoAutenticado) return await ResultadoFallidoAsync(resultado);
            AgregarErrores(resultado, nameof(Entrada));
            return Page();
        }
        MensajeExito = $"La entrada {resultado.Datos?.CodigoTicket} ahora está a nombre de {resultado.Datos?.NombreAsistente}. Se generó un QR nuevo.";
        return RedirectToPage("/MisEntradas/Index");
    }

    private async Task<IActionResult?> CargarAsync()
    {
        var resultado = await _entradas.ObtenerAsync(Id);
        if (!resultado.Exito || resultado.Datos is null) return await ResultadoFallidoAsync(resultado, "/MisEntradas/Index");
        if (resultado.Datos.Estado != EstadosEntrada.Valida)
        {
            MensajeError = "Solo se pueden transferir entradas activas.";
            return RedirectToPage("/MisEntradas/Index");
        }
        Actual = resultado.Datos;
        return null;
    }
}

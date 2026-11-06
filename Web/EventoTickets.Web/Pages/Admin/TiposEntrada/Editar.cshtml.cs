using EventoTickets.Web.Abstracciones.Interfaces;
using EventoTickets.Web.Abstracciones.Modelos;
using EventoTickets.Web.Infraestructura;
using Microsoft.AspNetCore.Mvc;

namespace EventoTickets.Web.Pages.Admin.TiposEntrada;

public class EditarModel : PaginaBase
{
    private readonly ITipoEntradaServicio _tipos;

    public EditarModel(ITipoEntradaServicio tipos) => _tipos = tipos;

    [BindProperty(SupportsGet = true)] public int Id { get; set; }
    [BindProperty] public TipoEntradaBase Entrada { get; set; } = new();
    public TipoEntradaResponse Tipo { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var carga = await CargarAsync();
        if (carga is not null) return carga;
        Entrada = new TipoEntradaBase { Nombre = Tipo.Nombre, Precio = Tipo.Precio, ColorHex = Tipo.ColorHex, CantidadInicial = Tipo.CantidadInicial, Activo = Tipo.Activo };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var carga = await CargarAsync();
        if (carga is not null) return carga;
        if (!ModelState.IsValid) return Page();
        var resultado = await _tipos.ActualizarAsync(Id, Entrada);
        if (!resultado.Exito)
        {
            AgregarErrores(resultado, nameof(Entrada));
            return Page();
        }
        MensajeExito = "Tipo de entrada actualizado.";
        return RedirectToPage("Index", new { eventoId = Tipo.EventoId });
    }

    private async Task<IActionResult?> CargarAsync()
    {
        var resultado = await _tipos.ObtenerAsync(Id);
        if (!resultado.Exito || resultado.Datos is null) return await ResultadoFallidoAsync(resultado, "/Admin/TiposEntrada/Index");
        Tipo = resultado.Datos;
        return null;
    }
}

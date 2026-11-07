using EventoTickets.Web.Abstracciones.Interfaces;
using EventoTickets.Web.Abstracciones.Modelos;
using EventoTickets.Web.Infraestructura;
using Microsoft.AspNetCore.Mvc;

namespace EventoTickets.Web.Pages.Admin.Categorias;

public class IndexModel : PaginaBase
{
    private readonly ICategoriaServicio _servicio;

    public IndexModel(ICategoriaServicio servicio) => _servicio = servicio;

    [BindProperty(SupportsGet = true)] public string? Busqueda { get; set; }
    [BindProperty(SupportsGet = true)] public bool SoloActivas { get; set; }
    public List<CategoriaResponse> Registros { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var resultado = await _servicio.ListarAsync(SoloActivas);
        if (!resultado.Exito) return await ResultadoFallidoAsync(resultado, "/Admin/Index");
        Registros = (resultado.Datos ?? new())
            .Where(r => string.IsNullOrWhiteSpace(Busqueda) || r.Nombre.Contains(Busqueda, StringComparison.OrdinalIgnoreCase))
            .OrderBy(r => r.Nombre).ToList();
        return Page();
    }
}

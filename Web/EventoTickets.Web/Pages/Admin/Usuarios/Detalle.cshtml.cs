using EventoTickets.Web.Abstracciones.Interfaces;
using EventoTickets.Web.Abstracciones.Modelos;
using EventoTickets.Web.Infraestructura;
using Microsoft.AspNetCore.Mvc;

namespace EventoTickets.Web.Pages.Admin.Usuarios;

/// <summary>HU-03 · Consulta de detalle de una cuenta.</summary>
public class DetalleModel : PaginaBase
{
    private readonly IUsuarioServicio _usuarios;

    public DetalleModel(IUsuarioServicio usuarios) => _usuarios = usuarios;

    [BindProperty(SupportsGet = true)] public int Id { get; set; }
    public UsuarioResponse Usuario { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var resultado = await _usuarios.ObtenerAsync(Id);
        if (!resultado.Exito || resultado.Datos is null) return await ResultadoFallidoAsync(resultado, "/Admin/Usuarios/Index");
        Usuario = resultado.Datos;
        return Page();
    }
}

using EventoTickets.Web.Abstracciones.Interfaces;
using EventoTickets.Web.Abstracciones.Modelos;
using EventoTickets.Web.Infraestructura;
using Microsoft.AspNetCore.Mvc;

namespace EventoTickets.Web.Pages.Admin.Usuarios;

/// <summary>HU-03 · Borrado lógico (Estado = Inactivo) con confirmación.</summary>
public class EliminarModel : PaginaBase
{
    private readonly IUsuarioServicio _usuarios;

    public EliminarModel(IUsuarioServicio usuarios) => _usuarios = usuarios;

    [BindProperty(SupportsGet = true)] public int Id { get; set; }
    public UsuarioResponse Usuario { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var resultado = await _usuarios.ObtenerAsync(Id);
        if (!resultado.Exito || resultado.Datos is null) return await ResultadoFallidoAsync(resultado, "/Admin/Usuarios/Index");
        Usuario = resultado.Datos;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var resultado = await _usuarios.EliminarAsync(Id);
        if (resultado.Exito) MensajeExito = resultado.Datos?.Mensaje;
        else MensajeError = resultado.Mensaje;
        return RedirectToPage("Index");
    }
}

using EventoTickets.Web.Abstracciones.Interfaces;
using EventoTickets.Web.Abstracciones.Modelos;
using EventoTickets.Web.Infraestructura;
using Microsoft.AspNetCore.Mvc;

namespace EventoTickets.Web.Pages.Admin.Usuarios;

public class IndexModel : PaginaBase
{
    private readonly IUsuarioServicio _usuarios;

    public IndexModel(IUsuarioServicio usuarios) => _usuarios = usuarios;

    [BindProperty(SupportsGet = true)] public string? Busqueda { get; set; }
    [BindProperty(SupportsGet = true)] public string? Rol { get; set; }
    [BindProperty(SupportsGet = true)] public string? Estado { get; set; }
    [BindProperty(SupportsGet = true)] public string Orden { get; set; } = "nombre";
    public List<UsuarioResponse> Usuarios { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var resultado = await _usuarios.ListarAsync(Busqueda, Rol, Estado);
        if (!resultado.Exito) return await ResultadoFallidoAsync(resultado, "/Admin/Index");
        var lista = resultado.Datos ?? new();
        Usuarios = (Orden == "registro" ? lista.OrderByDescending(u => u.FechaRegistro) : lista.OrderBy(u => u.NombreCompleto)).ToList();
        return Page();
    }

    public async Task<IActionResult> OnPostEstadoAsync(int id, string estado)
    {
        var resultado = await _usuarios.CambiarEstadoAsync(id, estado);
        if (resultado.Exito) MensajeExito = $"Estado actualizado a {estado}.";
        else MensajeError = resultado.Mensaje;
        return RedirectToPage(new { Busqueda, Rol, Estado });
    }
}

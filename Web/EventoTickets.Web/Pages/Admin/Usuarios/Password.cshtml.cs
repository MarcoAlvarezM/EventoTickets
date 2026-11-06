using EventoTickets.Web.Abstracciones.Interfaces;
using EventoTickets.Web.Abstracciones.Modelos;
using EventoTickets.Web.Infraestructura;
using Microsoft.AspNetCore.Mvc;

namespace EventoTickets.Web.Pages.Admin.Usuarios;

public class PasswordModel : PaginaBase
{
    private readonly IUsuarioServicio _usuarios;

    public PasswordModel(IUsuarioServicio usuarios) => _usuarios = usuarios;

    [BindProperty(SupportsGet = true)] public int Id { get; set; }
    [BindProperty] public CambiarPasswordRequest Entrada { get; set; } = new();
    public UsuarioResponse Usuario { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync() => await CargarAsync() ?? Page();

    public async Task<IActionResult> OnPostAsync()
    {
        var carga = await CargarAsync();
        if (carga is not null) return carga;
        if (!ModelState.IsValid) return Page();
        var resultado = await _usuarios.CambiarPasswordAsync(Id, Entrada);
        if (!resultado.Exito)
        {
            AgregarErrores(resultado, nameof(Entrada));
            return Page();
        }
        MensajeExito = $"Contraseña de {Usuario.NombreCompleto} restablecida.";
        return RedirectToPage("Index");
    }

    private async Task<IActionResult?> CargarAsync()
    {
        var resultado = await _usuarios.ObtenerAsync(Id);
        if (!resultado.Exito || resultado.Datos is null) return await ResultadoFallidoAsync(resultado, "/Admin/Usuarios/Index");
        Usuario = resultado.Datos;
        return null;
    }
}

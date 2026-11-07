using EventoTickets.Web.Abstracciones.Interfaces;
using EventoTickets.Web.Abstracciones.Modelos;
using EventoTickets.Web.Infraestructura;
using Microsoft.AspNetCore.Mvc;

namespace EventoTickets.Web.Pages.Admin.Usuarios;

public class EditarModel : PaginaBase
{
    private readonly IUsuarioServicio _usuarios;

    public EditarModel(IUsuarioServicio usuarios) => _usuarios = usuarios;

    [BindProperty(SupportsGet = true)] public int Id { get; set; }
    [BindProperty] public UsuarioActualizarRequest Entrada { get; set; } = new();
    public UsuarioResponse Usuario { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var resultado = await _usuarios.ObtenerAsync(Id);
        if (!resultado.Exito || resultado.Datos is null) return await ResultadoFallidoAsync(resultado, "/Admin/Usuarios/Index");
        Usuario = resultado.Datos;
        Entrada = new UsuarioActualizarRequest
        {
            TipoDocumento = Usuario.TipoDocumento, NumeroDocumento = Usuario.NumeroDocumento, NombreCompleto = Usuario.NombreCompleto,
            CorreoElectronico = Usuario.CorreoElectronico, Telefono = Usuario.Telefono, Rol = Usuario.Rol, Estado = Usuario.Estado
        };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var actual = await _usuarios.ObtenerAsync(Id);
        if (!actual.Exito || actual.Datos is null) return await ResultadoFallidoAsync(actual, "/Admin/Usuarios/Index");
        Usuario = actual.Datos;
        if (!ModelState.IsValid) return Page();

        var resultado = await _usuarios.ActualizarAsync(Id, Entrada);
        if (!resultado.Exito)
        {
            AgregarErrores(resultado, nameof(Entrada));
            return Page();
        }
        MensajeExito = "Cambios guardados.";
        return RedirectToPage("Index");
    }
}

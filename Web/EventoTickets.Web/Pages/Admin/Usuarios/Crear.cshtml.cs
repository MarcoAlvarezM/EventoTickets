using EventoTickets.Web.Abstracciones.Interfaces;
using EventoTickets.Web.Abstracciones.Modelos;
using EventoTickets.Web.Infraestructura;
using Microsoft.AspNetCore.Mvc;

namespace EventoTickets.Web.Pages.Admin.Usuarios;

public class CrearModel : PaginaBase
{
    private readonly IUsuarioServicio _usuarios;

    public CrearModel(IUsuarioServicio usuarios) => _usuarios = usuarios;

    [BindProperty] public UsuarioCrearRequest Entrada { get; set; } = new();

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        var resultado = await _usuarios.CrearAsync(Entrada);
        if (!resultado.Exito)
        {
            AgregarErrores(resultado, nameof(Entrada));
            return Page();
        }
        MensajeExito = $"Usuario {resultado.Datos?.NombreCompleto} creado con el rol {Formato.EtiquetaRol(Entrada.Rol)}.";
        return RedirectToPage("Index");
    }
}

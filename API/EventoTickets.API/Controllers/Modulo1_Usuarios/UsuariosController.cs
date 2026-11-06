using EventoTickets.Abstracciones.Constantes;
using EventoTickets.Abstracciones.Interfaces.API;
using EventoTickets.Abstracciones.Interfaces.Flujo;
using EventoTickets.Abstracciones.Modelos;
using EventoTickets.API.Seguridad;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventoTickets.API.Controllers;


/// <summary>Gestión de usuarios y roles (solo Superadministrador).</summary>
[ApiController]
[Route("api/usuarios")]
[Produces("application/json")]
[Authorize(Roles = Roles.Superadministrador)]
public class UsuariosController : ControllerBase, IUsuarioController
{
    private readonly IUsuarioFlujo _flujo;

    public UsuariosController(IUsuarioFlujo flujo) => _flujo = flujo;

    [HttpGet]
    [ProducesResponseType(typeof(RespuestaApi<IEnumerable<UsuarioResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar([FromQuery] string? busqueda, [FromQuery] string? rol, [FromQuery] string? estado) =>
        Ok(await _flujo.ListarAsync(busqueda, rol, estado));

    [HttpGet("{id:int:min(1)}")]
    [ProducesResponseType(typeof(RespuestaApi<UsuarioResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespuestaApi<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Obtener([FromRoute] int id) => Ok(await _flujo.ObtenerAsync(id));

    [HttpPost]
    [ProducesResponseType(typeof(RespuestaApi<UsuarioResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(RespuestaApi<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Crear([FromBody] UsuarioCrearRequest solicitud)
    {
        var usuario = await _flujo.CrearAsync(solicitud);
        return CreatedAtAction(nameof(Obtener), new { id = usuario.UsuarioId }, usuario);
    }

    [HttpPut("{id:int:min(1)}")]
    [ProducesResponseType(typeof(RespuestaApi<UsuarioResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Actualizar([FromRoute] int id, [FromBody] UsuarioActualizarRequest solicitud) =>
        Ok(await _flujo.ActualizarAsync(User.ObtenerUsuarioId(), id, solicitud));

    /// <summary>Bloquear (Bloqueado) o desbloquear (Activo) una cuenta (HU-03).</summary>
    [HttpPatch("{id:int:min(1)}/estado")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> CambiarEstado([FromRoute] int id, [FromBody] CambiarEstadoUsuarioRequest solicitud)
    {
        await _flujo.CambiarEstadoAsync(User.ObtenerUsuarioId(), id, solicitud.Estado);
        return Ok(RespuestaApi.Mensaje($"Estado actualizado a {solicitud.Estado}."));
    }

    [HttpPut("{id:int:min(1)}/password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> CambiarPassword([FromRoute] int id, [FromBody] CambiarPasswordRequest solicitud)
    {
        await _flujo.CambiarPasswordAsync(id, solicitud);
        return Ok(RespuestaApi.Mensaje("Contraseña restablecida."));
    }

    /// <summary>Borrado lógico: la cuenta pasa a Inactivo y conserva su historial (HU-03).</summary>
    [HttpDelete("{id:int:min(1)}")]
    [ProducesResponseType(typeof(RespuestaApi<ResultadoEliminacion>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Eliminar([FromRoute] int id) => Ok(await _flujo.EliminarAsync(User.ObtenerUsuarioId(), id));
}

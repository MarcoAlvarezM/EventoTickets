using EventoTickets.Abstracciones.Constantes;
using EventoTickets.Abstracciones.Interfaces.API;
using EventoTickets.Abstracciones.Interfaces.Flujo;
using EventoTickets.Abstracciones.Modelos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventoTickets.API.Controllers;


/// <summary>Locaciones o recintos. Lectura pública; escritura para gestión de eventos.</summary>
[ApiController]
[Route("api/locaciones")]
[Produces("application/json")]
[Authorize(Roles = Roles.GestionEventos)]
public class LocacionesController : ControllerBase, ILocacionController
{
    private readonly ILocacionFlujo _flujo;

    public LocacionesController(ILocacionFlujo flujo) => _flujo = flujo;

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(RespuestaApi<IEnumerable<LocacionResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar([FromQuery] bool soloActivas = false, [FromQuery] string? busqueda = null) =>
        Ok(await _flujo.ListarAsync(soloActivas, busqueda));

    [HttpGet("{id:int:min(1)}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(RespuestaApi<LocacionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespuestaApi<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Obtener([FromRoute] int id) => Ok(await _flujo.ObtenerAsync(id));

    [HttpPost]
    [ProducesResponseType(typeof(RespuestaApi<LocacionResponse>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Crear([FromBody] LocacionRequest solicitud)
    {
        var locacion = await _flujo.CrearAsync(solicitud);
        return CreatedAtAction(nameof(Obtener), new { id = locacion.LocacionId }, locacion);
    }

    [HttpPut("{id:int:min(1)}")]
    [ProducesResponseType(typeof(RespuestaApi<LocacionResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Actualizar([FromRoute] int id, [FromBody] LocacionRequest solicitud) =>
        Ok(await _flujo.ActualizarAsync(id, solicitud));

    [HttpDelete("{id:int:min(1)}")]
    [ProducesResponseType(typeof(RespuestaApi<ResultadoEliminacion>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Eliminar([FromRoute] int id) => Ok(await _flujo.EliminarAsync(id));
}

using EventoTickets.Abstracciones.Constantes;
using EventoTickets.Abstracciones.Interfaces.API;
using EventoTickets.Abstracciones.Interfaces.Flujo;
using EventoTickets.Abstracciones.Modelos;
using EventoTickets.API.Seguridad;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventoTickets.API.Controllers;


/// <summary>Tipos de entrada (localidades / tarifas) de cada evento.</summary>
[ApiController]
[Route("api/tipos-entrada")]
[Produces("application/json")]
[Authorize(Roles = Roles.GestionEventos)]
public class TiposEntradaController : ControllerBase, ITipoEntradaController
{
    private readonly ITipoEntradaFlujo _flujo;

    public TiposEntradaController(ITipoEntradaFlujo flujo) => _flujo = flujo;

    [HttpGet("{id:int:min(1)}")]
    [ProducesResponseType(typeof(RespuestaApi<TipoEntradaResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespuestaApi<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Obtener([FromRoute] int id) => Ok(await _flujo.ObtenerAsync(id));

    [HttpPost]
    [ProducesResponseType(typeof(RespuestaApi<TipoEntradaResponse>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Crear([FromBody] TipoEntradaRequest solicitud)
    {
        var tipo = await _flujo.CrearAsync(solicitud);
        return CreatedAtAction(nameof(Obtener), new { id = tipo.TipoEntradaId }, tipo);
    }

    [HttpPut("{id:int:min(1)}")]
    [ProducesResponseType(typeof(RespuestaApi<TipoEntradaResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Actualizar([FromRoute] int id, [FromBody] TipoEntradaBase solicitud) =>
        Ok(await _flujo.ActualizarAsync(id, solicitud));

    [HttpDelete("{id:int:min(1)}")]
    [ProducesResponseType(typeof(RespuestaApi<ResultadoEliminacion>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Eliminar([FromRoute] int id) => Ok(await _flujo.EliminarAsync(id));
}

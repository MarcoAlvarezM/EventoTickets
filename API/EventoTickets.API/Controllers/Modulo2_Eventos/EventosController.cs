using EventoTickets.Abstracciones.Constantes;
using EventoTickets.Abstracciones.Interfaces.API;
using EventoTickets.Abstracciones.Interfaces.Flujo;
using EventoTickets.Abstracciones.Modelos;
using EventoTickets.API.Seguridad;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventoTickets.API.Controllers;


/// <summary>Eventos: catálogo público y administración.</summary>
[ApiController]
[Route("api/eventos")]
[Produces("application/json")]
[Authorize(Roles = Roles.GestionEventos)]
public class EventosController : ControllerBase, IEventoController
{
    private readonly IEventoFlujo _flujo;
    private readonly ITipoEntradaFlujo _tipoEntradaFlujo;

    public EventosController(IEventoFlujo flujo, ITipoEntradaFlujo tipoEntradaFlujo)
    {
        _flujo = flujo;
        _tipoEntradaFlujo = tipoEntradaFlujo;
    }

    /// <summary>Cartelera / listado (HU-06). Sin sesión de personal solo devuelve eventos Publicados o Agotados vigentes.</summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(RespuestaApi<IEnumerable<EventoResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar([FromQuery] FiltroEventos filtro) => Ok(await _flujo.ListarAsync(filtro, User.EsPersonal()));

    [HttpGet("{id:int:min(1)}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(RespuestaApi<EventoResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespuestaApi<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Obtener([FromRoute] int id) => Ok(await _flujo.ObtenerAsync(id, User.EsPersonal()));

    [HttpPost]
    [ProducesResponseType(typeof(RespuestaApi<EventoResponse>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Crear([FromBody] EventoRequest solicitud)
    {
        var evento = await _flujo.CrearAsync(solicitud);
        return CreatedAtAction(nameof(Obtener), new { id = evento.EventoId }, evento);
    }

    [HttpPut("{id:int:min(1)}")]
    [ProducesResponseType(typeof(RespuestaApi<EventoResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Actualizar([FromRoute] int id, [FromBody] EventoRequest solicitud) =>
        Ok(await _flujo.ActualizarAsync(id, solicitud));

    /// <summary>Publica, devuelve a borrador, finaliza o cancela (cancelar libera reservas y anula entradas).</summary>
    [HttpPatch("{id:int:min(1)}/estado")]
    [ProducesResponseType(typeof(RespuestaApi<EventoResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CambiarEstado([FromRoute] int id, [FromBody] CambiarEstadoEventoRequest solicitud) =>
        Ok(await _flujo.CambiarEstadoAsync(id, solicitud.Estado));

    [HttpDelete("{id:int:min(1)}")]
    [ProducesResponseType(typeof(RespuestaApi<ResultadoEliminacion>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespuestaApi<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Eliminar([FromRoute] int id) => Ok(await _flujo.EliminarAsync(id));

    /// <summary>Tipos de entrada del evento con disponibilidad en tiempo real.</summary>
    [HttpGet("{id:int:min(1)}/tipos-entrada")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(RespuestaApi<IEnumerable<TipoEntradaResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarTiposEntrada([FromRoute] int id, [FromQuery] bool soloActivos = false)
    {
        await _flujo.ObtenerAsync(id, User.EsPersonal());
        return Ok(await _tipoEntradaFlujo.ListarPorEventoAsync(id, soloActivos || !User.EsPersonal()));
    }
}

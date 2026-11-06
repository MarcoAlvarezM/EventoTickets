using EventoTickets.Abstracciones.Constantes;
using EventoTickets.Abstracciones.Interfaces.API;
using EventoTickets.Abstracciones.Interfaces.Flujo;
using EventoTickets.Abstracciones.Modelos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventoTickets.API.Controllers;


/// <summary>Categorías de eventos. Lectura pública; escritura para gestión de eventos.</summary>
[ApiController]
[Route("api/categorias")]
[Produces("application/json")]
[Authorize(Roles = Roles.GestionEventos)]
public class CategoriasController : ControllerBase, ICategoriaController
{
    private readonly ICategoriaFlujo _flujo;

    public CategoriasController(ICategoriaFlujo flujo) => _flujo = flujo;

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(RespuestaApi<IEnumerable<CategoriaResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar([FromQuery] bool soloActivas = false) => Ok(await _flujo.ListarAsync(soloActivas));

    [HttpGet("{id:int:min(1)}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(RespuestaApi<CategoriaResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespuestaApi<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Obtener([FromRoute] int id) => Ok(await _flujo.ObtenerAsync(id));

    [HttpPost]
    [ProducesResponseType(typeof(RespuestaApi<CategoriaResponse>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Crear([FromBody] CategoriaRequest solicitud)
    {
        var categoria = await _flujo.CrearAsync(solicitud);
        return CreatedAtAction(nameof(Obtener), new { id = categoria.CategoriaId }, categoria);
    }

    [HttpPut("{id:int:min(1)}")]
    [ProducesResponseType(typeof(RespuestaApi<CategoriaResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Actualizar([FromRoute] int id, [FromBody] CategoriaRequest solicitud) =>
        Ok(await _flujo.ActualizarAsync(id, solicitud));

    [HttpDelete("{id:int:min(1)}")]
    [ProducesResponseType(typeof(RespuestaApi<ResultadoEliminacion>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Eliminar([FromRoute] int id) => Ok(await _flujo.EliminarAsync(id));
}

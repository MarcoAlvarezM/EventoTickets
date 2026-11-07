using EventoTickets.Abstracciones.Constantes;
using EventoTickets.Abstracciones.Interfaces.API;
using EventoTickets.Abstracciones.Interfaces.Flujo;
using EventoTickets.Abstracciones.Modelos;
using EventoTickets.API.Seguridad;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventoTickets.API.Controllers;


/// <summary>Proceso de compra: cotización, reserva de entradas y consulta de pedidos.</summary>
[ApiController]
[Route("api/pedidos")]
[Produces("application/json")]
[Authorize]
public class PedidosController : ControllerBase, IPedidoController
{
    private readonly IPedidoFlujo _flujo;

    public PedidosController(IPedidoFlujo flujo) => _flujo = flujo;

    /// <summary>Calcula subtotal, cargo por servicio, IVA y total sin reservar inventario.</summary>
    [HttpPost("cotizacion")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(RespuestaApi<CotizacionResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Cotizar([FromBody] PedidoRequest solicitud) => Ok(await _flujo.CotizarAsync(solicitud));

    /// <summary>Crea el pedido y reserva las entradas durante el tiempo configurado.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(RespuestaApi<PedidoCompletoResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(RespuestaApi<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Crear([FromBody] PedidoRequest solicitud)
    {
        var pedido = await _flujo.CrearAsync(User.ObtenerUsuarioId(), solicitud);
        return CreatedAtAction(nameof(Obtener), new { id = pedido.PedidoId }, pedido);
    }

    /// <summary>Pedidos del usuario autenticado.</summary>
    [HttpGet("mios")]
    [ProducesResponseType(typeof(RespuestaApi<IEnumerable<PedidoResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarMios([FromQuery] string? estado) =>
        Ok(await _flujo.ListarPorUsuarioAsync(User.ObtenerUsuarioId(), estado));

    /// <summary>Todos los pedidos (personal administrativo).</summary>
    [HttpGet]
    [Authorize(Roles = Roles.Supervision)]
    [ProducesResponseType(typeof(RespuestaApi<IEnumerable<PedidoResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar([FromQuery] FiltroPedidos filtro) => Ok(await _flujo.ListarAsync(filtro));

    [HttpGet("{id:int:min(1)}")]
    [ProducesResponseType(typeof(RespuestaApi<PedidoCompletoResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespuestaApi<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(RespuestaApi<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Obtener([FromRoute] int id) =>
        Ok(await _flujo.ObtenerAsync(id, User.ObtenerUsuarioId(), User.EsPersonal()));

    /// <summary>UPDATE del CRUD: cambia el método de pago de un pedido PendientePago.</summary>
    [HttpPatch("{id:int:min(1)}/metodo-pago")]
    [ProducesResponseType(typeof(RespuestaApi<PedidoCompletoResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespuestaApi<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ActualizarMetodoPago([FromRoute] int id, [FromBody] ActualizarMetodoPagoRequest solicitud) =>
        Ok(await _flujo.ActualizarMetodoPagoAsync(id, User.ObtenerUsuarioId(), User.EsPersonal(), solicitud));

    /// <summary>DELETE lógico del CRUD (HU-14): cancela un pedido PendientePago y libera el cupo. Los pedidos nunca se borran físicamente.</summary>
    [HttpPost("{id:int:min(1)}/cancelacion")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Cancelar([FromRoute] int id)
    {
        await _flujo.CancelarAsync(id, User.ObtenerUsuarioId(), User.EsPersonal());
        return Ok(RespuestaApi.Mensaje("Pedido cancelado; las entradas reservadas volvieron al inventario."));
    }

    /// <summary>Vence las reservas expiradas y devuelve el cupo al inventario.</summary>
    [HttpPost("liberacion-vencidos")]
    [Authorize(Roles = Roles.Supervision)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> LiberarVencidos() => Ok(new { pedidosLiberados = await _flujo.LiberarVencidosAsync() });
}

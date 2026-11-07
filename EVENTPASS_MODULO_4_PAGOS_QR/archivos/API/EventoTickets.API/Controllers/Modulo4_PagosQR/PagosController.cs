using EventoTickets.Abstracciones.Constantes;
using EventoTickets.Abstracciones.Interfaces.API;
using EventoTickets.Abstracciones.Interfaces.Flujo;
using EventoTickets.Abstracciones.Modelos;
using EventoTickets.API.Seguridad;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventoTickets.API.Controllers;


/// <summary>Reporte de pagos manuales y validación por auditoría.</summary>
[ApiController]
[Route("api/pagos")]
[Produces("application/json")]
[Authorize]
public class PagosController : ControllerBase, IPagoController
{
    private readonly IPagoFlujo _flujo;

    public PagosController(IPagoFlujo flujo) => _flujo = flujo;

    /// <summary>El comprador reporta su transferencia adjuntando el comprobante (multipart/form-data).</summary>
    [HttpPost]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(RespuestaApi<PagoResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(RespuestaApi<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Registrar([FromForm] PagoRegistroRequest solicitud)
    {
        var pago = await _flujo.RegistrarAsync(User.ObtenerUsuarioId(), solicitud);
        return CreatedAtAction(nameof(Obtener), new { id = pago.PagoId }, pago);
    }

    /// <summary>Bandeja de pagos (auditoría). Filtre por estado=Pendiente para la cola de validación.</summary>
    [HttpGet]
    [Authorize(Roles = Roles.Supervision)]
    [ProducesResponseType(typeof(RespuestaApi<IEnumerable<PagoResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar([FromQuery] string? estado, [FromQuery] string? busqueda) =>
        Ok(await _flujo.ListarAsync(estado, busqueda));

    [HttpGet("mios")]
    [ProducesResponseType(typeof(RespuestaApi<IEnumerable<PagoResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarMios() => Ok(await _flujo.ListarPorUsuarioAsync(User.ObtenerUsuarioId()));

    /// <summary>Historial de intentos de pago de un pedido (dueño o personal).</summary>
    [HttpGet("pedido/{pedidoId:int:min(1)}")]
    [ProducesResponseType(typeof(RespuestaApi<IEnumerable<PagoResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarPorPedido([FromRoute] int pedidoId) =>
        Ok(await _flujo.ListarPorPedidoAsync(pedidoId, User.ObtenerUsuarioId(), User.EsPersonal()));

    [HttpGet("{id:int:min(1)}")]
    [ProducesResponseType(typeof(RespuestaApi<PagoResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespuestaApi<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Obtener([FromRoute] int id) =>
        Ok(await _flujo.ObtenerAsync(id, User.ObtenerUsuarioId(), User.EsPersonal()));

    /// <summary>Descarga el comprobante (solo el comprador dueño o el personal).</summary>
    [HttpGet("{id:int:min(1)}/comprobante")]
    [Produces("application/octet-stream", "image/png", "image/jpeg", "application/pdf")]
    [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> DescargarComprobante([FromRoute] int id)
    {
        var archivo = await _flujo.ObtenerComprobanteAsync(id, User.ObtenerUsuarioId(), User.EsPersonal());
        return File(archivo.Contenido, archivo.TipoContenido, archivo.NombreArchivo);
    }

    /// <summary>Aprueba el pago: confirma el pedido y emite las entradas.</summary>
    [HttpPost("{id:int:min(1)}/aprobacion")]
    [Authorize(Roles = Roles.Auditoria)]
    [ProducesResponseType(typeof(RespuestaApi<PagoResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Aprobar([FromRoute] int id, [FromBody] PagoAprobarRequest solicitud) =>
        Ok(await _flujo.AprobarAsync(id, User.ObtenerUsuarioId(), solicitud));

    /// <summary>Rechaza el pago; el pedido vuelve a pendiente con un plazo de gracia.</summary>
    [HttpPost("{id:int:min(1)}/rechazo")]
    [Authorize(Roles = Roles.Auditoria)]
    [ProducesResponseType(typeof(RespuestaApi<PagoResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Rechazar([FromRoute] int id, [FromBody] PagoRechazarRequest solicitud) =>
        Ok(await _flujo.RechazarAsync(id, User.ObtenerUsuarioId(), solicitud));
}

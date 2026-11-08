using EventoTickets.Abstracciones.Constantes;
using EventoTickets.Abstracciones.Interfaces.API;
using EventoTickets.Abstracciones.Interfaces.Flujo;
using EventoTickets.Abstracciones.Interfaces.Servicios;
using EventoTickets.Abstracciones.Modelos;
using EventoTickets.API.Seguridad;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventoTickets.API.Controllers;


/// <summary>Entradas digitales: consulta, transferencia de titular y control de acceso.</summary>
[ApiController]
[Route("api/entradas")]
[Produces("application/json")]
[Authorize]
public class EntradasController : ControllerBase, IEntradaController
{
    private readonly IEntradaFlujo _flujo;
    private readonly IServicioQr _qr;

    public EntradasController(IEntradaFlujo flujo, IServicioQr qr)
    {
        _flujo = flujo;
        _qr = qr;
    }

    [HttpGet("mias")]
    [ProducesResponseType(typeof(RespuestaApi<IEnumerable<EntradaResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarMias() => Ok(await _flujo.ListarPorUsuarioAsync(User.ObtenerUsuarioId()));

    /// <summary>Entradas emitidas para un evento (control de acceso y auditoría).</summary>
    [HttpGet("evento/{eventoId:int:min(1)}")]
    [Authorize(Roles = Roles.Personal)]
    [ProducesResponseType(typeof(RespuestaApi<IEnumerable<EntradaResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarPorEvento([FromRoute] int eventoId, [FromQuery] string? estado) =>
        Ok(await _flujo.ListarPorEventoAsync(eventoId, estado));

    [HttpGet("{id:int:min(1)}")]
    [ProducesResponseType(typeof(RespuestaApi<EntradaResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespuestaApi<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Obtener([FromRoute] int id) =>
        Ok(await _flujo.ObtenerAsync(id, User.ObtenerUsuarioId(), User.EsPersonal()));

    /// <summary>Imagen PNG del código QR (contenido: CODIGO|HASH). Solo el dueño o el personal (HU-18).</summary>
    [HttpGet("{id:int:min(1)}/qr")]
    [Produces("image/png")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerQr([FromRoute] int id)
    {
        var entrada = await _flujo.ObtenerAsync(id, User.ObtenerUsuarioId(), User.EsPersonal());
        return File(_qr.GenerarPng($"{entrada.CodigoTicket}|{entrada.HashCriptografico}"), "image/png", $"{entrada.CodigoTicket}.png");
    }

    /// <summary>Cambia el titular de la entrada y regenera su código QR.</summary>
    [HttpPut("{id:int:min(1)}/titular")]
    [ProducesResponseType(typeof(RespuestaApi<EntradaResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Transferir([FromRoute] int id, [FromBody] TransferenciaEntradaRequest solicitud) =>
        Ok(await _flujo.TransferirAsync(id, User.ObtenerUsuarioId(), solicitud));

    /// <summary>Registra el ingreso en puerta (una sola vez por entrada).</summary>
    [HttpPost("ingresos")]
    [Authorize(Roles = Roles.ControlAcceso)]
    [ProducesResponseType(typeof(RespuestaApi<EntradaResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespuestaApi<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegistrarIngreso([FromBody] ValidacionAccesoRequest solicitud) =>
        Ok(await _flujo.RegistrarIngresoAsync(solicitud, User.ObtenerUsuarioId()));
}

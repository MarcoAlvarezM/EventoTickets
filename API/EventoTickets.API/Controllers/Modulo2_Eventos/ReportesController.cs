using EventoTickets.Abstracciones.Constantes;
using EventoTickets.Abstracciones.Interfaces.API;
using EventoTickets.Abstracciones.Interfaces.Flujo;
using EventoTickets.Abstracciones.Modelos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventoTickets.API.Controllers;


/// <summary>Indicadores del panel administrativo y reporte de ventas.</summary>
[ApiController]
[Route("api/reportes")]
[Produces("application/json")]
[Authorize(Roles = Roles.Personal)]
public class ReportesController : ControllerBase, IReporteController
{
    private readonly IReporteFlujo _flujo;

    public ReportesController(IReporteFlujo flujo) => _flujo = flujo;

    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(RespuestaApi<DashboardResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Dashboard() => Ok(await _flujo.ObtenerDashboardAsync());

    [HttpGet("ventas-eventos")]
    [Authorize(Roles = Roles.Supervision)]
    [ProducesResponseType(typeof(RespuestaApi<IEnumerable<VentaEventoResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> VentasPorEvento([FromQuery] FiltroReporteVentas filtro) => Ok(await _flujo.VentasPorEventoAsync(filtro));
}

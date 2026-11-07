using EventoTickets.Abstracciones.Excepciones;
using EventoTickets.Abstracciones.Interfaces.DA;
using EventoTickets.Abstracciones.Interfaces.Flujo;
using EventoTickets.Abstracciones.Modelos;

namespace EventoTickets.Flujo;


public class ReporteFlujo : IReporteFlujo
{
    private readonly IReporteDA _reporteDA;

    public ReporteFlujo(IReporteDA reporteDA) => _reporteDA = reporteDA;

    public Task<DashboardResponse> ObtenerDashboardAsync() => _reporteDA.ObtenerDashboardAsync();

    public Task<IEnumerable<VentaEventoResponse>> VentasPorEventoAsync(FiltroReporteVentas filtro)
    {
        if (filtro.FechaDesde > filtro.FechaHasta)
            throw new ReglaNegocioException("La fecha inicial no puede ser posterior a la final.");
        return _reporteDA.VentasPorEventoAsync(filtro);
    }
}

using EventoTickets.Abstracciones.Modelos;

namespace EventoTickets.Abstracciones.Interfaces.Flujo;


public interface IEventoFlujo
{
    Task<IEnumerable<EventoResponse>> ListarAsync(FiltroEventos filtro, bool esPersonal);
    Task<EventoResponse> ObtenerAsync(int eventoId, bool esPersonal);
    Task<EventoResponse> CrearAsync(EventoRequest solicitud);
    Task<EventoResponse> ActualizarAsync(int eventoId, EventoRequest solicitud);
    Task<EventoResponse> CambiarEstadoAsync(int eventoId, string estado);
    Task<ResultadoEliminacion> EliminarAsync(int eventoId);
}

public interface ITipoEntradaFlujo
{
    Task<IEnumerable<TipoEntradaResponse>> ListarPorEventoAsync(int eventoId, bool soloActivos);
    Task<TipoEntradaResponse> ObtenerAsync(int tipoEntradaId);
    Task<TipoEntradaResponse> CrearAsync(TipoEntradaRequest solicitud);
    Task<TipoEntradaResponse> ActualizarAsync(int tipoEntradaId, TipoEntradaBase solicitud);
    Task<ResultadoEliminacion> EliminarAsync(int tipoEntradaId);
}

public interface IReporteFlujo
{
    Task<DashboardResponse> ObtenerDashboardAsync();
    Task<IEnumerable<VentaEventoResponse>> VentasPorEventoAsync(FiltroReporteVentas filtro);
}

using EventoTickets.Web.Abstracciones.Modelos;

namespace EventoTickets.Web.Abstracciones.Interfaces;


public interface IEventoServicio
{
    Task<ResultadoApi<List<EventoResponse>>> ListarAsync(FiltroEventos filtro);
    Task<ResultadoApi<EventoResponse>> ObtenerAsync(int id);
    Task<ResultadoApi<EventoResponse>> CrearAsync(EventoRequest solicitud);
    Task<ResultadoApi<EventoResponse>> ActualizarAsync(int id, EventoRequest solicitud);
    Task<ResultadoApi<EventoResponse>> CambiarEstadoAsync(int id, string estado);
    Task<ResultadoApi<ResultadoEliminacion>> EliminarAsync(int id);
}

public interface ITipoEntradaServicio
{
    Task<ResultadoApi<List<TipoEntradaResponse>>> ListarPorEventoAsync(int eventoId, bool soloActivos);
    Task<ResultadoApi<TipoEntradaResponse>> ObtenerAsync(int id);
    Task<ResultadoApi<TipoEntradaResponse>> CrearAsync(TipoEntradaRequest solicitud);
    Task<ResultadoApi<TipoEntradaResponse>> ActualizarAsync(int id, TipoEntradaBase solicitud);
    Task<ResultadoApi<ResultadoEliminacion>> EliminarAsync(int id);
}

public interface IReporteServicio
{
    Task<ResultadoApi<DashboardResponse>> DashboardAsync();
    Task<ResultadoApi<List<VentaEventoResponse>>> VentasPorEventoAsync(FiltroReporteVentas filtro);
}

using EventoTickets.Web.Abstracciones.Interfaces;
using EventoTickets.Web.Abstracciones.Modelos;

namespace EventoTickets.Web.Servicios;


public class EventoServicio : IEventoServicio
{
    private readonly IClienteApi _api;
    public EventoServicio(IClienteApi api) => _api = api;

    public Task<ResultadoApi<List<EventoResponse>>> ListarAsync(FiltroEventos filtro) =>
        _api.GetAsync<List<EventoResponse>>(Consulta.Crear("api/eventos", ("busqueda", filtro.Busqueda), ("categoriaId", filtro.CategoriaId),
            ("locacionId", filtro.LocacionId), ("estado", filtro.Estado), ("soloVigentes", filtro.SoloVigentes),
            ("soloCartelera", filtro.SoloCartelera), ("ciudad", filtro.Ciudad), ("fechaDesde", filtro.FechaDesde), ("fechaHasta", filtro.FechaHasta)));
    public Task<ResultadoApi<EventoResponse>> ObtenerAsync(int id) => _api.GetAsync<EventoResponse>($"api/eventos/{id}");
    public Task<ResultadoApi<EventoResponse>> CrearAsync(EventoRequest solicitud) => _api.PostAsync<EventoResponse>("api/eventos", solicitud);
    public Task<ResultadoApi<EventoResponse>> ActualizarAsync(int id, EventoRequest solicitud) => _api.PutAsync<EventoResponse>($"api/eventos/{id}", solicitud);
    public Task<ResultadoApi<EventoResponse>> CambiarEstadoAsync(int id, string estado) =>
        _api.PatchAsync<EventoResponse>($"api/eventos/{id}/estado", new CambiarEstadoEventoRequest { Estado = estado });
    public Task<ResultadoApi<ResultadoEliminacion>> EliminarAsync(int id) => _api.DeleteAsync<ResultadoEliminacion>($"api/eventos/{id}");
}

public class TipoEntradaServicio : ITipoEntradaServicio
{
    private readonly IClienteApi _api;
    public TipoEntradaServicio(IClienteApi api) => _api = api;

    public Task<ResultadoApi<List<TipoEntradaResponse>>> ListarPorEventoAsync(int eventoId, bool soloActivos) =>
        _api.GetAsync<List<TipoEntradaResponse>>(Consulta.Crear($"api/eventos/{eventoId}/tipos-entrada", ("soloActivos", soloActivos)));
    public Task<ResultadoApi<TipoEntradaResponse>> ObtenerAsync(int id) => _api.GetAsync<TipoEntradaResponse>($"api/tipos-entrada/{id}");
    public Task<ResultadoApi<TipoEntradaResponse>> CrearAsync(TipoEntradaRequest solicitud) => _api.PostAsync<TipoEntradaResponse>("api/tipos-entrada", solicitud);
    public Task<ResultadoApi<TipoEntradaResponse>> ActualizarAsync(int id, TipoEntradaBase solicitud) => _api.PutAsync<TipoEntradaResponse>($"api/tipos-entrada/{id}", solicitud);
    public Task<ResultadoApi<ResultadoEliminacion>> EliminarAsync(int id) => _api.DeleteAsync<ResultadoEliminacion>($"api/tipos-entrada/{id}");
}

public class ReporteServicio : IReporteServicio
{
    private readonly IClienteApi _api;
    public ReporteServicio(IClienteApi api) => _api = api;

    public Task<ResultadoApi<DashboardResponse>> DashboardAsync() => _api.GetAsync<DashboardResponse>("api/reportes/dashboard");
    public Task<ResultadoApi<List<VentaEventoResponse>>> VentasPorEventoAsync(FiltroReporteVentas filtro) =>
        _api.GetAsync<List<VentaEventoResponse>>(Consulta.Crear("api/reportes/ventas-eventos",
            ("fechaDesde", filtro.FechaDesde), ("fechaHasta", filtro.FechaHasta), ("categoriaId", filtro.CategoriaId)));
}

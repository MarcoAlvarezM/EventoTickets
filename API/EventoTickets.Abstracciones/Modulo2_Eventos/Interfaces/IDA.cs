using System.Data;
using EventoTickets.Abstracciones.Modelos;
using Microsoft.AspNetCore.Http;

namespace EventoTickets.Abstracciones.Interfaces.DA;


public interface IEventoDA
{
    Task<IEnumerable<EventoResponse>> ListarAsync(FiltroEventos filtro);
    Task<EventoResponse?> ObtenerAsync(int eventoId);
    Task<int> CrearAsync(EventoRequest datos, string estado);
    Task ActualizarAsync(int eventoId, EventoRequest datos);
    Task CambiarEstadoAsync(int eventoId, string estado);
    Task<string> EliminarAsync(int eventoId);
}

public interface ITipoEntradaDA
{
    Task<IEnumerable<TipoEntradaResponse>> ListarPorEventoAsync(int eventoId, bool soloActivos);
    Task<TipoEntradaResponse?> ObtenerAsync(int tipoEntradaId);
    Task<int> CrearAsync(TipoEntradaRequest datos);
    Task ActualizarAsync(int tipoEntradaId, TipoEntradaBase datos);
    Task<string> EliminarAsync(int tipoEntradaId);
}

public interface IReporteDA
{
    Task<DashboardResponse> ObtenerDashboardAsync();
    Task<IEnumerable<VentaEventoResponse>> VentasPorEventoAsync(FiltroReporteVentas filtro);
}

using System.Data;
using Dapper;
using EventoTickets.Abstracciones.Interfaces.DA;
using EventoTickets.Abstracciones.Modelos;

namespace EventoTickets.DA.Repositorios;


public class ReporteDA : IReporteDA
{
    private readonly IRepositorioDapper _repositorio;

    public ReporteDA(IRepositorioDapper repositorio) => _repositorio = repositorio;

    public Task<DashboardResponse> ObtenerDashboardAsync() =>
        TraductorErroresSql.EjecutarAsync(async () =>
        {
            using var cn = _repositorio.ObtenerConexion();
            using var lector = await cn.QueryMultipleAsync("dbo.sp_Reporte_Dashboard", commandType: CommandType.StoredProcedure);
            return new DashboardResponse
            {
                Indicadores = await lector.ReadSingleAsync<IndicadoresDashboard>(),
                PagosPendientes = (await lector.ReadAsync<DashboardPago>()).ToList(),
                ProximosEventos = (await lector.ReadAsync<VentaEventoResponse>()).ToList(),
                PedidosRecientes = (await lector.ReadAsync<DashboardPedido>()).ToList()
            };
        });

    public Task<IEnumerable<VentaEventoResponse>> VentasPorEventoAsync(FiltroReporteVentas filtro) =>
        TraductorErroresSql.EjecutarAsync(async () =>
        {
            using var cn = _repositorio.ObtenerConexion();
            return await cn.QueryAsync<VentaEventoResponse>("dbo.sp_Reporte_VentasPorEvento", new
            {
                FechaDesde = filtro.FechaDesde?.Date,
                FechaHasta = filtro.FechaHasta?.Date,
                filtro.CategoriaId
            }, commandType: CommandType.StoredProcedure);
        });
}

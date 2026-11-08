using System.Data;
using Dapper;
using EventoTickets.Abstracciones.Interfaces.DA;
using EventoTickets.Abstracciones.Modelos;

namespace EventoTickets.DA.Repositorios;


public class PagoDA : IPagoDA
{
    private readonly IRepositorioDapper _repositorio;

    public PagoDA(IRepositorioDapper repositorio) => _repositorio = repositorio;

    public Task<int> RegistrarAsync(PagoRegistroDatos datos) =>
        TraductorErroresSql.EjecutarAsync(async () =>
        {
            using var cn = _repositorio.ObtenerConexion();
            return await cn.ExecuteScalarAsync<int>("dbo.sp_Pago_Registrar", new
            {
                datos.PedidoId, datos.UsuarioId, datos.BancoOrigen, datos.ReferenciaComprobante, datos.MontoReportado,
                datos.ComprobanteUrl, datos.HashComprobante, datos.NotaComprador
            }, commandType: CommandType.StoredProcedure);
        });

    public Task<IEnumerable<PagoResponse>> ListarAsync(string? estado, string? busqueda) =>
        TraductorErroresSql.EjecutarAsync(async () =>
        {
            using var cn = _repositorio.ObtenerConexion();
            return await cn.QueryAsync<PagoResponse>("dbo.sp_Pago_Listar", new
            {
                Estado = string.IsNullOrWhiteSpace(estado) ? null : estado,
                Busqueda = string.IsNullOrWhiteSpace(busqueda) ? null : busqueda.Trim()
            }, commandType: CommandType.StoredProcedure);
        });

    public Task<IEnumerable<PagoResponse>> ListarPorUsuarioAsync(int usuarioId) =>
        TraductorErroresSql.EjecutarAsync(async () =>
        {
            using var cn = _repositorio.ObtenerConexion();
            return await cn.QueryAsync<PagoResponse>("dbo.sp_Pago_ListarPorUsuario", new { UsuarioId = usuarioId },
                commandType: CommandType.StoredProcedure);
        });

    public Task<IEnumerable<PagoResponse>> ListarPorPedidoAsync(int pedidoId) =>
        TraductorErroresSql.EjecutarAsync(async () =>
        {
            using var cn = _repositorio.ObtenerConexion();
            return await cn.QueryAsync<PagoResponse>("dbo.sp_Pago_ListarPorPedido", new { PedidoId = pedidoId },
                commandType: CommandType.StoredProcedure);
        });

    public Task<PagoResponse?> ObtenerAsync(int pagoId) =>
        TraductorErroresSql.EjecutarAsync(async () =>
        {
            using var cn = _repositorio.ObtenerConexion();
            return await cn.QueryFirstOrDefaultAsync<PagoResponse>("dbo.sp_Pago_ObtenerPorId", new { PagoId = pagoId },
                commandType: CommandType.StoredProcedure);
        });

    public Task<int> AprobarAsync(int pagoId, int auditorUsuarioId, string? codigoAprobacion) =>
        TraductorErroresSql.EjecutarAsync(async () =>
        {
            using var cn = _repositorio.ObtenerConexion();
            return await cn.ExecuteScalarAsync<int>("dbo.sp_Pago_Aprobar",
                new { PagoId = pagoId, AuditorUsuarioId = auditorUsuarioId, CodigoAprobacion = codigoAprobacion },
                commandType: CommandType.StoredProcedure);
        });

    public Task RechazarAsync(int pagoId, int auditorUsuarioId, string motivo, int minutosGracia) =>
        TraductorErroresSql.EjecutarAsync(async () =>
        {
            using var cn = _repositorio.ObtenerConexion();
            await cn.ExecuteAsync("dbo.sp_Pago_Rechazar", new
            {
                PagoId = pagoId, AuditorUsuarioId = auditorUsuarioId, MotivoRechazo = motivo, MinutosGracia = minutosGracia
            }, commandType: CommandType.StoredProcedure);
        });
}

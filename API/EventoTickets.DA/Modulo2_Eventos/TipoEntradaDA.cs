using System.Data;
using Dapper;
using EventoTickets.Abstracciones.Interfaces.DA;
using EventoTickets.Abstracciones.Modelos;

namespace EventoTickets.DA.Repositorios;


public class TipoEntradaDA : ITipoEntradaDA
{
    private readonly IRepositorioDapper _repositorio;

    public TipoEntradaDA(IRepositorioDapper repositorio) => _repositorio = repositorio;

    public Task<IEnumerable<TipoEntradaResponse>> ListarPorEventoAsync(int eventoId, bool soloActivos) =>
        TraductorErroresSql.EjecutarAsync(async () =>
        {
            using var cn = _repositorio.ObtenerConexion();
            return await cn.QueryAsync<TipoEntradaResponse>("dbo.sp_TipoEntrada_ListarPorEvento",
                new { EventoId = eventoId, SoloActivos = soloActivos }, commandType: CommandType.StoredProcedure);
        });

    public Task<TipoEntradaResponse?> ObtenerAsync(int tipoEntradaId) =>
        TraductorErroresSql.EjecutarAsync(async () =>
        {
            using var cn = _repositorio.ObtenerConexion();
            return await cn.QueryFirstOrDefaultAsync<TipoEntradaResponse>("dbo.sp_TipoEntrada_ObtenerPorId",
                new { TipoEntradaId = tipoEntradaId }, commandType: CommandType.StoredProcedure);
        });

    public Task<int> CrearAsync(TipoEntradaRequest datos) =>
        TraductorErroresSql.EjecutarAsync(async () =>
        {
            using var cn = _repositorio.ObtenerConexion();
            return await cn.ExecuteScalarAsync<int>("dbo.sp_TipoEntrada_Crear", new
            {
                datos.EventoId, datos.Nombre, datos.Precio, datos.ColorHex, datos.CantidadInicial, datos.Activo
            }, commandType: CommandType.StoredProcedure);
        });

    public Task ActualizarAsync(int tipoEntradaId, TipoEntradaBase datos) =>
        TraductorErroresSql.EjecutarAsync(async () =>
        {
            using var cn = _repositorio.ObtenerConexion();
            await cn.ExecuteAsync("dbo.sp_TipoEntrada_Actualizar", new
            {
                TipoEntradaId = tipoEntradaId, datos.Nombre, datos.Precio, datos.ColorHex, datos.CantidadInicial, datos.Activo
            }, commandType: CommandType.StoredProcedure);
        });

    public Task<string> EliminarAsync(int tipoEntradaId) =>
        TraductorErroresSql.EjecutarAsync(async () =>
        {
            using var cn = _repositorio.ObtenerConexion();
            return await cn.ExecuteScalarAsync<string>("dbo.sp_TipoEntrada_Eliminar", new { TipoEntradaId = tipoEntradaId },
                commandType: CommandType.StoredProcedure) ?? string.Empty;
        });
}

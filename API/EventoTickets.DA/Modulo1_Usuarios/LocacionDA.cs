using System.Data;
using Dapper;
using EventoTickets.Abstracciones.Interfaces.DA;
using EventoTickets.Abstracciones.Modelos;

namespace EventoTickets.DA.Repositorios;


public class LocacionDA : ILocacionDA
{
    private readonly IRepositorioDapper _repositorio;

    public LocacionDA(IRepositorioDapper repositorio) => _repositorio = repositorio;

    public Task<IEnumerable<LocacionResponse>> ListarAsync(bool soloActivas, string? busqueda) =>
        TraductorErroresSql.EjecutarAsync(async () =>
        {
            using var cn = _repositorio.ObtenerConexion();
            return await cn.QueryAsync<LocacionResponse>("dbo.sp_Locacion_Listar",
                new { SoloActivas = soloActivas, Busqueda = busqueda }, commandType: CommandType.StoredProcedure);
        });

    public Task<LocacionResponse?> ObtenerAsync(int locacionId) =>
        TraductorErroresSql.EjecutarAsync(async () =>
        {
            using var cn = _repositorio.ObtenerConexion();
            return await cn.QueryFirstOrDefaultAsync<LocacionResponse>("dbo.sp_Locacion_ObtenerPorId",
                new { LocacionId = locacionId }, commandType: CommandType.StoredProcedure);
        });

    public Task<int> CrearAsync(LocacionRequest datos) =>
        TraductorErroresSql.EjecutarAsync(async () =>
        {
            using var cn = _repositorio.ObtenerConexion();
            return await cn.ExecuteScalarAsync<int>("dbo.sp_Locacion_Crear", new
            {
                datos.Nombre, datos.Ciudad, datos.Direccion, datos.PabellonPuerta, datos.CapacidadMaxima, datos.Activo
            }, commandType: CommandType.StoredProcedure);
        });

    public Task ActualizarAsync(int locacionId, LocacionRequest datos) =>
        TraductorErroresSql.EjecutarAsync(async () =>
        {
            using var cn = _repositorio.ObtenerConexion();
            await cn.ExecuteAsync("dbo.sp_Locacion_Actualizar", new
            {
                LocacionId = locacionId,
                datos.Nombre, datos.Ciudad, datos.Direccion, datos.PabellonPuerta, datos.CapacidadMaxima, datos.Activo
            }, commandType: CommandType.StoredProcedure);
        });

    public Task<string> EliminarAsync(int locacionId) =>
        TraductorErroresSql.EjecutarAsync(async () =>
        {
            using var cn = _repositorio.ObtenerConexion();
            return await cn.ExecuteScalarAsync<string>("dbo.sp_Locacion_Eliminar", new { LocacionId = locacionId },
                commandType: CommandType.StoredProcedure) ?? string.Empty;
        });
}

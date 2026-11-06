using System.Data;
using Dapper;
using EventoTickets.Abstracciones.Interfaces.DA;
using EventoTickets.Abstracciones.Modelos;

namespace EventoTickets.DA.Repositorios;


public class CategoriaDA : ICategoriaDA
{
    private readonly IRepositorioDapper _repositorio;

    public CategoriaDA(IRepositorioDapper repositorio) => _repositorio = repositorio;

    public Task<IEnumerable<CategoriaResponse>> ListarAsync(bool soloActivas) =>
        TraductorErroresSql.EjecutarAsync(async () =>
        {
            using var cn = _repositorio.ObtenerConexion();
            return await cn.QueryAsync<CategoriaResponse>("dbo.sp_Categoria_Listar", new { SoloActivas = soloActivas },
                commandType: CommandType.StoredProcedure);
        });

    public Task<CategoriaResponse?> ObtenerAsync(int categoriaId) =>
        TraductorErroresSql.EjecutarAsync(async () =>
        {
            using var cn = _repositorio.ObtenerConexion();
            return await cn.QueryFirstOrDefaultAsync<CategoriaResponse>("dbo.sp_Categoria_ObtenerPorId",
                new { CategoriaId = categoriaId }, commandType: CommandType.StoredProcedure);
        });

    public Task<int> CrearAsync(CategoriaRequest datos) =>
        TraductorErroresSql.EjecutarAsync(async () =>
        {
            using var cn = _repositorio.ObtenerConexion();
            return await cn.ExecuteScalarAsync<int>("dbo.sp_Categoria_Crear",
                new { datos.Nombre, datos.Descripcion, datos.Activo }, commandType: CommandType.StoredProcedure);
        });

    public Task ActualizarAsync(int categoriaId, CategoriaRequest datos) =>
        TraductorErroresSql.EjecutarAsync(async () =>
        {
            using var cn = _repositorio.ObtenerConexion();
            await cn.ExecuteAsync("dbo.sp_Categoria_Actualizar",
                new { CategoriaId = categoriaId, datos.Nombre, datos.Descripcion, datos.Activo },
                commandType: CommandType.StoredProcedure);
        });

    public Task<string> EliminarAsync(int categoriaId) =>
        TraductorErroresSql.EjecutarAsync(async () =>
        {
            using var cn = _repositorio.ObtenerConexion();
            return await cn.ExecuteScalarAsync<string>("dbo.sp_Categoria_Eliminar", new { CategoriaId = categoriaId },
                commandType: CommandType.StoredProcedure) ?? string.Empty;
        });
}

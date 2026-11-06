using System.Data;
using Dapper;
using EventoTickets.Abstracciones.Interfaces.DA;
using EventoTickets.Abstracciones.Modelos;

namespace EventoTickets.DA.Repositorios;


public class UsuarioDA : IUsuarioDA
{
    private readonly IRepositorioDapper _repositorio;

    public UsuarioDA(IRepositorioDapper repositorio) => _repositorio = repositorio;

    public Task<IEnumerable<UsuarioResponse>> ListarAsync(string? busqueda, string? rol, string? estado) =>
        TraductorErroresSql.EjecutarAsync(async () =>
        {
            using var cn = _repositorio.ObtenerConexion();
            return await cn.QueryAsync<UsuarioResponse>("dbo.sp_Usuario_Listar",
                new { Busqueda = busqueda, Rol = rol, Estado = estado }, commandType: CommandType.StoredProcedure);
        });

    public Task<UsuarioResponse?> ObtenerAsync(int usuarioId) =>
        TraductorErroresSql.EjecutarAsync(async () =>
        {
            using var cn = _repositorio.ObtenerConexion();
            return await cn.QueryFirstOrDefaultAsync<UsuarioResponse>("dbo.sp_Usuario_ObtenerPorId",
                new { UsuarioId = usuarioId }, commandType: CommandType.StoredProcedure);
        });

    public Task<CredencialesUsuario?> ObtenerCredencialesAsync(string correoElectronico) =>
        TraductorErroresSql.EjecutarAsync(async () =>
        {
            using var cn = _repositorio.ObtenerConexion();
            return await cn.QueryFirstOrDefaultAsync<CredencialesUsuario>("dbo.sp_Usuario_ObtenerCredenciales",
                new { CorreoElectronico = correoElectronico }, commandType: CommandType.StoredProcedure);
        });

    public Task<int> CrearAsync(UsuarioBase datos, string passwordHash, string rol, string estado) =>
        TraductorErroresSql.EjecutarAsync(async () =>
        {
            using var cn = _repositorio.ObtenerConexion();
            return await cn.ExecuteScalarAsync<int>("dbo.sp_Usuario_Crear", new
            {
                datos.TipoDocumento,
                datos.NumeroDocumento,
                datos.NombreCompleto,
                datos.CorreoElectronico,
                datos.Telefono,
                PasswordHash = passwordHash,
                Rol = rol,
                Estado = estado
            }, commandType: CommandType.StoredProcedure);
        });

    public Task ActualizarAsync(int usuarioId, UsuarioActualizarRequest datos) =>
        TraductorErroresSql.EjecutarAsync(async () =>
        {
            using var cn = _repositorio.ObtenerConexion();
            await cn.ExecuteAsync("dbo.sp_Usuario_Actualizar", new
            {
                UsuarioId = usuarioId,
                datos.TipoDocumento,
                datos.NumeroDocumento,
                datos.NombreCompleto,
                datos.CorreoElectronico,
                datos.Telefono,
                datos.Rol,
                datos.Estado
            }, commandType: CommandType.StoredProcedure);
        });

    public Task CambiarEstadoAsync(int usuarioId, string estado) =>
        TraductorErroresSql.EjecutarAsync(async () =>
        {
            using var cn = _repositorio.ObtenerConexion();
            await cn.ExecuteAsync("dbo.sp_Usuario_CambiarEstado", new { UsuarioId = usuarioId, Estado = estado },
                commandType: CommandType.StoredProcedure);
        });

    public Task CambiarPasswordAsync(int usuarioId, string passwordHash) =>
        TraductorErroresSql.EjecutarAsync(async () =>
        {
            using var cn = _repositorio.ObtenerConexion();
            await cn.ExecuteAsync("dbo.sp_Usuario_CambiarPassword", new { UsuarioId = usuarioId, PasswordHash = passwordHash },
                commandType: CommandType.StoredProcedure);
        });

    public Task<string> EliminarAsync(int usuarioId) =>
        TraductorErroresSql.EjecutarAsync(async () =>
        {
            using var cn = _repositorio.ObtenerConexion();
            return await cn.ExecuteScalarAsync<string>("dbo.sp_Usuario_Eliminar", new { UsuarioId = usuarioId },
                commandType: CommandType.StoredProcedure) ?? string.Empty;
        });
}

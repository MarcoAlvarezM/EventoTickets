using System.Data;
using Dapper;
using EventoTickets.Abstracciones.Interfaces.DA;
using EventoTickets.Abstracciones.Modelos;

namespace EventoTickets.DA.Repositorios;


public class EntradaDA : IEntradaDA
{
    private readonly IRepositorioDapper _repositorio;

    public EntradaDA(IRepositorioDapper repositorio) => _repositorio = repositorio;

    public Task<IEnumerable<EntradaResponse>> ListarPorUsuarioAsync(int usuarioId) =>
        TraductorErroresSql.EjecutarAsync(async () =>
        {
            using var cn = _repositorio.ObtenerConexion();
            return await cn.QueryAsync<EntradaResponse>("dbo.sp_Entrada_ListarPorUsuario", new { UsuarioId = usuarioId },
                commandType: CommandType.StoredProcedure);
        });

    public Task<IEnumerable<EntradaResponse>> ListarPorEventoAsync(int eventoId, string? estado) =>
        TraductorErroresSql.EjecutarAsync(async () =>
        {
            using var cn = _repositorio.ObtenerConexion();
            return await cn.QueryAsync<EntradaResponse>("dbo.sp_Entrada_ListarPorEvento",
                new { EventoId = eventoId, Estado = string.IsNullOrWhiteSpace(estado) ? null : estado },
                commandType: CommandType.StoredProcedure);
        });

    public Task<EntradaResponse?> ObtenerAsync(int entradaId) =>
        TraductorErroresSql.EjecutarAsync(async () =>
        {
            using var cn = _repositorio.ObtenerConexion();
            return await cn.QueryFirstOrDefaultAsync<EntradaResponse>("dbo.sp_Entrada_ObtenerPorId", new { EntradaId = entradaId },
                commandType: CommandType.StoredProcedure);
        });

    public Task<EntradaResponse> TransferirAsync(int entradaId, int usuarioId, TransferenciaEntradaRequest datos) =>
        TraductorErroresSql.EjecutarAsync(async () =>
        {
            using var cn = _repositorio.ObtenerConexion();
            return await cn.QuerySingleAsync<EntradaResponse>("dbo.sp_Entrada_Transferir", new
            {
                EntradaId = entradaId,
                UsuarioId = usuarioId,
                NombreAsistente = datos.NombreAsistente.Trim(),
                DocumentoAsistente = datos.DocumentoAsistente.Trim()
            }, commandType: CommandType.StoredProcedure);
        });

    public Task<EntradaResponse> RegistrarIngresoAsync(ValidacionAccesoRequest datos, int validadorUsuarioId) =>
        TraductorErroresSql.EjecutarAsync(async () =>
        {
            using var cn = _repositorio.ObtenerConexion();
            return await cn.QuerySingleAsync<EntradaResponse>("dbo.sp_Entrada_RegistrarIngreso", new
            {
                CodigoTicket = datos.CodigoTicket.Trim(),
                HashCriptografico = string.IsNullOrWhiteSpace(datos.HashCriptografico) ? null : datos.HashCriptografico.Trim(),
                ValidadorUsuarioId = validadorUsuarioId,
                datos.PuertaAcceso
            }, commandType: CommandType.StoredProcedure);
        });
}

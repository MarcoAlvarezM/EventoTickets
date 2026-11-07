using System.Data;
using Dapper;
using EventoTickets.Abstracciones.Interfaces.DA;
using EventoTickets.Abstracciones.Modelos;

namespace EventoTickets.DA.Repositorios;


public class EventoDA : IEventoDA
{
    private readonly IRepositorioDapper _repositorio;

    public EventoDA(IRepositorioDapper repositorio) => _repositorio = repositorio;

    public Task<IEnumerable<EventoResponse>> ListarAsync(FiltroEventos filtro) =>
        TraductorErroresSql.EjecutarAsync(async () =>
        {
            using var cn = _repositorio.ObtenerConexion();
            return await cn.QueryAsync<EventoResponse>("dbo.sp_Evento_Listar", new
            {
                Busqueda = string.IsNullOrWhiteSpace(filtro.Busqueda) ? null : filtro.Busqueda.Trim(),
                filtro.CategoriaId,
                filtro.LocacionId,
                Estado = string.IsNullOrWhiteSpace(filtro.Estado) ? null : filtro.Estado,
                filtro.SoloVigentes,
                filtro.SoloCartelera,
                Ciudad = string.IsNullOrWhiteSpace(filtro.Ciudad) ? null : filtro.Ciudad.Trim(),
                FechaDesde = filtro.FechaDesde?.Date,
                FechaHasta = filtro.FechaHasta?.Date
            }, commandType: CommandType.StoredProcedure);
        });

    public Task<EventoResponse?> ObtenerAsync(int eventoId) =>
        TraductorErroresSql.EjecutarAsync(async () =>
        {
            using var cn = _repositorio.ObtenerConexion();
            return await cn.QueryFirstOrDefaultAsync<EventoResponse>("dbo.sp_Evento_ObtenerPorId",
                new { EventoId = eventoId }, commandType: CommandType.StoredProcedure);
        });

    public Task<int> CrearAsync(EventoRequest datos, string estado) =>
        TraductorErroresSql.EjecutarAsync(async () =>
        {
            using var cn = _repositorio.ObtenerConexion();
            return await cn.ExecuteScalarAsync<int>("dbo.sp_Evento_Crear", new
            {
                CodigoReferencia = string.IsNullOrWhiteSpace(datos.CodigoReferencia) ? null : datos.CodigoReferencia.Trim().ToUpperInvariant(),
                datos.CategoriaId, datos.LocacionId, datos.Nombre, datos.DescripcionCorta,
                datos.DescripcionCompleta, datos.Organizador, datos.ImagenUrl,
                FechaEvento = datos.FechaEvento.Date, datos.HoraEvento, Estado = estado
            }, commandType: CommandType.StoredProcedure);
        });

    public Task ActualizarAsync(int eventoId, EventoRequest datos) =>
        TraductorErroresSql.EjecutarAsync(async () =>
        {
            using var cn = _repositorio.ObtenerConexion();
            await cn.ExecuteAsync("dbo.sp_Evento_Actualizar", new
            {
                EventoId = eventoId,
                CodigoReferencia = string.IsNullOrWhiteSpace(datos.CodigoReferencia) ? null : datos.CodigoReferencia.Trim().ToUpperInvariant(),
                datos.CategoriaId, datos.LocacionId, datos.Nombre, datos.DescripcionCorta,
                datos.DescripcionCompleta, datos.Organizador, datos.ImagenUrl,
                FechaEvento = datos.FechaEvento.Date, datos.HoraEvento
            }, commandType: CommandType.StoredProcedure);
        });

    public Task CambiarEstadoAsync(int eventoId, string estado) =>
        TraductorErroresSql.EjecutarAsync(async () =>
        {
            using var cn = _repositorio.ObtenerConexion();
            await cn.ExecuteAsync("dbo.sp_Evento_CambiarEstado", new { EventoId = eventoId, Estado = estado },
                commandType: CommandType.StoredProcedure);
        });

    public Task<string> EliminarAsync(int eventoId) =>
        TraductorErroresSql.EjecutarAsync(async () =>
        {
            using var cn = _repositorio.ObtenerConexion();
            return await cn.ExecuteScalarAsync<string>("dbo.sp_Evento_Eliminar", new { EventoId = eventoId },
                commandType: CommandType.StoredProcedure) ?? string.Empty;
        });
}

using System.Data;
using System.Text.Json;
using Dapper;
using EventoTickets.Abstracciones.Interfaces.DA;
using EventoTickets.Abstracciones.Modelos;

namespace EventoTickets.DA.Repositorios;


public class PedidoDA : IPedidoDA
{
    private readonly IRepositorioDapper _repositorio;

    public PedidoDA(IRepositorioDapper repositorio) => _repositorio = repositorio;

    public Task<int> CrearAsync(int usuarioId, int eventoId, string metodoPago, IEnumerable<PedidoLineaRequest> lineas,
                                decimal porcentajeCargo, decimal porcentajeIva, int minutosReserva, int maxEntradas) =>
        TraductorErroresSql.EjecutarAsync(async () =>
        {
            // El procedimiento lee el JSON con OPENJSON ('$.TipoEntradaId', '$.Cantidad')
            var detallesJson = JsonSerializer.Serialize(
                lineas.Select(l => new { l.TipoEntradaId, l.Cantidad }));

            using var cn = _repositorio.ObtenerConexion();
            return await cn.ExecuteScalarAsync<int>("dbo.sp_Pedido_Crear", new
            {
                UsuarioId = usuarioId,
                EventoId = eventoId,
                MetodoPago = metodoPago,
                DetallesJson = detallesJson,
                PorcentajeCargo = porcentajeCargo,
                PorcentajeIVA = porcentajeIva,
                MinutosReserva = minutosReserva,
                MaxEntradasPorPedido = maxEntradas
            }, commandType: CommandType.StoredProcedure);
        });

    public Task<PedidoCompletoResponse?> ObtenerAsync(int pedidoId) =>
        TraductorErroresSql.EjecutarAsync(async () =>
        {
            using var cn = _repositorio.ObtenerConexion();
            using var lector = await cn.QueryMultipleAsync("dbo.sp_Pedido_ObtenerPorId", new { PedidoId = pedidoId },
                commandType: CommandType.StoredProcedure);
            var pedido = await lector.ReadFirstOrDefaultAsync<PedidoCompletoResponse>();
            if (pedido is null) return null;
            pedido.Detalles = (await lector.ReadAsync<PedidoDetalleResponse>()).ToList();
            return pedido;
        });

    public Task<IEnumerable<PedidoResponse>> ListarPorUsuarioAsync(int usuarioId, string? estado) =>
        TraductorErroresSql.EjecutarAsync(async () =>
        {
            using var cn = _repositorio.ObtenerConexion();
            return await cn.QueryAsync<PedidoResponse>("dbo.sp_Pedido_ListarPorUsuario",
                new { UsuarioId = usuarioId, Estado = string.IsNullOrWhiteSpace(estado) ? null : estado },
                commandType: CommandType.StoredProcedure);
        });

    public Task<IEnumerable<PedidoResponse>> ListarAsync(FiltroPedidos filtro) =>
        TraductorErroresSql.EjecutarAsync(async () =>
        {
            using var cn = _repositorio.ObtenerConexion();
            return await cn.QueryAsync<PedidoResponse>("dbo.sp_Pedido_Listar", new
            {
                Estado = string.IsNullOrWhiteSpace(filtro.Estado) ? null : filtro.Estado,
                filtro.EventoId,
                Busqueda = string.IsNullOrWhiteSpace(filtro.Busqueda) ? null : filtro.Busqueda.Trim()
            }, commandType: CommandType.StoredProcedure);
        });

    public Task ActualizarMetodoPagoAsync(int pedidoId, int? usuarioId, string metodoPago) =>
        TraductorErroresSql.EjecutarAsync(async () =>
        {
            using var cn = _repositorio.ObtenerConexion();
            await cn.ExecuteAsync("dbo.sp_Pedido_ActualizarMetodoPago",
                new { PedidoId = pedidoId, UsuarioId = usuarioId, MetodoPago = metodoPago }, commandType: CommandType.StoredProcedure);
        });

    public Task CancelarAsync(int pedidoId, int? usuarioId) =>
        TraductorErroresSql.EjecutarAsync(async () =>
        {
            using var cn = _repositorio.ObtenerConexion();
            await cn.ExecuteAsync("dbo.sp_Pedido_Cancelar", new { PedidoId = pedidoId, UsuarioId = usuarioId },
                commandType: CommandType.StoredProcedure);
        });

    public Task<int> LiberarVencidosAsync() =>
        TraductorErroresSql.EjecutarAsync(async () =>
        {
            using var cn = _repositorio.ObtenerConexion();
            return await cn.ExecuteScalarAsync<int>("dbo.sp_Pedido_LiberarVencidos", commandType: CommandType.StoredProcedure);
        });
}

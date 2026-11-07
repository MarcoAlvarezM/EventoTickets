using System.Data;
using EventoTickets.Pruebas.Nucleo;
using Microsoft.Data.SqlClient;
using Xunit;

namespace EventoTickets.Pruebas.Modulo3_Pedidos;

/// <summary>
/// HU-12 · Integración con SQL Server real (EventCR_BoleteriaPro_DB con datos iniciales).
/// Se omite si no existe la variable EVENTCR_PRUEBAS_SQL. Úsela en una base de pruebas:
/// deja un pedido de prueba en estado Cancelado (los pedidos nunca se borran).
/// </summary>
[Trait("Modulo", "M3")]
[Trait("Categoria", "Integracion")]
public class ConcurrenciaReservaPruebas
{
    private static string Cadena => Environment.GetEnvironmentVariable(FactConSqlServerAttribute.Variable)!;

    private static async Task<int?> EjecutarPedidoAsync(int tipoEntradaId, int eventoId, int cantidad, Barrier barrera)
    {
        await using var cn = new SqlConnection(Cadena);
        await cn.OpenAsync();
        await using var cmd = new SqlCommand("dbo.sp_Pedido_Crear", cn) { CommandType = CommandType.StoredProcedure };
        cmd.Parameters.AddWithValue("@UsuarioId", 6);
        cmd.Parameters.AddWithValue("@EventoId", eventoId);
        cmd.Parameters.AddWithValue("@MetodoPago", "SINPE Movil");
        cmd.Parameters.AddWithValue("@DetallesJson", $"[{{\"TipoEntradaId\":{tipoEntradaId},\"Cantidad\":{cantidad}}}]");
        cmd.Parameters.AddWithValue("@PorcentajeCargo", 10m);
        cmd.Parameters.AddWithValue("@PorcentajeIVA", 13m);
        cmd.Parameters.AddWithValue("@MinutosReserva", 30);
        cmd.Parameters.AddWithValue("@MaxEntradasPorPedido", 100000);
        barrera.SignalAndWait(TimeSpan.FromSeconds(10));
        try { return Convert.ToInt32(await cmd.ExecuteScalarAsync()); }
        catch (SqlException ex) when (ex.Number == 50409) { return null; }
    }

    /// <summary>HU-12 · Dos compras simultáneas del último cupo: exactamente una lo obtiene.</summary>
    [FactConSqlServer]
    public async Task Doble_reserva_simultanea_solo_una_obtiene_el_cupo()
    {
        int tipo, evento, disponible;
        await using (var cn = new SqlConnection(Cadena))
        {
            await cn.OpenAsync();
            await using var cmd = new SqlCommand(@"SELECT TOP (1) t.TipoEntradaId, t.EventoId, dbo.fn_DisponibilidadTipoEntrada(t.TipoEntradaId)
                FROM dbo.TiposEntrada t INNER JOIN dbo.Eventos e ON e.EventoId = t.EventoId
                WHERE e.Estado = 'Publicado' AND t.Activo = 1 AND dbo.fn_DisponibilidadTipoEntrada(t.TipoEntradaId) > 0
                ORDER BY dbo.fn_DisponibilidadTipoEntrada(t.TipoEntradaId)", cn);
            await using var r = await cmd.ExecuteReaderAsync();
            Assert.True(await r.ReadAsync(), "No hay tipos de entrada con cupo para la prueba.");
            (tipo, evento, disponible) = (r.GetInt32(0), r.GetInt32(1), r.GetInt32(2));
        }

        using var barrera = new Barrier(2);
        var resultados = await Task.WhenAll(
            Task.Run(() => EjecutarPedidoAsync(tipo, evento, disponible, barrera)),
            Task.Run(() => EjecutarPedidoAsync(tipo, evento, disponible, barrera)));

        var creados = resultados.Where(r => r.HasValue).Select(r => r!.Value).ToList();
        try
        {
            Assert.Single(creados);
        }
        finally
        {
            foreach (var pedidoId in creados)
            {
                await using var cn = new SqlConnection(Cadena);
                await cn.OpenAsync();
                await using var cancelar = new SqlCommand("dbo.sp_Pedido_Cancelar", cn) { CommandType = CommandType.StoredProcedure };
                cancelar.Parameters.AddWithValue("@PedidoId", pedidoId);
                cancelar.Parameters.AddWithValue("@UsuarioId", DBNull.Value);
                await cancelar.ExecuteNonQueryAsync();
            }
        }
    }
}

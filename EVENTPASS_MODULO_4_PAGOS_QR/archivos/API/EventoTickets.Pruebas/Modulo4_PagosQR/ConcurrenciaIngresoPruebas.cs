using System.Data;
using EventoTickets.Pruebas.Nucleo;
using Microsoft.Data.SqlClient;
using Xunit;

namespace EventoTickets.Pruebas.Modulo4_PagosQR;

/// <summary>
/// HU-19 · Integración con SQL Server real. Se omite si no existe EVENTCR_PRUEBAS_SQL.
/// Restaura la entrada usada al terminar.
/// </summary>
[Trait("Modulo", "M4")]
[Trait("Categoria", "Integracion")]
public class ConcurrenciaIngresoPruebas
{
    private static string Cadena => Environment.GetEnvironmentVariable(FactConSqlServerAttribute.Variable)!;

    /// <summary>HU-19 · Dos lecturas simultáneas del mismo QR: exactamente un ingreso (la entrada se restaura al final).</summary>
    [FactConSqlServer]
    public async Task Doble_ingreso_simultaneo_solo_uno_es_autorizado()
    {
        int entradaId; string codigo; object puerta;
        await using (var cn = new SqlConnection(Cadena))
        {
            await cn.OpenAsync();
            await using var cmd = new SqlCommand("SELECT TOP (1) EntradaId, CodigoTicket, PuertaAcceso FROM dbo.Entradas WHERE Estado = 'Valida' ORDER BY EntradaId", cn);
            await using var r = await cmd.ExecuteReaderAsync();
            Assert.True(await r.ReadAsync(), "No hay entradas válidas para la prueba.");
            (entradaId, codigo, puerta) = (r.GetInt32(0), r.GetString(1), r.IsDBNull(2) ? DBNull.Value : r.GetString(2));
        }

        using var barrera = new Barrier(2);
        async Task<int> IngresarAsync()
        {
            await using var cn = new SqlConnection(Cadena);
            await cn.OpenAsync();
            await using var cmd = new SqlCommand("dbo.sp_Entrada_RegistrarIngreso", cn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@CodigoTicket", codigo);
            cmd.Parameters.AddWithValue("@HashCriptografico", DBNull.Value);
            cmd.Parameters.AddWithValue("@ValidadorUsuarioId", 4);
            cmd.Parameters.AddWithValue("@PuertaAcceso", DBNull.Value);
            barrera.SignalAndWait(TimeSpan.FromSeconds(10));
            try { await using var r = await cmd.ExecuteReaderAsync(); return 0; }
            catch (SqlException ex) { return ex.Number; }
        }

        try
        {
            var codigos = await Task.WhenAll(Task.Run(IngresarAsync), Task.Run(IngresarAsync));
            Assert.Equal(1, codigos.Count(c => c == 0));
            Assert.Equal(1, codigos.Count(c => c == 50043));
        }
        finally
        {
            await using var cn = new SqlConnection(Cadena);
            await cn.OpenAsync();
            await using var restaurar = new SqlCommand("UPDATE dbo.Entradas SET Estado = 'Valida', FechaHoraIngreso = NULL, ValidadorPuertaUsuarioId = NULL, PuertaAcceso = @Puerta WHERE EntradaId = @Id", cn);
            restaurar.Parameters.AddWithValue("@Id", entradaId);
            restaurar.Parameters.AddWithValue("@Puerta", puerta);
            await restaurar.ExecuteNonQueryAsync();
        }
    }
}

using EventoTickets.Abstracciones.Excepciones;
using Microsoft.Data.SqlClient;

namespace EventoTickets.DA.Repositorios;


/// <summary>
/// Convierte los errores THROW de los procedimientos almacenados (50000-50999) y las
/// violaciones de restricciones en excepciones de dominio con su código HTTP.
/// </summary>
internal static class TraductorErroresSql
{
    public static async Task<T> EjecutarAsync<T>(Func<Task<T>> accion)
    {
        try
        {
            return await accion();
        }
        catch (SqlException ex)
        {
            throw Traducir(ex);
        }
    }

    public static async Task EjecutarAsync(Func<Task> accion)
    {
        try
        {
            await accion();
        }
        catch (SqlException ex)
        {
            throw Traducir(ex);
        }
    }

    private static Exception Traducir(SqlException ex) => ex.Number switch
    {
        50404 => new RecursoNoEncontradoException(ex.Message),
        50409 => new ConflictoException(ex.Message),
        50403 => new AccesoDenegadoException(ex.Message),
        >= 50000 and < 51000 => new ReglaNegocioException(ex.Message, ex.Number),
        2627 or 2601 => new ConflictoException("Ya existe un registro con esos datos únicos."),
        547 => new ReglaNegocioException("La operación viola una regla de integridad de los datos."),
        _ => ex
    };
}

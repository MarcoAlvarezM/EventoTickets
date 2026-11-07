using EventoTickets.Abstracciones.Excepciones;
using EventoTickets.Abstracciones.Modelos;

namespace EventoTickets.API.Middleware;

/// <summary>
/// CONTRATO COMPARTIDO: convierte las excepciones en <see cref="RespuestaApi{T}"/> con el código HTTP correcto.
/// Excepciones de dominio (400/401/403/404/409) muestran su mensaje; las no controladas se registran y
/// devuelven 500 sin exponer detalles internos.
/// </summary>
public class ManejoExcepcionesMiddleware : IMiddleware
{
    private readonly ILogger<ManejoExcepcionesMiddleware> _logger;

    public ManejoExcepcionesMiddleware(ILogger<ManejoExcepcionesMiddleware> logger) => _logger = logger;

    public async Task InvokeAsync(HttpContext contexto, RequestDelegate siguiente)
    {
        try
        {
            await siguiente(contexto);
        }
        catch (EventoTicketsException ex)
        {
            await EscribirAsync(contexto, RespuestaApi.Error(ex.CodigoHttp, ex.Message, ex.CodigoError));
        }
        catch (UnauthorizedAccessException ex)
        {
            await EscribirAsync(contexto, RespuestaApi.Error(StatusCodes.Status403Forbidden, ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error no controlado en {Ruta}", contexto.Request.Path);
            await EscribirAsync(contexto, RespuestaApi.Error(StatusCodes.Status500InternalServerError,
                "Ocurrió un error inesperado. Intente de nuevo más tarde."));
        }
    }

    public static async Task EscribirAsync(HttpContext contexto, RespuestaApi<object> respuesta)
    {
        if (contexto.Response.HasStarted) return;
        contexto.Response.Clear();
        contexto.Response.StatusCode = respuesta.Codigo;
        await contexto.Response.WriteAsJsonAsync(respuesta);
    }
}

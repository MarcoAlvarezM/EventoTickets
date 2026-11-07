using EventoTickets.Abstracciones.Modelos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EventoTickets.API.Nucleo.Filtros;

/// <summary>
/// CONTRATO COMPARTIDO: envuelve toda respuesta de datos (Ok, CreatedAtAction…) en <see cref="RespuestaApi{T}"/>.
/// Los módulos no deben construir su propio formato: basta con devolver Ok(datos).
/// Los archivos (File) no se envuelven.
/// </summary>
public class RespuestaApiFiltro : IAsyncResultFilter
{
    public Task OnResultExecutionAsync(ResultExecutingContext contexto, ResultExecutionDelegate siguiente)
    {
        if (contexto.Result is ObjectResult resultado && !EsRespuestaApi(resultado.Value))
        {
            var codigo = resultado.StatusCode ?? StatusCodes.Status200OK;
            resultado.Value = codigo < 400
                ? RespuestaApi.Correcta(resultado.Value, codigo)
                : RespuestaApi.Error(codigo, resultado.Value?.ToString() ?? "Error");
            resultado.DeclaredType = null;
        }
        return siguiente();
    }

    private static bool EsRespuestaApi(object? valor) =>
        valor is not null && valor.GetType().IsGenericType && valor.GetType().GetGenericTypeDefinition() == typeof(RespuestaApi<>);
}

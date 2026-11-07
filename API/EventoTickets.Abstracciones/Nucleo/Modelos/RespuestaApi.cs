namespace EventoTickets.Abstracciones.Modelos;

/// <summary>
/// CONTRATO COMPARTIDO: estructura única de todas las respuestas JSON del API (éxito y error).
/// Los controladores devuelven el objeto de datos; el filtro global <c>RespuestaApiFiltro</c> lo envuelve.
/// Los errores los produce <c>ManejoExcepcionesMiddleware</c> y la validación automática de modelos.
/// </summary>
public class RespuestaApi<T>
{
    public bool Exito { get; set; }
    /// <summary>Código HTTP de la respuesta (200, 201, 400, 401, 403, 404, 409, 500).</summary>
    public int Codigo { get; set; }
    public string? Mensaje { get; set; }
    public T? Datos { get; set; }
    /// <summary>Errores de validación por campo (solo cuando Codigo = 400 por Data Annotations).</summary>
    public Dictionary<string, string[]>? Errores { get; set; }
    /// <summary>Número de error de negocio (THROW 50xxx de los procedimientos), si aplica.</summary>
    public int? CodigoError { get; set; }
    public DateTime FechaUtc { get; set; } = DateTime.UtcNow;
}

/// <summary>Fábricas de <see cref="RespuestaApi{T}"/>.</summary>
public static class RespuestaApi
{
    public static RespuestaApi<T> Correcta<T>(T? datos, int codigo = 200, string? mensaje = null) =>
        new() { Exito = true, Codigo = codigo, Datos = datos, Mensaje = mensaje };

    public static RespuestaApi<object> Mensaje(string mensaje, int codigo = 200) =>
        new() { Exito = true, Codigo = codigo, Mensaje = mensaje };

    public static RespuestaApi<object> Error(int codigo, string mensaje, int? codigoError = null, Dictionary<string, string[]>? errores = null) =>
        new() { Exito = false, Codigo = codigo, Mensaje = mensaje, CodigoError = codigoError, Errores = errores };
}

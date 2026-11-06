namespace EventoTickets.Web.Abstracciones.Modelos;


/// <summary>Respuesta normalizada de una llamada HTTP al API.</summary>
public class ResultadoApi<T>
{
    public bool Exito { get; set; }
    public int CodigoEstado { get; set; }
    public T? Datos { get; set; }
    public string? Mensaje { get; set; }
    /// <summary>Errores de validación por campo (ValidationProblemDetails.errors).</summary>
    public Dictionary<string, string[]> Errores { get; set; } = new();

    public bool NoAutenticado => CodigoEstado == 401;
    public bool NoEncontrado => CodigoEstado == 404;
}

/// <summary>Marcador para respuestas 204 No Content.</summary>
public sealed class SinContenido { }

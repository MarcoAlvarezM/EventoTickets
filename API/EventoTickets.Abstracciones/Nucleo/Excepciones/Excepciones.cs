namespace EventoTickets.Abstracciones.Excepciones;


/// <summary>Excepción base del dominio; lleva el código HTTP con el que debe responder la API.</summary>
public abstract class EventoTicketsException : Exception
{
    protected EventoTicketsException(string mensaje, int codigoHttp, int? codigoError = null) : base(mensaje)
    {
        CodigoHttp = codigoHttp;
        CodigoError = codigoError;
    }

    public int CodigoHttp { get; }
    public int? CodigoError { get; }
}

/// <summary>Regla de negocio incumplida (HTTP 400).</summary>
public sealed class ReglaNegocioException : EventoTicketsException
{
    public ReglaNegocioException(string mensaje, int? codigoError = null) : base(mensaje, 400, codigoError) { }
}

/// <summary>Recurso inexistente (HTTP 404).</summary>
public sealed class RecursoNoEncontradoException : EventoTicketsException
{
    public RecursoNoEncontradoException(string mensaje) : base(mensaje, 404, 50404) { }
}

/// <summary>Conflicto con el estado actual: duplicados o falta de inventario (HTTP 409).</summary>
public sealed class ConflictoException : EventoTicketsException
{
    public ConflictoException(string mensaje) : base(mensaje, 409, 50409) { }
}

/// <summary>El usuario autenticado no puede operar sobre el recurso (HTTP 403).</summary>
public sealed class AccesoDenegadoException : EventoTicketsException
{
    public AccesoDenegadoException(string mensaje) : base(mensaje, 403, 50403) { }
}

/// <summary>Credenciales inválidas o cuenta no habilitada (HTTP 401).</summary>
public sealed class AutenticacionException : EventoTicketsException
{
    public AutenticacionException(string mensaje) : base(mensaje, 401) { }
}

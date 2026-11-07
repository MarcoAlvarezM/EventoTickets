using EventoTickets.Abstracciones.Constantes;
using EventoTickets.Abstracciones.Excepciones;
using EventoTickets.Abstracciones.Interfaces.DA;
using EventoTickets.Abstracciones.Interfaces.Flujo;
using EventoTickets.Abstracciones.Interfaces.Reglas;
using EventoTickets.Abstracciones.Modelos;

namespace EventoTickets.Flujo;


public class TipoEntradaFlujo : ITipoEntradaFlujo
{
    private readonly ITipoEntradaDA _tipoEntradaDA;
    private readonly IEventoDA _eventoDA;

    public TipoEntradaFlujo(ITipoEntradaDA tipoEntradaDA, IEventoDA eventoDA)
    {
        _tipoEntradaDA = tipoEntradaDA;
        _eventoDA = eventoDA;
    }

    public async Task<IEnumerable<TipoEntradaResponse>> ListarPorEventoAsync(int eventoId, bool soloActivos)
    {
        _ = await _eventoDA.ObtenerAsync(eventoId) ?? throw new RecursoNoEncontradoException("El evento no existe.");
        return await _tipoEntradaDA.ListarPorEventoAsync(eventoId, soloActivos);
    }

    public async Task<TipoEntradaResponse> ObtenerAsync(int tipoEntradaId) =>
        await _tipoEntradaDA.ObtenerAsync(tipoEntradaId) ?? throw new RecursoNoEncontradoException("El tipo de entrada no existe.");

    public async Task<TipoEntradaResponse> CrearAsync(TipoEntradaRequest solicitud)
    {
        solicitud.ColorHex = NormalizarColor(solicitud.ColorHex);
        var id = await _tipoEntradaDA.CrearAsync(solicitud);
        return await ObtenerAsync(id);
    }

    public async Task<TipoEntradaResponse> ActualizarAsync(int tipoEntradaId, TipoEntradaBase solicitud)
    {
        solicitud.ColorHex = NormalizarColor(solicitud.ColorHex);
        await _tipoEntradaDA.ActualizarAsync(tipoEntradaId, solicitud);
        return await ObtenerAsync(tipoEntradaId);
    }

    public async Task<ResultadoEliminacion> EliminarAsync(int tipoEntradaId)
    {
        var resultado = await _tipoEntradaDA.EliminarAsync(tipoEntradaId);
        return new ResultadoEliminacion
        {
            Resultado = resultado,
            Mensaje = resultado == "Desactivado"
                ? "El tipo de entrada tiene ventas: se desactivó para nuevas compras."
                : "Tipo de entrada eliminado."
        };
    }

    private static string? NormalizarColor(string? color) =>
        string.IsNullOrWhiteSpace(color) ? null : color.Trim().ToUpperInvariant();
}

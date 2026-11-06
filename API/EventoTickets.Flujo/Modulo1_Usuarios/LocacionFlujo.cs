using EventoTickets.Abstracciones.Excepciones;
using EventoTickets.Abstracciones.Interfaces.DA;
using EventoTickets.Abstracciones.Interfaces.Flujo;
using EventoTickets.Abstracciones.Modelos;

namespace EventoTickets.Flujo;


public class LocacionFlujo : ILocacionFlujo
{
    private readonly ILocacionDA _locacionDA;

    public LocacionFlujo(ILocacionDA locacionDA) => _locacionDA = locacionDA;

    public Task<IEnumerable<LocacionResponse>> ListarAsync(bool soloActivas, string? busqueda) =>
        _locacionDA.ListarAsync(soloActivas, string.IsNullOrWhiteSpace(busqueda) ? null : busqueda.Trim());

    public async Task<LocacionResponse> ObtenerAsync(int locacionId) =>
        await _locacionDA.ObtenerAsync(locacionId) ?? throw new RecursoNoEncontradoException("La locación no existe.");

    public async Task<LocacionResponse> CrearAsync(LocacionRequest solicitud)
    {
        var id = await _locacionDA.CrearAsync(solicitud);
        return await ObtenerAsync(id);
    }

    public async Task<LocacionResponse> ActualizarAsync(int locacionId, LocacionRequest solicitud)
    {
        await _locacionDA.ActualizarAsync(locacionId, solicitud);
        return await ObtenerAsync(locacionId);
    }

    public async Task<ResultadoEliminacion> EliminarAsync(int locacionId)
    {
        var resultado = await _locacionDA.EliminarAsync(locacionId);
        return new ResultadoEliminacion
        {
            Resultado = resultado,
            Mensaje = resultado == "Desactivada"
                ? "La locación tiene eventos asociados: se desactivó en lugar de eliminarse."
                : "Locación eliminada."
        };
    }
}

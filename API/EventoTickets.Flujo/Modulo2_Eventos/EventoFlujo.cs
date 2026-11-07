using EventoTickets.Abstracciones.Constantes;
using EventoTickets.Abstracciones.Excepciones;
using EventoTickets.Abstracciones.Interfaces.DA;
using EventoTickets.Abstracciones.Interfaces.Flujo;
using EventoTickets.Abstracciones.Interfaces.Reglas;
using EventoTickets.Abstracciones.Modelos;

namespace EventoTickets.Flujo;


public class EventoFlujo : IEventoFlujo
{
    private readonly IEventoDA _eventoDA;
    private readonly IEventoReglas _eventoReglas;

    public EventoFlujo(IEventoDA eventoDA, IEventoReglas eventoReglas)
    {
        _eventoDA = eventoDA;
        _eventoReglas = eventoReglas;
    }

    public Task<IEnumerable<EventoResponse>> ListarAsync(FiltroEventos filtro, bool esPersonal)
    {
        if (!esPersonal)
        {
            // HU-06: el público solo ve la cartelera vigente (Publicado o Agotado)
            filtro.Estado = null;
            filtro.SoloCartelera = true;
            filtro.SoloVigentes = true;
        }
        return _eventoDA.ListarAsync(filtro);
    }

    public async Task<EventoResponse> ObtenerAsync(int eventoId, bool esPersonal)
    {
        var evento = await _eventoDA.ObtenerAsync(eventoId) ?? throw new RecursoNoEncontradoException("El evento no existe.");
        if (!esPersonal && evento.Estado == EstadosEvento.Borrador)
            throw new RecursoNoEncontradoException("El evento no existe.");
        return evento;
    }

    public async Task<EventoResponse> CrearAsync(EventoRequest solicitud)
    {
        _eventoReglas.ValidarFechaProgramada(solicitud.FechaEvento);
        var id = await _eventoDA.CrearAsync(solicitud, EstadosEvento.Borrador);
        return await ObtenerAsync(id, true);
    }

    public async Task<EventoResponse> ActualizarAsync(int eventoId, EventoRequest solicitud)
    {
        var actual = await ObtenerAsync(eventoId, true);
        if (actual.Estado is EstadosEvento.Cancelado or EstadosEvento.Finalizado)
            throw new ReglaNegocioException("No se puede editar un evento cancelado o finalizado.");
        if (solicitud.FechaEvento.Date != actual.FechaEvento.Date)
            _eventoReglas.ValidarFechaProgramada(solicitud.FechaEvento);

        await _eventoDA.ActualizarAsync(eventoId, solicitud);
        return await ObtenerAsync(eventoId, true);
    }

    public async Task<EventoResponse> CambiarEstadoAsync(int eventoId, string estado)
    {
        var actual = await ObtenerAsync(eventoId, true);
        _eventoReglas.ValidarTransicionEstado(actual.Estado, estado);
        if (estado == EstadosEvento.Publicado)
            _eventoReglas.ValidarFechaProgramada(actual.FechaEvento);
        await _eventoDA.CambiarEstadoAsync(eventoId, estado);
        return await ObtenerAsync(eventoId, true);
    }

    public async Task<ResultadoEliminacion> EliminarAsync(int eventoId)
    {
        var resultado = await _eventoDA.EliminarAsync(eventoId);
        return new ResultadoEliminacion { Resultado = resultado, Mensaje = "Evento eliminado junto con sus tipos de entrada." };
    }
}

using EventoTickets.Abstracciones.Constantes;
using EventoTickets.Abstracciones.Excepciones;
using EventoTickets.Abstracciones.Interfaces.Reglas;

namespace EventoTickets.Reglas;


/// <summary>Ciclo de vida (HU-05): Borrador → Publicado ⇄ Agotado → Finalizado; Cancelado es terminal.</summary>
public class EventoReglas : IEventoReglas
{
    private static readonly Dictionary<string, string[]> Transiciones = new()
    {
        [EstadosEvento.Borrador] = new[] { EstadosEvento.Publicado, EstadosEvento.Cancelado },
        [EstadosEvento.Publicado] = new[] { EstadosEvento.Borrador, EstadosEvento.Finalizado, EstadosEvento.Cancelado },
        // Agotado lo asigna la base de datos (trigger); manualmente solo puede finalizarse o cancelarse
        [EstadosEvento.Agotado] = new[] { EstadosEvento.Finalizado, EstadosEvento.Cancelado },
        [EstadosEvento.Finalizado] = Array.Empty<string>(),
        [EstadosEvento.Cancelado] = Array.Empty<string>()
    };

    public void ValidarTransicionEstado(string estadoActual, string estadoNuevo)
    {
        if (estadoActual == estadoNuevo)
            throw new ReglaNegocioException($"El evento ya se encuentra en estado {estadoNuevo}.");
        if (!Transiciones.TryGetValue(estadoActual, out var permitidos) || !permitidos.Contains(estadoNuevo))
            throw new ReglaNegocioException($"No se permite cambiar un evento de {estadoActual} a {estadoNuevo}.");
    }

    public void ValidarFechaProgramada(DateTime fechaEvento)
    {
        if (fechaEvento.Date < DateTime.Today)
            throw new ReglaNegocioException("La fecha del evento no puede ser anterior a hoy.");
    }
}

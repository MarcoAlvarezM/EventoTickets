using EventoTickets.Abstracciones.Modelos;
using Microsoft.AspNetCore.Http;

namespace EventoTickets.Abstracciones.Interfaces.Reglas;


public interface IEventoReglas
{
    void ValidarTransicionEstado(string estadoActual, string estadoNuevo);
    void ValidarFechaProgramada(DateTime fechaEvento);
}

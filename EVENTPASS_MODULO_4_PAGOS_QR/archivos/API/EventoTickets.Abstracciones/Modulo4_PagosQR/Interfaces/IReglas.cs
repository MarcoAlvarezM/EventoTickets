using EventoTickets.Abstracciones.Modelos;
using Microsoft.AspNetCore.Http;

namespace EventoTickets.Abstracciones.Interfaces.Reglas;


public interface IPagoReglas
{
    void ValidarArchivo(IFormFile? archivo);
    void ValidarMonto(decimal montoReportado, decimal importeTotal);
}

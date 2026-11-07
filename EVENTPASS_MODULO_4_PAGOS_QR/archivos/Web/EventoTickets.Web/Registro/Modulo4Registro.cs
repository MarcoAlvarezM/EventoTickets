using EventoTickets.Web.Abstracciones.Interfaces;
using EventoTickets.Web.Servicios;

namespace EventoTickets.Web.Registro;

/// <summary>MÓDULO 4 — Pagos, auditoría y entradas QR: servicios HTTP que consumen el API.</summary>
public static class Modulo4Registro
{
    public static IServiceCollection AgregarModulo4PagosQR(this IServiceCollection servicios)
    {
        servicios.AddScoped<IPagoServicio, PagoServicio>();
        servicios.AddScoped<IEntradaServicio, EntradaServicio>();
        return servicios;
    }
}

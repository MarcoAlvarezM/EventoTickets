using EventoTickets.Web.Abstracciones.Interfaces;
using EventoTickets.Web.Servicios;

namespace EventoTickets.Web.Registro;

/// <summary>MÓDULO 2 — Eventos, cartelera, localidades e inventario: servicios HTTP que consumen el API.</summary>
public static class Modulo2Registro
{
    public static IServiceCollection AgregarModulo2Eventos(this IServiceCollection servicios)
    {
        servicios.AddScoped<IEventoServicio, EventoServicio>();
        servicios.AddScoped<ITipoEntradaServicio, TipoEntradaServicio>();
        servicios.AddScoped<IReporteServicio, ReporteServicio>();
        return servicios;
    }
}

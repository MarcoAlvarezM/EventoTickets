using EventoTickets.Abstracciones.Interfaces.DA;
using EventoTickets.Abstracciones.Interfaces.Flujo;
using EventoTickets.Abstracciones.Interfaces.Reglas;
using EventoTickets.DA.Repositorios;
using EventoTickets.Flujo;
using EventoTickets.Reglas;

namespace EventoTickets.API.Registro;

/// <summary>MÓDULO 2 — Eventos, cartelera, localidades, inventario y panel (HU-04, 05, 06, 09, 10, 11).</summary>
public static class Modulo2Registro
{
    public static IServiceCollection AgregarModulo2Eventos(this IServiceCollection servicios)
    {
        servicios.AddScoped<IEventoDA, EventoDA>();
        servicios.AddScoped<ITipoEntradaDA, TipoEntradaDA>();
        servicios.AddScoped<IReporteDA, ReporteDA>();
        servicios.AddSingleton<IEventoReglas, EventoReglas>();
        servicios.AddScoped<IEventoFlujo, EventoFlujo>();
        servicios.AddScoped<ITipoEntradaFlujo, TipoEntradaFlujo>();
        servicios.AddScoped<IReporteFlujo, ReporteFlujo>();
        return servicios;
    }
}

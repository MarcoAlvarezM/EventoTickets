using EventoTickets.Abstracciones.Interfaces.DA;
using EventoTickets.Abstracciones.Interfaces.Flujo;
using EventoTickets.Abstracciones.Interfaces.Reglas;
using EventoTickets.DA.Repositorios;
using EventoTickets.Flujo;
using EventoTickets.Reglas;
using EventoTickets.Servicios;

namespace EventoTickets.API.Registro;

/// <summary>MÓDULO 3 — Pedidos, detalle de órdenes y reservas (HU-12, 13, 14).</summary>
public static class Modulo3Registro
{
    public static IServiceCollection AgregarModulo3Pedidos(this IServiceCollection servicios)
    {
        servicios.AddScoped<IPedidoDA, PedidoDA>();
        servicios.AddSingleton<IPedidoReglas, PedidoReglas>();
        servicios.AddScoped<IPedidoFlujo, PedidoFlujo>();
        // HU-14: expiración automática de reservas en segundo plano
        servicios.AddHostedService<ExpiracionReservasServicio>();
        return servicios;
    }
}

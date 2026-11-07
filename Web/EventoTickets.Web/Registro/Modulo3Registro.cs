using EventoTickets.Web.Abstracciones.Interfaces;
using EventoTickets.Web.Servicios;

namespace EventoTickets.Web.Registro;

/// <summary>MÓDULO 3 — Pedidos y reservas: servicios HTTP que consumen el API.</summary>
public static class Modulo3Registro
{
    public static IServiceCollection AgregarModulo3Pedidos(this IServiceCollection servicios)
    {
        servicios.AddScoped<IPedidoServicio, PedidoServicio>();
        return servicios;
    }
}

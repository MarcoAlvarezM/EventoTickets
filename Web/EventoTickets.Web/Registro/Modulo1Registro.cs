using EventoTickets.Web.Abstracciones.Interfaces;
using EventoTickets.Web.Servicios;

namespace EventoTickets.Web.Registro;

/// <summary>MÓDULO 1 — Usuarios, seguridad, categorías y locaciones: servicios HTTP que consumen el API.</summary>
public static class Modulo1Registro
{
    public static IServiceCollection AgregarModulo1Usuarios(this IServiceCollection servicios)
    {
        servicios.AddScoped<IAutenticacionServicio, AutenticacionServicio>();
        servicios.AddScoped<IUsuarioServicio, UsuarioServicio>();
        servicios.AddScoped<ICategoriaServicio, CategoriaServicio>();
        servicios.AddScoped<ILocacionServicio, LocacionServicio>();
        return servicios;
    }
}

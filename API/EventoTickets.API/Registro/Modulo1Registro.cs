using EventoTickets.Abstracciones.Interfaces.DA;
using EventoTickets.Abstracciones.Interfaces.Flujo;
using EventoTickets.Abstracciones.Interfaces.Reglas;
using EventoTickets.Abstracciones.Interfaces.Servicios;
using EventoTickets.DA.Repositorios;
using EventoTickets.Flujo;
using EventoTickets.Reglas;
using EventoTickets.Servicios;

namespace EventoTickets.API.Registro;

/// <summary>MÓDULO 1 — Usuarios, seguridad, categorías y locaciones (HU-01, 02, 03, 07, 08).</summary>
public static class Modulo1Registro
{
    public static IServiceCollection AgregarModulo1Usuarios(this IServiceCollection servicios)
    {
        servicios.AddScoped<IUsuarioDA, UsuarioDA>();
        servicios.AddScoped<ICategoriaDA, CategoriaDA>();
        servicios.AddScoped<ILocacionDA, LocacionDA>();
        servicios.AddSingleton<IPasswordReglas, PasswordReglas>();
        servicios.AddSingleton<IUsuarioReglas, UsuarioReglas>();
        servicios.AddSingleton<IServicioTokenJwt, ServicioTokenJwt>();
        servicios.AddScoped<IAutenticacionFlujo, AutenticacionFlujo>();
        servicios.AddScoped<IUsuarioFlujo, UsuarioFlujo>();
        servicios.AddScoped<ICategoriaFlujo, CategoriaFlujo>();
        servicios.AddScoped<ILocacionFlujo, LocacionFlujo>();
        return servicios;
    }
}

using EventoTickets.Abstracciones.Configuracion;
using EventoTickets.Abstracciones.Interfaces.DA;
using EventoTickets.Abstracciones.Interfaces.Servicios;
using EventoTickets.API.Middleware;
using EventoTickets.DA.Repositorios;
using EventoTickets.Servicios;

namespace EventoTickets.API.Registro;

/// <summary>NÚCLEO (responsable de integración): configuración tipada, conexión SQL, hash y manejo de errores.</summary>
public static class NucleoRegistro
{
    public static IServiceCollection AgregarNucleo(this IServiceCollection servicios, IConfiguration configuracion)
    {
        servicios.Configure<ConfiguracionVentas>(configuracion.GetSection(ConfiguracionVentas.Seccion));
        servicios.Configure<ConfiguracionJwt>(configuracion.GetSection(ConfiguracionJwt.Seccion));
        servicios.Configure<ConfiguracionAlmacenamiento>(configuracion.GetSection(ConfiguracionAlmacenamiento.Seccion));
        servicios.AddSingleton<IRepositorioDapper, RepositorioDapper>();
        servicios.AddSingleton<IServicioHash, ServicioHash>();
        servicios.AddTransient<ManejoExcepcionesMiddleware>();
        return servicios;
    }
}

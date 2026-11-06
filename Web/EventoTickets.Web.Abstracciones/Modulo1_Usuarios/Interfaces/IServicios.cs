using EventoTickets.Web.Abstracciones.Modelos;

namespace EventoTickets.Web.Abstracciones.Interfaces;


public interface IAutenticacionServicio
{
    Task<ResultadoApi<LoginResponse>> LoginAsync(LoginRequest solicitud);
    Task<ResultadoApi<LoginResponse>> RegistrarAsync(RegistroCompradorRequest solicitud);
}

public interface IUsuarioServicio
{
    Task<ResultadoApi<List<UsuarioResponse>>> ListarAsync(string? busqueda, string? rol, string? estado);
    Task<ResultadoApi<UsuarioResponse>> ObtenerAsync(int id);
    Task<ResultadoApi<UsuarioResponse>> CrearAsync(UsuarioCrearRequest solicitud);
    Task<ResultadoApi<UsuarioResponse>> ActualizarAsync(int id, UsuarioActualizarRequest solicitud);
    Task<ResultadoApi<SinContenido>> CambiarEstadoAsync(int id, string estado);
    Task<ResultadoApi<SinContenido>> CambiarPasswordAsync(int id, CambiarPasswordRequest solicitud);
    Task<ResultadoApi<ResultadoEliminacion>> EliminarAsync(int id);
}

public interface ICategoriaServicio
{
    Task<ResultadoApi<List<CategoriaResponse>>> ListarAsync(bool soloActivas);
    Task<ResultadoApi<CategoriaResponse>> ObtenerAsync(int id);
    Task<ResultadoApi<CategoriaResponse>> CrearAsync(CategoriaRequest solicitud);
    Task<ResultadoApi<CategoriaResponse>> ActualizarAsync(int id, CategoriaRequest solicitud);
    Task<ResultadoApi<ResultadoEliminacion>> EliminarAsync(int id);
}

public interface ILocacionServicio
{
    Task<ResultadoApi<List<LocacionResponse>>> ListarAsync(bool soloActivas, string? busqueda = null);
    Task<ResultadoApi<LocacionResponse>> ObtenerAsync(int id);
    Task<ResultadoApi<LocacionResponse>> CrearAsync(LocacionRequest solicitud);
    Task<ResultadoApi<LocacionResponse>> ActualizarAsync(int id, LocacionRequest solicitud);
    Task<ResultadoApi<ResultadoEliminacion>> EliminarAsync(int id);
}

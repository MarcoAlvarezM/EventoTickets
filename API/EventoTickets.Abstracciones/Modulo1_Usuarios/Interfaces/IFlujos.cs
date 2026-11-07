using EventoTickets.Abstracciones.Modelos;

namespace EventoTickets.Abstracciones.Interfaces.Flujo;


public interface IAutenticacionFlujo
{
    Task<UsuarioResponse> AutenticarAsync(LoginRequest solicitud);
    Task<UsuarioResponse> RegistrarCompradorAsync(RegistroCompradorRequest solicitud);
}

public interface IUsuarioFlujo
{
    Task<IEnumerable<UsuarioResponse>> ListarAsync(string? busqueda, string? rol, string? estado);
    Task<UsuarioResponse> ObtenerAsync(int usuarioId);
    Task<UsuarioResponse> CrearAsync(UsuarioCrearRequest solicitud);
    Task<UsuarioResponse> ActualizarAsync(int actorUsuarioId, int usuarioId, UsuarioActualizarRequest solicitud);
    Task CambiarEstadoAsync(int actorUsuarioId, int usuarioId, string estado);
    Task CambiarPasswordAsync(int usuarioId, CambiarPasswordRequest solicitud);
    Task<ResultadoEliminacion> EliminarAsync(int actorUsuarioId, int usuarioId);
}

public interface ICategoriaFlujo
{
    Task<IEnumerable<CategoriaResponse>> ListarAsync(bool soloActivas);
    Task<CategoriaResponse> ObtenerAsync(int categoriaId);
    Task<CategoriaResponse> CrearAsync(CategoriaRequest solicitud);
    Task<CategoriaResponse> ActualizarAsync(int categoriaId, CategoriaRequest solicitud);
    Task<ResultadoEliminacion> EliminarAsync(int categoriaId);
}

public interface ILocacionFlujo
{
    Task<IEnumerable<LocacionResponse>> ListarAsync(bool soloActivas, string? busqueda);
    Task<LocacionResponse> ObtenerAsync(int locacionId);
    Task<LocacionResponse> CrearAsync(LocacionRequest solicitud);
    Task<LocacionResponse> ActualizarAsync(int locacionId, LocacionRequest solicitud);
    Task<ResultadoEliminacion> EliminarAsync(int locacionId);
}

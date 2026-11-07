using EventoTickets.Abstracciones.Constantes;
using EventoTickets.Abstracciones.Excepciones;
using EventoTickets.Abstracciones.Interfaces.DA;
using EventoTickets.Abstracciones.Interfaces.Flujo;
using EventoTickets.Abstracciones.Interfaces.Reglas;
using EventoTickets.Abstracciones.Modelos;

namespace EventoTickets.Flujo;


public class UsuarioFlujo : IUsuarioFlujo
{
    private readonly IUsuarioDA _usuarioDA;
    private readonly IPasswordReglas _passwordReglas;
    private readonly IUsuarioReglas _usuarioReglas;

    public UsuarioFlujo(IUsuarioDA usuarioDA, IPasswordReglas passwordReglas, IUsuarioReglas usuarioReglas)
    {
        _usuarioDA = usuarioDA;
        _passwordReglas = passwordReglas;
        _usuarioReglas = usuarioReglas;
    }

    public Task<IEnumerable<UsuarioResponse>> ListarAsync(string? busqueda, string? rol, string? estado) =>
        _usuarioDA.ListarAsync(Limpio(busqueda), Limpio(rol), Limpio(estado));

    public async Task<UsuarioResponse> ObtenerAsync(int usuarioId) =>
        await _usuarioDA.ObtenerAsync(usuarioId) ?? throw new RecursoNoEncontradoException("El usuario no existe.");

    public async Task<UsuarioResponse> CrearAsync(UsuarioCrearRequest solicitud)
    {
        solicitud.CorreoElectronico = solicitud.CorreoElectronico.Trim().ToLowerInvariant();
        var hash = _passwordReglas.GenerarHash(solicitud.Password);
        var id = await _usuarioDA.CrearAsync(solicitud, hash, solicitud.Rol, solicitud.Estado);
        return await ObtenerAsync(id);
    }

    public async Task<UsuarioResponse> ActualizarAsync(int actorUsuarioId, int usuarioId, UsuarioActualizarRequest solicitud)
    {
        var actual = await ObtenerAsync(usuarioId);
        _usuarioReglas.ValidarAutogestion(actorUsuarioId, usuarioId, actual.Rol, solicitud.Rol, solicitud.Estado);
        solicitud.CorreoElectronico = solicitud.CorreoElectronico.Trim().ToLowerInvariant();
        await _usuarioDA.ActualizarAsync(usuarioId, solicitud);
        return await ObtenerAsync(usuarioId);
    }

    public async Task CambiarEstadoAsync(int actorUsuarioId, int usuarioId, string estado)
    {
        var actual = await ObtenerAsync(usuarioId);
        _usuarioReglas.ValidarAutogestion(actorUsuarioId, usuarioId, actual.Rol, actual.Rol, estado);
        await _usuarioDA.CambiarEstadoAsync(usuarioId, estado);
    }

    public async Task CambiarPasswordAsync(int usuarioId, CambiarPasswordRequest solicitud)
    {
        await ObtenerAsync(usuarioId);
        await _usuarioDA.CambiarPasswordAsync(usuarioId, _passwordReglas.GenerarHash(solicitud.PasswordNueva));
    }

    public async Task<ResultadoEliminacion> EliminarAsync(int actorUsuarioId, int usuarioId)
    {
        var actual = await ObtenerAsync(usuarioId);
        _usuarioReglas.ValidarAutogestion(actorUsuarioId, usuarioId, actual.Rol, actual.Rol, EstadosUsuario.Inactivo);
        var resultado = await _usuarioDA.EliminarAsync(usuarioId);
        return new ResultadoEliminacion
        {
            Resultado = resultado,
            Mensaje = "La cuenta se desactivó (borrado lógico). Sus pedidos, pagos y entradas se conservan."
        };
    }

    private static string? Limpio(string? valor) => string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
}

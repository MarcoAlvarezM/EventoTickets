using EventoTickets.Abstracciones.Constantes;
using EventoTickets.Abstracciones.Excepciones;
using EventoTickets.Abstracciones.Interfaces.DA;
using EventoTickets.Abstracciones.Interfaces.Flujo;
using EventoTickets.Abstracciones.Interfaces.Reglas;
using EventoTickets.Abstracciones.Modelos;

namespace EventoTickets.Flujo;


public class AutenticacionFlujo : IAutenticacionFlujo
{
    private readonly IUsuarioDA _usuarioDA;
    private readonly IPasswordReglas _passwordReglas;
    private readonly IUsuarioReglas _usuarioReglas;

    public AutenticacionFlujo(IUsuarioDA usuarioDA, IPasswordReglas passwordReglas, IUsuarioReglas usuarioReglas)
    {
        _usuarioDA = usuarioDA;
        _passwordReglas = passwordReglas;
        _usuarioReglas = usuarioReglas;
    }

    public async Task<UsuarioResponse> AutenticarAsync(LoginRequest solicitud)
    {
        var credenciales = await _usuarioDA.ObtenerCredencialesAsync(solicitud.CorreoElectronico.Trim());
        // Mismo mensaje para correo inexistente o clave incorrecta (no revela qué cuentas existen)
        if (credenciales is null || !_passwordReglas.Verificar(solicitud.Password, credenciales.PasswordHash))
            throw new AutenticacionException("Correo electrónico o contraseña incorrectos.");

        _usuarioReglas.ValidarCuentaHabilitada(credenciales.Estado);
        return await _usuarioDA.ObtenerAsync(credenciales.UsuarioId)
               ?? throw new AutenticacionException("Correo electrónico o contraseña incorrectos.");
    }

    public async Task<UsuarioResponse> RegistrarCompradorAsync(RegistroCompradorRequest solicitud)
    {
        var hash = _passwordReglas.GenerarHash(solicitud.Password);
        solicitud.CorreoElectronico = solicitud.CorreoElectronico.Trim().ToLowerInvariant();
        var id = await _usuarioDA.CrearAsync(solicitud, hash, Roles.Comprador, EstadosUsuario.Activo);
        return await _usuarioDA.ObtenerAsync(id) ?? throw new RecursoNoEncontradoException("No se pudo recuperar el usuario creado.");
    }
}

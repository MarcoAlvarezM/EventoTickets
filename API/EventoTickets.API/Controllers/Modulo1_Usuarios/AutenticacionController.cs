using EventoTickets.Abstracciones.Interfaces.API;
using EventoTickets.Abstracciones.Interfaces.Servicios;
using EventoTickets.Abstracciones.Interfaces.Flujo;
using EventoTickets.Abstracciones.Modelos;
using EventoTickets.API.Seguridad;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventoTickets.API.Controllers;


/// <summary>Inicio de sesión, registro de compradores y perfil del usuario autenticado.</summary>
[ApiController]
[Route("api/autenticacion")]
[Produces("application/json")]
public class AutenticacionController : ControllerBase, IAutenticacionController
{
    private readonly IAutenticacionFlujo _flujo;
    private readonly IUsuarioFlujo _usuarioFlujo;
    private readonly IServicioTokenJwt _generadorToken;

    public AutenticacionController(IAutenticacionFlujo flujo, IUsuarioFlujo usuarioFlujo, IServicioTokenJwt generadorToken)
    {
        _flujo = flujo;
        _usuarioFlujo = usuarioFlujo;
        _generadorToken = generadorToken;
    }

    /// <summary>Valida credenciales y devuelve un token JWT.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(RespuestaApi<LoginResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespuestaApi<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest solicitud)
    {
        var usuario = await _flujo.AutenticarAsync(solicitud);
        return Ok(_generadorToken.Generar(usuario));
    }

    /// <summary>Registro público de un comprador; devuelve el token para iniciar sesión de inmediato.</summary>
    [HttpPost("registro")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(RespuestaApi<LoginResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(RespuestaApi<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(RespuestaApi<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Registro([FromBody] RegistroCompradorRequest solicitud)
    {
        var usuario = await _flujo.RegistrarCompradorAsync(solicitud);
        return StatusCode(StatusCodes.Status201Created, _generadorToken.Generar(usuario));
    }

    /// <summary>Datos del usuario dueño del token.</summary>
    [HttpGet("perfil")]
    [Authorize]
    [ProducesResponseType(typeof(RespuestaApi<UsuarioResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Perfil() => Ok(await _usuarioFlujo.ObtenerAsync(User.ObtenerUsuarioId()));
}

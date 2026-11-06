using EventoTickets.Abstracciones.Modelos;
using Microsoft.AspNetCore.Mvc;

namespace EventoTickets.Abstracciones.Interfaces.API;


public interface IAutenticacionController
{
    Task<IActionResult> Login(LoginRequest solicitud);
    Task<IActionResult> Registro(RegistroCompradorRequest solicitud);
    Task<IActionResult> Perfil();
}

public interface IUsuarioController
{
    Task<IActionResult> Listar(string? busqueda, string? rol, string? estado);
    Task<IActionResult> Obtener(int id);
    Task<IActionResult> Crear(UsuarioCrearRequest solicitud);
    Task<IActionResult> Actualizar(int id, UsuarioActualizarRequest solicitud);
    Task<IActionResult> CambiarEstado(int id, CambiarEstadoUsuarioRequest solicitud);
    Task<IActionResult> CambiarPassword(int id, CambiarPasswordRequest solicitud);
    Task<IActionResult> Eliminar(int id);
}

public interface ICategoriaController
{
    Task<IActionResult> Listar(bool soloActivas);
    Task<IActionResult> Obtener(int id);
    Task<IActionResult> Crear(CategoriaRequest solicitud);
    Task<IActionResult> Actualizar(int id, CategoriaRequest solicitud);
    Task<IActionResult> Eliminar(int id);
}

public interface ILocacionController
{
    Task<IActionResult> Listar(bool soloActivas, string? busqueda);
    Task<IActionResult> Obtener(int id);
    Task<IActionResult> Crear(LocacionRequest solicitud);
    Task<IActionResult> Actualizar(int id, LocacionRequest solicitud);
    Task<IActionResult> Eliminar(int id);
}

using System.Data;
using EventoTickets.Abstracciones.Modelos;
using Microsoft.AspNetCore.Http;

namespace EventoTickets.Abstracciones.Interfaces.DA;


public interface IUsuarioDA
{
    Task<IEnumerable<UsuarioResponse>> ListarAsync(string? busqueda, string? rol, string? estado);
    Task<UsuarioResponse?> ObtenerAsync(int usuarioId);
    Task<CredencialesUsuario?> ObtenerCredencialesAsync(string correoElectronico);
    Task<int> CrearAsync(UsuarioBase datos, string passwordHash, string rol, string estado);
    Task ActualizarAsync(int usuarioId, UsuarioActualizarRequest datos);
    Task CambiarEstadoAsync(int usuarioId, string estado);
    Task CambiarPasswordAsync(int usuarioId, string passwordHash);
    /// <summary>Borrado lógico (Estado = Inactivo); el historial se conserva.</summary>
    Task<string> EliminarAsync(int usuarioId);
}

public interface ICategoriaDA
{
    Task<IEnumerable<CategoriaResponse>> ListarAsync(bool soloActivas);
    Task<CategoriaResponse?> ObtenerAsync(int categoriaId);
    Task<int> CrearAsync(CategoriaRequest datos);
    Task ActualizarAsync(int categoriaId, CategoriaRequest datos);
    Task<string> EliminarAsync(int categoriaId);
}

public interface ILocacionDA
{
    Task<IEnumerable<LocacionResponse>> ListarAsync(bool soloActivas, string? busqueda);
    Task<LocacionResponse?> ObtenerAsync(int locacionId);
    Task<int> CrearAsync(LocacionRequest datos);
    Task ActualizarAsync(int locacionId, LocacionRequest datos);
    Task<string> EliminarAsync(int locacionId);
}

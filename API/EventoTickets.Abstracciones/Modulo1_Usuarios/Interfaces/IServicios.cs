using EventoTickets.Abstracciones.Modelos;

namespace EventoTickets.Abstracciones.Interfaces.Servicios;

/// <summary>Emite el JWT que la aplicación web adjunta como Bearer en cada llamada (HU-02).</summary>
public interface IServicioTokenJwt
{
    LoginResponse Generar(UsuarioResponse usuario);
}

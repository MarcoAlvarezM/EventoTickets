using EventoTickets.Abstracciones.Modelos;
using Microsoft.AspNetCore.Http;

namespace EventoTickets.Abstracciones.Interfaces.Reglas;


/// <summary>Hash y verificación de contraseñas (PBKDF2-SHA256). Nunca se guardan en texto plano.</summary>
public interface IPasswordReglas
{
    string GenerarHash(string password);
    bool Verificar(string password, string hashAlmacenado);
}

public interface IUsuarioReglas
{
    /// <summary>Impide que un usuario se suspenda o se quite a sí mismo el rol de superadministrador.</summary>
    void ValidarAutogestion(int actorUsuarioId, int usuarioObjetivoId, string rolActual, string rolNuevo, string estadoNuevo);
    void ValidarCuentaHabilitada(string estado);
}

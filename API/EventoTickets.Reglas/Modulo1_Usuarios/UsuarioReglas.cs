using EventoTickets.Abstracciones.Constantes;
using EventoTickets.Abstracciones.Excepciones;
using EventoTickets.Abstracciones.Interfaces.Reglas;

namespace EventoTickets.Reglas;


public class UsuarioReglas : IUsuarioReglas
{
    public void ValidarAutogestion(int actorUsuarioId, int usuarioObjetivoId, string rolActual, string rolNuevo, string estadoNuevo)
    {
        if (actorUsuarioId != usuarioObjetivoId) return;

        if (estadoNuevo != EstadosUsuario.Activo)
            throw new ReglaNegocioException("No puede bloquear ni desactivar su propia cuenta.");
        if (rolActual == Roles.Superadministrador && rolNuevo != Roles.Superadministrador)
            throw new ReglaNegocioException("No puede quitarse a sí mismo el rol de Superadministrador.");
    }

    public void ValidarCuentaHabilitada(string estado)
    {
        if (estado == EstadosUsuario.Bloqueado)
            throw new AutenticacionException("La cuenta está bloqueada. Contacte al administrador.");
        if (estado != EstadosUsuario.Activo)
            throw new AutenticacionException("La cuenta no está activa.");
    }
}

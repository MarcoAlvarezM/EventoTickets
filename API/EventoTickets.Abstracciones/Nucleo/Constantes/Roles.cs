namespace EventoTickets.Abstracciones.Constantes;


/// <summary>Roles del sistema (columna Usuarios.Rol). Basados en los perfiles RBAC del diseño Stitch.</summary>
public static class Roles
{
    public const string Superadministrador = "Superadministrador";
    public const string AdministradorEventos = "AdministradorEventos";
    public const string AuditorPagos = "AuditorPagos";
    public const string ValidadorAcceso = "ValidadorAcceso";
    public const string Comprador = "Comprador";

    // Combinaciones para [Authorize(Roles = ...)]
    public const string GestionEventos = Superadministrador + "," + AdministradorEventos;
    public const string Auditoria = Superadministrador + "," + AuditorPagos;
    public const string ControlAcceso = Superadministrador + "," + ValidadorAcceso;
    public const string Supervision = Superadministrador + "," + AdministradorEventos + "," + AuditorPagos;
    public const string Personal = Superadministrador + "," + AdministradorEventos + "," + AuditorPagos + "," + ValidadorAcceso;

    public static readonly string[] Todos = { Superadministrador, AdministradorEventos, AuditorPagos, ValidadorAcceso, Comprador };

    public static bool EsPersonal(string? rol) => rol is not null && rol != Comprador && Todos.Contains(rol);
}

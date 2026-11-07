namespace EventoTickets.Web.Infraestructura;

/// <summary>Nombres de los claims que guarda la cookie de sesión de la Web.</summary>
public static class ReclamosSesion
{
    public const string Token = "access_token";
}

/// <summary>Políticas de autorización de la Web (espejo de los roles del API).</summary>
public static class Politicas
{
    public const string Personal = nameof(Personal);
    public const string Superadministrador = nameof(Superadministrador);
    public const string GestionEventos = nameof(GestionEventos);
    public const string Supervision = nameof(Supervision);
    public const string ControlAcceso = nameof(ControlAcceso);
}

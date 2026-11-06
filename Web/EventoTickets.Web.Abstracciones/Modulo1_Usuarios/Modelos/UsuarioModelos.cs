using System.ComponentModel.DataAnnotations;
using EventoTickets.Web.Abstracciones.Constantes;
using EventoTickets.Web.Abstracciones.Validaciones;

namespace EventoTickets.Web.Abstracciones.Modelos;


public class UsuarioBase
{
    [Required(ErrorMessage = "El tipo de documento es requerido.")]
    [AllowedValues(TiposDocumento.Cedula, TiposDocumento.Dimex, TiposDocumento.Pasaporte, ErrorMessage = "Tipo de documento no válido.")]
    public string TipoDocumento { get; set; } = TiposDocumento.Cedula;

    [Required(ErrorMessage = "El número de documento es requerido.")]
    [StringLength(30, ErrorMessage = "El documento admite máximo {1} caracteres.")]
    [DocumentoIdentidad(nameof(TipoDocumento))]
    public string NumeroDocumento { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre completo es requerido.")]
    [StringLength(150, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre {2} y {1} caracteres.")]
    public string NombreCompleto { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo electrónico es requerido.")]
    [EmailAddress(ErrorMessage = "El correo electrónico no tiene un formato válido.")]
    [StringLength(150)]
    public string CorreoElectronico { get; set; } = string.Empty;

    [Phone(ErrorMessage = "El teléfono no tiene un formato válido.")]
    [StringLength(30)]
    public string? Telefono { get; set; }
}

/// <summary>Alta de usuario desde administración.</summary>
public class UsuarioCrearRequest : UsuarioBase
{
    [Required(ErrorMessage = "La contraseña es requerida.")]
    [PasswordSegura]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "El rol es requerido.")]
    [AllowedValues(Roles.Superadministrador, Roles.AdministradorEventos, Roles.AuditorPagos, Roles.ValidadorAcceso, Roles.Comprador, ErrorMessage = "Rol no válido.")]
    public string Rol { get; set; } = Roles.Comprador;

    [Required]
    [AllowedValues(EstadosUsuario.Activo, EstadosUsuario.Bloqueado, EstadosUsuario.Inactivo, ErrorMessage = "Estado no válido.")]
    public string Estado { get; set; } = EstadosUsuario.Activo;
}

public class UsuarioActualizarRequest : UsuarioBase
{
    [Required(ErrorMessage = "El rol es requerido.")]
    [AllowedValues(Roles.Superadministrador, Roles.AdministradorEventos, Roles.AuditorPagos, Roles.ValidadorAcceso, Roles.Comprador, ErrorMessage = "Rol no válido.")]
    public string Rol { get; set; } = Roles.Comprador;

    [Required]
    [AllowedValues(EstadosUsuario.Activo, EstadosUsuario.Bloqueado, EstadosUsuario.Inactivo, ErrorMessage = "Estado no válido.")]
    public string Estado { get; set; } = EstadosUsuario.Activo;
}

/// <summary>Registro público de compradores.</summary>
public class RegistroCompradorRequest : UsuarioBase
{
    [Required(ErrorMessage = "La contraseña es requerida.")]
    [PasswordSegura]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirme la contraseña.")]
    [Compare(nameof(Password), ErrorMessage = "Las contraseñas no coinciden.")]
    public string ConfirmarPassword { get; set; } = string.Empty;

    [Range(typeof(bool), "true", "true", ErrorMessage = "Debe aceptar los términos y condiciones.")]
    public bool AceptaTerminos { get; set; }
}

public class UsuarioResponse : UsuarioBase
{
    public int UsuarioId { get; set; }
    public string Rol { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public DateTime FechaRegistro { get; set; }
}

public class CambiarEstadoUsuarioRequest
{
    [Required]
    [AllowedValues(EstadosUsuario.Activo, EstadosUsuario.Bloqueado, EstadosUsuario.Inactivo, ErrorMessage = "Estado no válido.")]
    public string Estado { get; set; } = string.Empty;
}

public class CambiarPasswordRequest
{
    [Required(ErrorMessage = "La nueva contraseña es requerida.")]
    [PasswordSegura]
    public string PasswordNueva { get; set; } = string.Empty;
}

public class LoginRequest
{
    [Required(ErrorMessage = "El correo electrónico es requerido.")]
    [EmailAddress(ErrorMessage = "El correo electrónico no tiene un formato válido.")]
    public string CorreoElectronico { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es requerida.")]
    public string Password { get; set; } = string.Empty;
}

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiraUtc { get; set; }
    public int UsuarioId { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string CorreoElectronico { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
}

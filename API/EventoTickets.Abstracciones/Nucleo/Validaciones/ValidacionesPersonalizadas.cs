using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using EventoTickets.Abstracciones.Constantes;

namespace EventoTickets.Abstracciones.Validaciones;


/// <summary>La fecha no puede ser anterior al día actual.</summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class FechaNoPasadaAttribute : ValidationAttribute
{
    public FechaNoPasadaAttribute() : base("La fecha no puede ser anterior a hoy.") { }

    public override bool IsValid(object? value) =>
        value is not DateTime fecha || fecha.Date >= DateTime.Today;
}

/// <summary>Valida el formato del documento según el tipo indicado en otra propiedad.</summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class DocumentoIdentidadAttribute : ValidationAttribute
{
    private static readonly Regex Cedula = new(@"^\d-\d{4}-\d{4}$", RegexOptions.Compiled);
    private static readonly Regex Dimex = new(@"^\d{11,12}$", RegexOptions.Compiled);
    private static readonly Regex Pasaporte = new(@"^[A-Za-z0-9]{6,20}$", RegexOptions.Compiled);

    public DocumentoIdentidadAttribute(string propiedadTipo) => PropiedadTipo = propiedadTipo;

    public string PropiedadTipo { get; }

    protected override ValidationResult? IsValid(object? value, ValidationContext contexto)
    {
        if (value is not string numero || string.IsNullOrWhiteSpace(numero))
            return ValidationResult.Success; // [Required] se encarga del vacío

        var tipo = contexto.ObjectType.GetProperty(PropiedadTipo)?.GetValue(contexto.ObjectInstance) as string;
        var valido = tipo switch
        {
            TiposDocumento.Cedula => Cedula.IsMatch(numero),
            TiposDocumento.Dimex => Dimex.IsMatch(numero),
            TiposDocumento.Pasaporte => Pasaporte.IsMatch(numero),
            _ => true
        };
        return valido
            ? ValidationResult.Success
            : new ValidationResult(tipo switch
            {
                TiposDocumento.Cedula => "La cédula debe tener el formato 0-0000-0000.",
                TiposDocumento.Dimex => "El DIMEX debe tener 11 o 12 dígitos.",
                _ => "El pasaporte debe tener entre 6 y 20 caracteres alfanuméricos."
            }, new[] { contexto.MemberName ?? string.Empty });
    }
}

/// <summary>Contraseña robusta: mínimo 8 caracteres, una letra, un número y un símbolo.</summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class PasswordSeguraAttribute : ValidationAttribute
{
    public PasswordSeguraAttribute()
        : base("La contraseña debe tener al menos 8 caracteres e incluir una letra, un número y un símbolo.") { }

    public override bool IsValid(object? value)
    {
        if (value is not string clave || clave.Length == 0) return true;
        return clave.Length >= 8 && clave.Any(char.IsLetter) && clave.Any(char.IsDigit)
               && clave.Any(c => !char.IsLetterOrDigit(c));
    }
}

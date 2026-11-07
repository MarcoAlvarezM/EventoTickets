using System.ComponentModel.DataAnnotations;
using EventoTickets.Abstracciones.Constantes;

namespace EventoTickets.Abstracciones.Modelos;


public class EventoBase
{
    /// <summary>Opcional: si se omite, la base genera EVT-AAAA-00000 (HU-04).</summary>
    [StringLength(50)]
    [RegularExpression(@"^[A-Z0-9\-]+$", ErrorMessage = "Use solo mayúsculas, números y guiones (ej. EVT-2026-00001).")]
    public string? CodigoReferencia { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Seleccione una categoría.")]
    public int CategoriaId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Seleccione una locación.")]
    public int LocacionId { get; set; }

    [Required(ErrorMessage = "El nombre del evento es requerido.")]
    [StringLength(150, MinimumLength = 5, ErrorMessage = "El nombre debe tener entre {2} y {1} caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(300)]
    public string? DescripcionCorta { get; set; }

    [Required(ErrorMessage = "La descripción completa es requerida.")]
    [MinLength(20, ErrorMessage = "La descripción debe tener al menos {1} caracteres.")]
    public string DescripcionCompleta { get; set; } = string.Empty;

    [StringLength(150)]
    public string? Organizador { get; set; }

    [StringLength(500)]
    [Url(ErrorMessage = "La imagen debe ser una URL válida.")]
    public string? ImagenUrl { get; set; }

    [Required(ErrorMessage = "La fecha del evento es requerida.")]
    [DataType(DataType.Date)]
    public DateTime FechaEvento { get; set; }

    [Required(ErrorMessage = "La hora del evento es requerida.")]
    [DataType(DataType.Time)]
    public TimeSpan HoraEvento { get; set; }
}

public class EventoRequest : EventoBase { }

public class EventoResponse : EventoBase
{
    public int EventoId { get; set; }
    public string Estado { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
    public string CategoriaNombre { get; set; } = string.Empty;
    public string LocacionNombre { get; set; } = string.Empty;
    public string Ciudad { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
    public string? PabellonPuerta { get; set; }
    public int CapacidadMaxima { get; set; }
    public decimal? PrecioDesde { get; set; }
    public int EntradasDisponibles { get; set; }
    public int EntradasVendidas { get; set; }
    public int CapacidadAsignada { get; set; }
}

public class CambiarEstadoEventoRequest
{
    [Required]
    [AllowedValues(EstadosEvento.Borrador, EstadosEvento.Publicado, EstadosEvento.Cancelado, EstadosEvento.Finalizado, ErrorMessage = "Estado no válido.")]
    public string Estado { get; set; } = string.Empty;
}

public class FiltroEventos
{
    public string? Busqueda { get; set; }
    public int? CategoriaId { get; set; }
    public int? LocacionId { get; set; }
    public string? Estado { get; set; }
    public bool SoloVigentes { get; set; }
    /// <summary>Cartelera pública: solo Publicado o Agotado (HU-06).</summary>
    public bool SoloCartelera { get; set; }
    public string? Ciudad { get; set; }
    [DataType(DataType.Date)] public DateTime? FechaDesde { get; set; }
    [DataType(DataType.Date)] public DateTime? FechaHasta { get; set; }
}

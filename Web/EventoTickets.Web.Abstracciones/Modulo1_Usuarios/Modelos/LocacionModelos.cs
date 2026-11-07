using System.ComponentModel.DataAnnotations;

namespace EventoTickets.Web.Abstracciones.Modelos;


public class LocacionBase
{
    [Required(ErrorMessage = "El nombre es requerido.")]
    [StringLength(150, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre {2} y {1} caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "La ciudad es requerida.")]
    [StringLength(100)]
    public string Ciudad { get; set; } = string.Empty;

    [Required(ErrorMessage = "La dirección es requerida.")]
    [StringLength(255)]
    public string Direccion { get; set; } = string.Empty;

    [StringLength(100)]
    public string? PabellonPuerta { get; set; }

    [Required(ErrorMessage = "La capacidad máxima es requerida.")]
    [Range(1, 200000, ErrorMessage = "La capacidad debe estar entre {1} y {2} personas.")]
    public int CapacidadMaxima { get; set; }

    public bool Activo { get; set; } = true;
}

public class LocacionRequest : LocacionBase { }

public class LocacionResponse : LocacionBase
{
    public int LocacionId { get; set; }
    public int CantidadEventos { get; set; }
}

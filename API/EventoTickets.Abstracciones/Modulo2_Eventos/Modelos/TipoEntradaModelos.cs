using System.ComponentModel.DataAnnotations;
using EventoTickets.Abstracciones.Constantes;

namespace EventoTickets.Abstracciones.Modelos;


public class TipoEntradaBase
{
    [Required(ErrorMessage = "El nombre es requerido.")]
    [StringLength(80, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre {2} y {1} caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Range(typeof(decimal), "0", "99999999", ErrorMessage = "El precio debe ser mayor o igual a 0.")]
    public decimal Precio { get; set; }

    [RegularExpression("^#[0-9A-Fa-f]{6}$", ErrorMessage = "El color debe tener formato hexadecimal (#RRGGBB).")]
    public string? ColorHex { get; set; }

    [Range(1, 200000, ErrorMessage = "La cantidad inicial debe estar entre {1} y {2}.")]
    public int CantidadInicial { get; set; }

    public bool Activo { get; set; } = true;
}

public class TipoEntradaRequest : TipoEntradaBase
{
    [Range(1, int.MaxValue, ErrorMessage = "Seleccione el evento.")]
    public int EventoId { get; set; }
}

public class TipoEntradaResponse : TipoEntradaBase
{
    public int TipoEntradaId { get; set; }
    public int EventoId { get; set; }
    public string EventoNombre { get; set; } = string.Empty;
    public int CantidadComprometida { get; set; }
    public int CantidadVendida { get; set; }
    public int Disponible { get; set; }
    public decimal? PorcentajeVendido { get; set; }
}

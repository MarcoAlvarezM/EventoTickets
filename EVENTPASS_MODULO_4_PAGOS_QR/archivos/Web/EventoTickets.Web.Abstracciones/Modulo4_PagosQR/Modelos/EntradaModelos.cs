using System.ComponentModel.DataAnnotations;

namespace EventoTickets.Web.Abstracciones.Modelos;


public class EntradaResponse
{
    public int EntradaId { get; set; }
    public string CodigoTicket { get; set; } = string.Empty;
    public int PedidoId { get; set; }
    public int EventoId { get; set; }
    public int TipoEntradaId { get; set; }
    public string NombreAsistente { get; set; } = string.Empty;
    public string DocumentoAsistente { get; set; } = string.Empty;
    public string? AsientoAsignado { get; set; }
    public string? PuertaAcceso { get; set; }
    public string HashCriptografico { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public DateTime FechaEmision { get; set; }
    public DateTime? FechaHoraIngreso { get; set; }
    public int? ValidadorPuertaUsuarioId { get; set; }
    public string CodigoPedido { get; set; } = string.Empty;
    public int UsuarioId { get; set; }
    public string EventoNombre { get; set; } = string.Empty;
    public DateTime FechaEvento { get; set; }
    public TimeSpan HoraEvento { get; set; }
    public string? EventoImagenUrl { get; set; }
    public string EventoEstado { get; set; } = string.Empty;
    public string LocacionNombre { get; set; } = string.Empty;
    public string Ciudad { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
    public string TipoEntradaNombre { get; set; } = string.Empty;
    public string? ColorHex { get; set; }
    public string? ValidadorNombre { get; set; }
}

public class TransferenciaEntradaRequest
{
    [Required(ErrorMessage = "El nombre del nuevo titular es requerido.")]
    [StringLength(150, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre {2} y {1} caracteres.")]
    public string NombreAsistente { get; set; } = string.Empty;

    [Required(ErrorMessage = "El documento del nuevo titular es requerido.")]
    [StringLength(40, MinimumLength = 6, ErrorMessage = "El documento debe tener entre {2} y {1} caracteres.")]
    public string DocumentoAsistente { get; set; } = string.Empty;
}

public class ValidacionAccesoRequest
{
    [Required(ErrorMessage = "Ingrese o escanee el código de la entrada.")]
    [StringLength(50)]
    public string CodigoTicket { get; set; } = string.Empty;

    [StringLength(64)]
    public string? HashCriptografico { get; set; }

    [StringLength(50)]
    public string? PuertaAcceso { get; set; }
}

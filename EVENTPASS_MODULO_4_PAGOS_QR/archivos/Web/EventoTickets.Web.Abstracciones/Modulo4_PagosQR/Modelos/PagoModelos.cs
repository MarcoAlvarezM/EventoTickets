using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace EventoTickets.Web.Abstracciones.Modelos;


/// <summary>Reporte de pago manual (multipart/form-data con el comprobante adjunto).</summary>
public class PagoRegistroRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "Pedido no válido.")]
    public int PedidoId { get; set; }

    [Required(ErrorMessage = "Indique la entidad bancaria de origen.")]
    [StringLength(80)]
    public string BancoOrigen { get; set; } = string.Empty;

    [Required(ErrorMessage = "Indique el número de referencia o comprobante.")]
    [StringLength(100, MinimumLength = 4, ErrorMessage = "La referencia debe tener entre {2} y {1} caracteres.")]
    public string ReferenciaComprobante { get; set; } = string.Empty;

    [Range(typeof(decimal), "1", "99999999", ErrorMessage = "El monto debe ser mayor a 0.")]
    public decimal MontoReportado { get; set; }

    [StringLength(255)]
    public string? NotaComprador { get; set; }

    [Required(ErrorMessage = "Adjunte el comprobante (JPG, PNG o PDF).")]
    public IFormFile? Comprobante { get; set; }
}

public class PagoAprobarRequest
{
    [StringLength(50)]
    public string? CodigoAprobacion { get; set; }
}

public class PagoRechazarRequest
{
    [Required(ErrorMessage = "Indique el motivo del rechazo.")]
    [StringLength(255, MinimumLength = 10, ErrorMessage = "El motivo debe tener entre {2} y {1} caracteres.")]
    public string MotivoRechazo { get; set; } = string.Empty;
}

public class PagoResponse
{
    public int PagoId { get; set; }
    public string CodigoPago { get; set; } = string.Empty;
    public int PedidoId { get; set; }
    public int? AuditorUsuarioId { get; set; }
    public string BancoOrigen { get; set; } = string.Empty;
    public string ReferenciaComprobante { get; set; } = string.Empty;
    public string? CodigoAprobacion { get; set; }
    public decimal MontoReportado { get; set; }
    public string? ComprobanteUrl { get; set; }
    public string HashComprobante { get; set; } = string.Empty;
    public string? NotaComprador { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string? MotivoRechazo { get; set; }
    public DateTime FechaReporte { get; set; }
    public DateTime? FechaValidacion { get; set; }
    public string CodigoPedido { get; set; } = string.Empty;
    public decimal PedidoImporteTotal { get; set; }
    public string PedidoEstado { get; set; } = string.Empty;
    public string ReferenciaBancariaUnica { get; set; } = string.Empty;
    public string MetodoPagoSeleccionado { get; set; } = string.Empty;
    public int CompradorUsuarioId { get; set; }
    public string CompradorNombre { get; set; } = string.Empty;
    public string CompradorCorreo { get; set; } = string.Empty;
    public string CompradorDocumento { get; set; } = string.Empty;
    public string? CompradorTelefono { get; set; }
    public int EventoId { get; set; }
    public string EventoNombre { get; set; } = string.Empty;
    public DateTime FechaEvento { get; set; }
    public string? AuditorNombre { get; set; }
    public bool MontoCoincide { get; set; }
}

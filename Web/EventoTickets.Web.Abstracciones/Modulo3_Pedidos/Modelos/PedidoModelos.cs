using System.ComponentModel.DataAnnotations;
using EventoTickets.Web.Abstracciones.Constantes;

namespace EventoTickets.Web.Abstracciones.Modelos;


public class PedidoLineaRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "Tipo de entrada no válido.")]
    public int TipoEntradaId { get; set; }

    [Range(0, 50, ErrorMessage = "La cantidad debe estar entre {1} y {2}.")]
    public int Cantidad { get; set; }
}

public class PedidoRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "Evento no válido.")]
    public int EventoId { get; set; }

    [Required(ErrorMessage = "Seleccione un método de pago.")]
    [AllowedValues(MetodosPago.SinpeMovil, MetodosPago.Transferencia, ErrorMessage = "Método de pago no válido.")]
    public string MetodoPago { get; set; } = MetodosPago.SinpeMovil;

    [Required]
    [MinLength(1, ErrorMessage = "Debe seleccionar al menos una entrada.")]
    public List<PedidoLineaRequest> Lineas { get; set; } = new();
}

/// <summary>UPDATE del CRUD de pedidos: cambio de método de pago mientras está PendientePago.</summary>
public class ActualizarMetodoPagoRequest
{
    [Required(ErrorMessage = "Seleccione un método de pago.")]
    [AllowedValues(MetodosPago.SinpeMovil, MetodosPago.Transferencia, ErrorMessage = "Método de pago no válido.")]
    public string MetodoPago { get; set; } = MetodosPago.SinpeMovil;
}

public class CotizacionLinea
{
    public int TipoEntradaId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? ColorHex { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal SubtotalLinea { get; set; }
}

/// <summary>Desglose calculado por la capa Reglas antes de confirmar el pedido.</summary>
public class CotizacionResponse
{
    public int EventoId { get; set; }
    public List<CotizacionLinea> Lineas { get; set; } = new();
    public int CantidadEntradas { get; set; }
    public decimal Subtotal { get; set; }
    public decimal PorcentajeCargoServicio { get; set; }
    public decimal CargoServicio { get; set; }
    public decimal PorcentajeIVA { get; set; }
    public decimal ImpuestoIVA { get; set; }
    public decimal ImporteTotal { get; set; }
    public int MinutosReserva { get; set; }
    public string Moneda { get; set; } = "CRC";
}

public class PedidoResponse
{
    public int PedidoId { get; set; }
    public string CodigoPedido { get; set; } = string.Empty;
    public int UsuarioId { get; set; }
    public int EventoId { get; set; }
    public string MetodoPagoSeleccionado { get; set; } = string.Empty;
    public string ReferenciaBancariaUnica { get; set; } = string.Empty;
    public decimal Subtotal { get; set; }
    public decimal CargoServicio { get; set; }
    public decimal ImpuestoIVA { get; set; }
    public decimal ImporteTotal { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime FechaVencimientoReserva { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string CompradorNombre { get; set; } = string.Empty;
    public string CompradorCorreo { get; set; } = string.Empty;
    public string CompradorTipoDocumento { get; set; } = string.Empty;
    public string CompradorDocumento { get; set; } = string.Empty;
    public string? CompradorTelefono { get; set; }
    public string EventoNombre { get; set; } = string.Empty;
    public DateTime FechaEvento { get; set; }
    public TimeSpan HoraEvento { get; set; }
    public string? EventoImagenUrl { get; set; }
    public string LocacionNombre { get; set; } = string.Empty;
    public string Ciudad { get; set; } = string.Empty;
    public int CantidadEntradas { get; set; }
    public string? EstadoUltimoPago { get; set; }
}

public class PedidoDetalleResponse
{
    public int PedidoDetalleId { get; set; }
    public int PedidoId { get; set; }
    public int TipoEntradaId { get; set; }
    public string TipoEntradaNombre { get; set; } = string.Empty;
    public string? ColorHex { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal SubtotalLinea { get; set; }
}

public class PedidoCompletoResponse : PedidoResponse
{
    public List<PedidoDetalleResponse> Detalles { get; set; } = new();
}

public class FiltroPedidos
{
    public string? Estado { get; set; }
    public int? EventoId { get; set; }
    public string? Busqueda { get; set; }
}

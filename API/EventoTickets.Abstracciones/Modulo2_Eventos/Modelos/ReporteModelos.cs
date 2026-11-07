namespace EventoTickets.Abstracciones.Modelos;


public class IndicadoresDashboard
{
    public int EventosPublicados { get; set; }
    public int EventosBorrador { get; set; }
    public int EventosFinalizados { get; set; }
    public int UsuariosRegistrados { get; set; }
    public int UsuariosNuevosHoy { get; set; }
    public int PedidosTotales { get; set; }
    public int PedidosEnProceso { get; set; }
    public int PagosPorValidar { get; set; }
    public int EntradasEmitidas { get; set; }
    public decimal IngresosTotales { get; set; }
}

public class VentaEventoResponse
{
    public int EventoId { get; set; }
    public string CodigoReferencia { get; set; } = string.Empty;
    public string EventoNombre { get; set; } = string.Empty;
    public DateTime FechaEvento { get; set; }
    public string Estado { get; set; } = string.Empty;
    public int CategoriaId { get; set; }
    public string CategoriaNombre { get; set; } = string.Empty;
    public string LocacionNombre { get; set; } = string.Empty;
    public int CapacidadAsignada { get; set; }
    public int EntradasVendidas { get; set; }
    public int EntradasComprometidas { get; set; }
    public int EntradasDisponibles { get; set; }
    public int PedidosPagados { get; set; }
    public decimal IngresosTotales { get; set; }
    public decimal? PorcentajeOcupacion { get; set; }
}

/// <summary>Fila del panel: pago pendiente (DTO propio para no depender del módulo 4).</summary>
public class DashboardPago
{
    public int PagoId { get; set; }
    public string CodigoPago { get; set; } = string.Empty;
    public string CodigoPedido { get; set; } = string.Empty;
    public string CompradorNombre { get; set; } = string.Empty;
    public string CompradorCorreo { get; set; } = string.Empty;
    public decimal MontoReportado { get; set; }
    public string ReferenciaComprobante { get; set; } = string.Empty;
    public DateTime FechaReporte { get; set; }
}

/// <summary>Fila del panel: pedido reciente (DTO propio para no depender del módulo 3).</summary>
public class DashboardPedido
{
    public int PedidoId { get; set; }
    public string CodigoPedido { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
    public string CompradorNombre { get; set; } = string.Empty;
    public string EventoNombre { get; set; } = string.Empty;
    public int CantidadEntradas { get; set; }
    public decimal ImporteTotal { get; set; }
    public string Estado { get; set; } = string.Empty;
}

public class DashboardResponse
{
    public IndicadoresDashboard Indicadores { get; set; } = new();
    public List<DashboardPago> PagosPendientes { get; set; } = new();
    public List<VentaEventoResponse> ProximosEventos { get; set; } = new();
    public List<DashboardPedido> PedidosRecientes { get; set; } = new();
}

public class FiltroReporteVentas
{
    public DateTime? FechaDesde { get; set; }
    public DateTime? FechaHasta { get; set; }
    public int? CategoriaId { get; set; }
}

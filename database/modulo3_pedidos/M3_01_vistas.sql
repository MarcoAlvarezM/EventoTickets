/* =============================================================================
   M3 · Vistas
   Módulo: Módulo 3 · Pedidos, historial, cancelación y vencimiento de reservas (HU-12, 13, 14)
   Objetos: vw_PedidosResumen
   Depende de: base/01..06, modulo2 (vistas de eventos)
   ============================================================================= */
USE EventCR_BoleteriaPro_DB;
GO

/* Pedidos con comprador, evento y último estado de pago */
CREATE OR ALTER VIEW dbo.vw_PedidosResumen
AS
SELECT
    p.PedidoId, p.CodigoPedido, p.UsuarioId, p.EventoId, p.MetodoPagoSeleccionado, p.ReferenciaBancariaUnica,
    p.Subtotal, p.CargoServicio, p.ImpuestoIVA, p.ImporteTotal, p.FechaCreacion, p.FechaVencimientoReserva, p.Estado,
    u.NombreCompleto AS CompradorNombre, u.CorreoElectronico AS CompradorCorreo,
    u.TipoDocumento AS CompradorTipoDocumento, u.NumeroDocumento AS CompradorDocumento, u.Telefono AS CompradorTelefono,
    e.Nombre AS EventoNombre, e.FechaEvento, e.HoraEvento, e.ImagenUrl AS EventoImagenUrl,
    l.Nombre AS LocacionNombre, l.Ciudad,
    CantidadEntradas = ISNULL((SELECT SUM(d.Cantidad) FROM dbo.PedidoDetalles d WHERE d.PedidoId = p.PedidoId), 0),
    EstadoUltimoPago = (SELECT TOP (1) pg.Estado FROM dbo.Pagos pg WHERE pg.PedidoId = p.PedidoId ORDER BY pg.FechaReporte DESC, pg.PagoId DESC)
FROM dbo.Pedidos p
INNER JOIN dbo.Usuarios   u ON u.UsuarioId = p.UsuarioId
INNER JOIN dbo.Eventos    e ON e.EventoId  = p.EventoId
INNER JOIN dbo.Locaciones l ON l.LocacionId = e.LocacionId;
GO

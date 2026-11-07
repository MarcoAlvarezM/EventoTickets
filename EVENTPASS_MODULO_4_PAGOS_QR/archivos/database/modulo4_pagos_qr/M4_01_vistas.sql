/* =============================================================================
   M4 · Vistas
   Módulo: Módulo 4 · Pagos, auditoría, entradas QR y control de acceso (HU-15 a HU-19)
   Objetos: vw_PagosDetalle, vw_EntradasDetalle
   Depende de: base/01..06, modulo3 (vw_PedidosResumen, sp_Pedido_LiberarVencidos)
   ============================================================================= */
USE EventCR_BoleteriaPro_DB;
GO

/* Pagos con pedido, comprador, evento y auditor */
CREATE OR ALTER VIEW dbo.vw_PagosDetalle
AS
SELECT
    pg.PagoId, pg.CodigoPago, pg.PedidoId, pg.AuditorUsuarioId, pg.BancoOrigen, pg.ReferenciaComprobante,
    pg.CodigoAprobacion, pg.MontoReportado, pg.ComprobanteUrl, pg.HashComprobante, pg.NotaComprador,
    pg.Estado, pg.MotivoRechazo, pg.FechaReporte, pg.FechaValidacion,
    p.CodigoPedido, p.ImporteTotal AS PedidoImporteTotal, p.Estado AS PedidoEstado, p.ReferenciaBancariaUnica,
    p.MetodoPagoSeleccionado, p.UsuarioId AS CompradorUsuarioId,
    u.NombreCompleto AS CompradorNombre, u.CorreoElectronico AS CompradorCorreo, u.NumeroDocumento AS CompradorDocumento,
    u.Telefono AS CompradorTelefono,
    e.EventoId, e.Nombre AS EventoNombre, e.FechaEvento,
    a.NombreCompleto AS AuditorNombre,
    MontoCoincide = CAST(CASE WHEN pg.MontoReportado = p.ImporteTotal THEN 1 ELSE 0 END AS BIT)
FROM dbo.Pagos pg
INNER JOIN dbo.Pedidos  p ON p.PedidoId  = pg.PedidoId
INNER JOIN dbo.Usuarios u ON u.UsuarioId = p.UsuarioId
INNER JOIN dbo.Eventos  e ON e.EventoId  = p.EventoId
LEFT  JOIN dbo.Usuarios a ON a.UsuarioId = pg.AuditorUsuarioId;
GO

/* Entradas emitidas con evento, tipo, locación y validador */
CREATE OR ALTER VIEW dbo.vw_EntradasDetalle
AS
SELECT
    en.EntradaId, en.CodigoTicket, en.PedidoId, en.EventoId, en.TipoEntradaId, en.NombreAsistente,
    en.DocumentoAsistente, en.AsientoAsignado, en.PuertaAcceso, en.HashCriptografico, en.Estado,
    en.FechaEmision, en.FechaHoraIngreso, en.ValidadorPuertaUsuarioId,
    p.CodigoPedido, p.UsuarioId,
    e.Nombre AS EventoNombre, e.FechaEvento, e.HoraEvento, e.ImagenUrl AS EventoImagenUrl, e.Estado AS EventoEstado,
    l.Nombre AS LocacionNombre, l.Ciudad, l.Direccion,
    t.Nombre AS TipoEntradaNombre, t.ColorHex,
    v.NombreCompleto AS ValidadorNombre
FROM dbo.Entradas en
INNER JOIN dbo.Pedidos      p ON p.PedidoId = en.PedidoId
INNER JOIN dbo.Eventos      e ON e.EventoId = en.EventoId
INNER JOIN dbo.Locaciones   l ON l.LocacionId = e.LocacionId
INNER JOIN dbo.TiposEntrada t ON t.TipoEntradaId = en.TipoEntradaId
LEFT  JOIN dbo.Usuarios     v ON v.UsuarioId = en.ValidadorPuertaUsuarioId;
GO

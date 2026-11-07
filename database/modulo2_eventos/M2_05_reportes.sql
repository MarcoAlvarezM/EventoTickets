/* =============================================================================
   M2 · Reportes (panel y ventas por evento)
   Módulo: Módulo 2 · Eventos, cartelera, tipos de entrada y disponibilidad (HU-04, 05, 06, 09, 10, 11)
   Se ejecuta después de los módulos 3 y 4 porque el panel lee vw_PedidosResumen y vw_PagosDetalle.
   Objetos: vw_VentasPorEvento, sp_Reporte_Dashboard, sp_Reporte_VentasPorEvento
   Depende de: modulo2, modulo3, modulo4
   ============================================================================= */
USE EventCR_BoleteriaPro_DB;
GO

/* Ventas consolidadas por evento (reportes y panel) */
CREATE OR ALTER VIEW dbo.vw_VentasPorEvento
AS
SELECT
    e.EventoId, e.CodigoReferencia, e.Nombre AS EventoNombre, e.FechaEvento, e.Estado,
    e.CategoriaId, c.Nombre AS CategoriaNombre, l.Nombre AS LocacionNombre,
    CapacidadAsignada     = ISNULL(t.CapacidadAsignada, 0),
    EntradasVendidas      = ISNULL(t.Vendidas, 0),
    EntradasComprometidas = ISNULL(t.Comprometidas, 0),
    EntradasDisponibles   = ISNULL(t.CapacidadAsignada, 0) - ISNULL(t.Vendidas, 0) - ISNULL(t.Comprometidas, 0),
    PedidosPagados        = ISNULL(v.PedidosPagados, 0),
    IngresosTotales       = ISNULL(v.Ingresos, 0),
    PorcentajeOcupacion   = CAST(ROUND(100.0 * ISNULL(t.Vendidas, 0) / NULLIF(t.CapacidadAsignada, 0), 1) AS DECIMAL(5,1))
FROM dbo.Eventos e
INNER JOIN dbo.Categorias c ON c.CategoriaId = e.CategoriaId
INNER JOIN dbo.Locaciones l ON l.LocacionId = e.LocacionId
LEFT JOIN (SELECT EventoId, SUM(CantidadInicial) AS CapacidadAsignada, SUM(CantidadVendida) AS Vendidas,
                  SUM(CantidadComprometida) AS Comprometidas
           FROM dbo.TiposEntrada GROUP BY EventoId) t ON t.EventoId = e.EventoId
LEFT JOIN (SELECT EventoId, COUNT(*) AS PedidosPagados, SUM(ImporteTotal) AS Ingresos
           FROM dbo.Pedidos WHERE Estado = 'Pagado' GROUP BY EventoId) v ON v.EventoId = e.EventoId;
GO

/* Panel principal: 4 conjuntos de resultados */
CREATE OR ALTER PROCEDURE dbo.sp_Reporte_Dashboard
AS
BEGIN
    SET NOCOUNT ON;
    -- 1. Indicadores
    SELECT
        EventosPublicados  = (SELECT COUNT(*) FROM dbo.Eventos WHERE Estado IN ('Publicado', 'Agotado')),
        EventosBorrador    = (SELECT COUNT(*) FROM dbo.Eventos WHERE Estado = 'Borrador'),
        EventosFinalizados = (SELECT COUNT(*) FROM dbo.Eventos WHERE Estado = 'Finalizado'),
        UsuariosRegistrados = (SELECT COUNT(*) FROM dbo.Usuarios),
        UsuariosNuevosHoy  = (SELECT COUNT(*) FROM dbo.Usuarios WHERE CAST(FechaRegistro AS DATE) = CAST(GETDATE() AS DATE)),
        PedidosTotales     = (SELECT COUNT(*) FROM dbo.Pedidos),
        PedidosEnProceso   = (SELECT COUNT(*) FROM dbo.Pedidos WHERE Estado IN ('PendientePago', 'EnValidacion')),
        PagosPorValidar    = (SELECT COUNT(*) FROM dbo.Pagos WHERE Estado = 'PendienteValidacion'),
        EntradasEmitidas   = (SELECT COUNT(*) FROM dbo.Entradas WHERE Estado IN ('Valida', 'Utilizada')),
        IngresosTotales    = ISNULL((SELECT SUM(ImporteTotal) FROM dbo.Pedidos WHERE Estado = 'Pagado'), 0);

    -- 2. Pagos pendientes más antiguos
    SELECT TOP (5) * FROM dbo.vw_PagosDetalle WHERE Estado = 'PendienteValidacion' ORDER BY FechaReporte;

    -- 3. Próximos eventos publicados con ocupación
    SELECT TOP (4) * FROM dbo.vw_VentasPorEvento
    WHERE Estado IN ('Publicado', 'Agotado') AND FechaEvento >= CAST(GETDATE() AS DATE)
    ORDER BY FechaEvento;

    -- 4. Pedidos recientes
    SELECT TOP (5) * FROM dbo.vw_PedidosResumen ORDER BY FechaCreacion DESC;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Reporte_VentasPorEvento
    @FechaDesde  DATE = NULL,
    @FechaHasta  DATE = NULL,
    @CategoriaId INT  = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM dbo.vw_VentasPorEvento
    WHERE (@FechaDesde IS NULL OR FechaEvento >= @FechaDesde)
      AND (@FechaHasta IS NULL OR FechaEvento <= @FechaHasta)
      AND (@CategoriaId IS NULL OR CategoriaId = @CategoriaId)
    ORDER BY FechaEvento;
END
GO

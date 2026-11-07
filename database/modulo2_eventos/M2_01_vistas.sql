/* =============================================================================
   M2 · Vistas
   Módulo: Módulo 2 · Eventos, cartelera, tipos de entrada y disponibilidad (HU-04, 05, 06, 09, 10, 11)
   Objetos: vw_EventosCatalogo, vw_TiposEntradaDisponibilidad
   Depende de: base/01..06
   ============================================================================= */
USE EventCR_BoleteriaPro_DB;
GO

/* Catálogo de eventos con categoría, locación, precio mínimo y disponibilidad */
CREATE OR ALTER VIEW dbo.vw_EventosCatalogo
AS
SELECT
    e.EventoId, e.CodigoReferencia, e.Nombre, e.DescripcionCorta, e.DescripcionCompleta,
    e.Organizador, e.ImagenUrl, e.FechaEvento, e.HoraEvento, e.Estado, e.FechaCreacion,
    e.CategoriaId, c.Nombre AS CategoriaNombre,
    e.LocacionId, l.Nombre AS LocacionNombre, l.Ciudad, l.Direccion, l.PabellonPuerta, l.CapacidadMaxima,
    PrecioDesde          = (SELECT MIN(t.Precio) FROM dbo.TiposEntrada t WHERE t.EventoId = e.EventoId AND t.Activo = 1),
    EntradasDisponibles  = ISNULL((SELECT SUM(t.CantidadInicial - t.CantidadComprometida - t.CantidadVendida)
                                   FROM dbo.TiposEntrada t WHERE t.EventoId = e.EventoId AND t.Activo = 1), 0),
    EntradasVendidas     = ISNULL((SELECT SUM(t.CantidadVendida) FROM dbo.TiposEntrada t WHERE t.EventoId = e.EventoId), 0),
    CapacidadAsignada    = dbo.fn_CapacidadAsignadaEvento(e.EventoId, NULL)
FROM dbo.Eventos e
INNER JOIN dbo.Categorias c ON c.CategoriaId = e.CategoriaId
INNER JOIN dbo.Locaciones l ON l.LocacionId = e.LocacionId;
GO

/* Tipos de entrada con disponibilidad calculada */
CREATE OR ALTER VIEW dbo.vw_TiposEntradaDisponibilidad
AS
SELECT
    t.TipoEntradaId, t.EventoId, e.Nombre AS EventoNombre, t.Nombre, t.Precio, t.ColorHex,
    t.CantidadInicial, t.CantidadComprometida, t.CantidadVendida, t.Activo,
    Disponible        = t.CantidadInicial - t.CantidadComprometida - t.CantidadVendida,
    PorcentajeVendido = CAST(ROUND(100.0 * t.CantidadVendida / NULLIF(t.CantidadInicial, 0), 1) AS DECIMAL(5,1))
FROM dbo.TiposEntrada t
INNER JOIN dbo.Eventos e ON e.EventoId = t.EventoId;
GO

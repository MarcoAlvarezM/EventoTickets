/* =============================================================================
   Estructura general de la base de datos · Responsable: Módulo 2 (Donnkan Cervantes Peraza)
   06. Funciones escalares
   Se crean antes que las vistas porque éstas las utilizan.
   ============================================================================= */
USE EventCR_BoleteriaPro_DB;
GO

/* Entradas disponibles de un tipo: inicial - comprometidas (reservas vigentes) - vendidas */
CREATE OR ALTER FUNCTION dbo.fn_DisponibilidadTipoEntrada (@TipoEntradaId INT)
RETURNS INT
AS
BEGIN
    DECLARE @Disponible INT;
    SELECT @Disponible = CantidadInicial - CantidadComprometida - CantidadVendida
    FROM dbo.TiposEntrada
    WHERE TipoEntradaId = @TipoEntradaId;
    RETURN ISNULL(@Disponible, 0);
END
GO

/* Capacidad ya repartida entre los tipos de entrada de un evento.
   @ExcluirTipoEntradaId permite validar una edición sin contar el propio tipo. */
CREATE OR ALTER FUNCTION dbo.fn_CapacidadAsignadaEvento (@EventoId INT, @ExcluirTipoEntradaId INT = NULL)
RETURNS INT
AS
BEGIN
    DECLARE @Total INT;
    SELECT @Total = SUM(CantidadInicial)
    FROM dbo.TiposEntrada
    WHERE EventoId = @EventoId
      AND (@ExcluirTipoEntradaId IS NULL OR TipoEntradaId <> @ExcluirTipoEntradaId);
    RETURN ISNULL(@Total, 0);
END
GO

/* Entradas disponibles de todo el evento (tipos activos). Base del estado Agotado. */
CREATE OR ALTER FUNCTION dbo.fn_DisponibilidadEvento (@EventoId INT)
RETURNS INT
AS
BEGIN
    DECLARE @Total INT;
    SELECT @Total = SUM(CantidadInicial - CantidadComprometida - CantidadVendida)
    FROM dbo.TiposEntrada
    WHERE EventoId = @EventoId AND Activo = 1;
    RETURN ISNULL(@Total, 0);
END
GO

PRINT N'Funciones creadas.';
GO

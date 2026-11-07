/* =============================================================================
   M2 · Procedimientos
   Módulo: Módulo 2 · Eventos, cartelera, tipos de entrada y disponibilidad (HU-04, 05, 06, 09, 10, 11)
   Objetos:
     sp_Evento_Listar
     sp_Evento_ObtenerPorId
     sp_Evento_Crear
     sp_Evento_Actualizar
     sp_Evento_CambiarEstado
     sp_Evento_Eliminar
     sp_TipoEntrada_ListarPorEvento
     sp_TipoEntrada_ObtenerPorId
     sp_TipoEntrada_Crear
     sp_TipoEntrada_Actualizar
     sp_TipoEntrada_Eliminar
   Depende de: base/01..06
   ============================================================================= */
USE EventCR_BoleteriaPro_DB;
GO

CREATE OR ALTER PROCEDURE dbo.sp_Evento_Listar
    @Busqueda     VARCHAR(150) = NULL,
    @CategoriaId  INT = NULL,
    @LocacionId   INT = NULL,
    @Estado       VARCHAR(30) = NULL,
    @SoloVigentes BIT = 0,
    @SoloCartelera BIT = 0,          -- 1 = solo Publicado o Agotado (HU-06)
    @Ciudad       VARCHAR(100) = NULL,
    @FechaDesde   DATE = NULL,
    @FechaHasta   DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT EventoId, CodigoReferencia, Nombre, DescripcionCorta, Organizador, ImagenUrl, FechaEvento, HoraEvento,
           Estado, FechaCreacion, CategoriaId, CategoriaNombre, LocacionId, LocacionNombre, Ciudad, Direccion,
           PabellonPuerta, CapacidadMaxima, PrecioDesde, EntradasDisponibles, EntradasVendidas, CapacidadAsignada
    FROM dbo.vw_EventosCatalogo
    WHERE (@Busqueda IS NULL OR Nombre LIKE '%' + @Busqueda + '%' OR Organizador LIKE '%' + @Busqueda + '%'
                             OR CodigoReferencia LIKE '%' + @Busqueda + '%')
      AND (@CategoriaId IS NULL OR CategoriaId = @CategoriaId)
      AND (@LocacionId IS NULL OR LocacionId = @LocacionId)
      AND (@Estado IS NULL OR Estado = @Estado)
      AND (@SoloVigentes = 0 OR FechaEvento >= CAST(GETDATE() AS DATE))
      AND (@SoloCartelera = 0 OR Estado IN ('Publicado', 'Agotado'))
      AND (@Ciudad IS NULL OR Ciudad = @Ciudad)
      AND (@FechaDesde IS NULL OR FechaEvento >= @FechaDesde)
      AND (@FechaHasta IS NULL OR FechaEvento <= @FechaHasta)
    ORDER BY FechaEvento, HoraEvento;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Evento_ObtenerPorId
    @EventoId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM dbo.vw_EventosCatalogo WHERE EventoId = @EventoId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Evento_Crear
    @CodigoReferencia    VARCHAR(50) = NULL,   -- NULL = se genera EVT-AAAA-00000 (HU-04)
    @CategoriaId         INT,
    @LocacionId          INT,
    @Nombre              VARCHAR(150),
    @DescripcionCorta    VARCHAR(300) = NULL,
    @DescripcionCompleta VARCHAR(MAX),
    @Organizador         VARCHAR(150) = NULL,
    @ImagenUrl           VARCHAR(500) = NULL,
    @FechaEvento         DATE,
    @HoraEvento          TIME(0),
    @Estado              VARCHAR(30) = 'Borrador'
AS
BEGIN
    SET NOCOUNT ON;
    IF @CodigoReferencia IS NOT NULL AND EXISTS (SELECT 1 FROM dbo.Eventos WHERE CodigoReferencia = @CodigoReferencia)
        THROW 50409, N'Ya existe un evento con ese código de referencia.', 1;
    IF @FechaEvento < CAST(GETDATE() AS DATE)
        THROW 50017, N'La fecha del evento debe ser futura.', 1;
    IF NOT EXISTS (SELECT 1 FROM dbo.Categorias WHERE CategoriaId = @CategoriaId AND Activo = 1)
        THROW 50011, N'La categoría seleccionada no existe o está inactiva.', 1;
    IF NOT EXISTS (SELECT 1 FROM dbo.Locaciones WHERE LocacionId = @LocacionId AND Activo = 1)
        THROW 50011, N'La locación seleccionada no existe o está inactiva.', 1;
    IF @Estado = 'Publicado'
        THROW 50013, N'Un evento nuevo no tiene tipos de entrada: créelo como Borrador y publíquelo después.', 1;

    INSERT INTO dbo.Eventos (CodigoReferencia, CategoriaId, LocacionId, Nombre, DescripcionCorta, DescripcionCompleta,
                             Organizador, ImagenUrl, FechaEvento, HoraEvento, Estado, FechaCreacion)
    VALUES (ISNULL(@CodigoReferencia, CONCAT('TMP', REPLACE(CONVERT(VARCHAR(36), NEWID()), '-', ''))), @CategoriaId, @LocacionId,
            @Nombre, @DescripcionCorta, @DescripcionCompleta, @Organizador, @ImagenUrl, @FechaEvento, @HoraEvento, @Estado, GETDATE());

    DECLARE @EventoId INT = CAST(SCOPE_IDENTITY() AS INT);
    IF @CodigoReferencia IS NULL
        UPDATE dbo.Eventos SET CodigoReferencia = CONCAT('EVT-', YEAR(@FechaEvento), '-', FORMAT(@EventoId, '00000'))
        WHERE EventoId = @EventoId;

    SELECT @EventoId AS EventoId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Evento_Actualizar
    @EventoId            INT,
    @CodigoReferencia    VARCHAR(50) = NULL,   -- NULL = conserva el actual
    @CategoriaId         INT,
    @LocacionId          INT,
    @Nombre              VARCHAR(150),
    @DescripcionCorta    VARCHAR(300) = NULL,
    @DescripcionCompleta VARCHAR(MAX),
    @Organizador         VARCHAR(150) = NULL,
    @ImagenUrl           VARCHAR(500) = NULL,
    @FechaEvento         DATE,
    @HoraEvento          TIME(0)
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM dbo.Eventos WHERE EventoId = @EventoId)
        THROW 50404, N'El evento no existe.', 1;
    IF @CodigoReferencia IS NOT NULL AND EXISTS (SELECT 1 FROM dbo.Eventos WHERE CodigoReferencia = @CodigoReferencia AND EventoId <> @EventoId)
        THROW 50409, N'Otro evento ya usa ese código de referencia.', 1;
    IF NOT EXISTS (SELECT 1 FROM dbo.Categorias WHERE CategoriaId = @CategoriaId)
        THROW 50011, N'La categoría seleccionada no existe.', 1;
    IF NOT EXISTS (SELECT 1 FROM dbo.Locaciones WHERE LocacionId = @LocacionId)
        THROW 50011, N'La locación seleccionada no existe.', 1;
    IF dbo.fn_CapacidadAsignadaEvento(@EventoId, NULL) > (SELECT CapacidadMaxima FROM dbo.Locaciones WHERE LocacionId = @LocacionId)
        THROW 50012, N'El aforo asignado a los tipos de entrada supera la capacidad de la nueva locación.', 1;

    UPDATE dbo.Eventos
    SET CodigoReferencia = ISNULL(@CodigoReferencia, CodigoReferencia), CategoriaId = @CategoriaId, LocacionId = @LocacionId, Nombre = @Nombre,
        DescripcionCorta = @DescripcionCorta, DescripcionCompleta = @DescripcionCompleta, Organizador = @Organizador,
        ImagenUrl = @ImagenUrl, FechaEvento = @FechaEvento, HoraEvento = @HoraEvento
    WHERE EventoId = @EventoId;
    SELECT @@ROWCOUNT AS Filas;
END
GO

/* Publicar requiere al menos un tipo de entrada activo.
   Cancelar anula reservas pendientes (libera cupo) y entradas activas. */
CREATE OR ALTER PROCEDURE dbo.sp_Evento_CambiarEstado
    @EventoId INT,
    @Estado   VARCHAR(30)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    DECLARE @EstadoActual VARCHAR(30) = (SELECT Estado FROM dbo.Eventos WHERE EventoId = @EventoId);
    IF @EstadoActual IS NULL
        THROW 50404, N'El evento no existe.', 1;
    IF @EstadoActual = 'Cancelado'
        THROW 50014, N'Un evento cancelado no puede cambiar de estado.', 1;
    IF @Estado = 'Publicado' AND NOT EXISTS (SELECT 1 FROM dbo.TiposEntrada WHERE EventoId = @EventoId AND Activo = 1)
        THROW 50013, N'No se puede publicar un evento sin tipos de entrada activos.', 1;

    BEGIN TRANSACTION;
        UPDATE dbo.Eventos SET Estado = @Estado WHERE EventoId = @EventoId;

        -- Si se publica sin cupo disponible queda directamente Agotado
        IF @Estado = 'Publicado' AND dbo.fn_DisponibilidadEvento(@EventoId) = 0
            UPDATE dbo.Eventos SET Estado = 'Agotado' WHERE EventoId = @EventoId;

        IF @Estado = 'Cancelado'
        BEGIN
            DECLARE @Liberados TABLE (PedidoId INT PRIMARY KEY);
            UPDATE dbo.Pedidos SET Estado = 'Cancelado'
            OUTPUT inserted.PedidoId INTO @Liberados
            WHERE EventoId = @EventoId AND Estado IN ('PendientePago', 'EnValidacion');

            UPDATE t SET CantidadComprometida = t.CantidadComprometida - x.Cantidad
            FROM dbo.TiposEntrada t
            INNER JOIN (SELECT d.TipoEntradaId, SUM(d.Cantidad) AS Cantidad
                        FROM dbo.PedidoDetalles d INNER JOIN @Liberados l ON l.PedidoId = d.PedidoId
                        GROUP BY d.TipoEntradaId) x ON x.TipoEntradaId = t.TipoEntradaId;

            UPDATE dbo.Entradas SET Estado = 'Anulada' WHERE EventoId = @EventoId AND Estado = 'Valida';
        END
    COMMIT TRANSACTION;
    SELECT 1 AS Filas;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Evento_Eliminar
    @EventoId INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    IF NOT EXISTS (SELECT 1 FROM dbo.Eventos WHERE EventoId = @EventoId)
        THROW 50404, N'El evento no existe.', 1;
    IF EXISTS (SELECT 1 FROM dbo.Pedidos WHERE EventoId = @EventoId)
        THROW 50409, N'El evento tiene pedidos registrados; cámbielo a estado Cancelado en lugar de eliminarlo.', 1;
    BEGIN TRANSACTION;
        DELETE FROM dbo.TiposEntrada WHERE EventoId = @EventoId;
        DELETE FROM dbo.Eventos WHERE EventoId = @EventoId;
    COMMIT TRANSACTION;
    SELECT 'Eliminado' AS Resultado;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_TipoEntrada_ListarPorEvento
    @EventoId   INT,
    @SoloActivos BIT = 0
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM dbo.vw_TiposEntradaDisponibilidad
    WHERE EventoId = @EventoId AND (@SoloActivos = 0 OR Activo = 1)
    ORDER BY Precio DESC, Nombre;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_TipoEntrada_ObtenerPorId
    @TipoEntradaId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM dbo.vw_TiposEntradaDisponibilidad WHERE TipoEntradaId = @TipoEntradaId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_TipoEntrada_Crear
    @EventoId        INT,
    @Nombre          VARCHAR(80),
    @Precio          DECIMAL(12,2),
    @ColorHex        VARCHAR(10) = NULL,
    @CantidadInicial INT,
    @Activo          BIT = 1
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @EstadoEvento VARCHAR(30), @Capacidad INT;
    SELECT @EstadoEvento = e.Estado, @Capacidad = l.CapacidadMaxima
    FROM dbo.Eventos e INNER JOIN dbo.Locaciones l ON l.LocacionId = e.LocacionId
    WHERE e.EventoId = @EventoId;

    IF @EstadoEvento IS NULL
        THROW 50404, N'El evento no existe.', 1;
    IF @EstadoEvento IN ('Cancelado', 'Finalizado')
        THROW 50014, N'No se pueden agregar tipos de entrada a un evento cancelado o finalizado.', 1;
    IF EXISTS (SELECT 1 FROM dbo.TiposEntrada WHERE EventoId = @EventoId AND Nombre = @Nombre)
        THROW 50409, N'El evento ya tiene un tipo de entrada con ese nombre.', 1;
    IF dbo.fn_CapacidadAsignadaEvento(@EventoId, NULL) + @CantidadInicial > @Capacidad
        THROW 50015, N'La cantidad supera la capacidad máxima disponible de la locación.', 1;

    INSERT INTO dbo.TiposEntrada (EventoId, Nombre, Precio, ColorHex, CantidadInicial, CantidadComprometida, CantidadVendida, Activo)
    VALUES (@EventoId, @Nombre, @Precio, @ColorHex, @CantidadInicial, 0, 0, @Activo);
    SELECT CAST(SCOPE_IDENTITY() AS INT) AS TipoEntradaId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_TipoEntrada_Actualizar
    @TipoEntradaId   INT,
    @Nombre          VARCHAR(80),
    @Precio          DECIMAL(12,2),
    @ColorHex        VARCHAR(10) = NULL,
    @CantidadInicial INT,
    @Activo          BIT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRANSACTION;
        DECLARE @EventoId INT, @Usadas INT, @Capacidad INT;
        SELECT @EventoId = t.EventoId, @Usadas = t.CantidadComprometida + t.CantidadVendida, @Capacidad = l.CapacidadMaxima
        FROM dbo.TiposEntrada t WITH (UPDLOCK, ROWLOCK)
        INNER JOIN dbo.Eventos e ON e.EventoId = t.EventoId
        INNER JOIN dbo.Locaciones l ON l.LocacionId = e.LocacionId
        WHERE t.TipoEntradaId = @TipoEntradaId;

        IF @EventoId IS NULL
            THROW 50404, N'El tipo de entrada no existe.', 1;
        IF EXISTS (SELECT 1 FROM dbo.TiposEntrada WHERE EventoId = @EventoId AND Nombre = @Nombre AND TipoEntradaId <> @TipoEntradaId)
            THROW 50409, N'El evento ya tiene otro tipo de entrada con ese nombre.', 1;
        IF @CantidadInicial < @Usadas
            THROW 50016, N'La cantidad inicial no puede ser menor que las entradas ya vendidas o reservadas.', 1;
        IF dbo.fn_CapacidadAsignadaEvento(@EventoId, @TipoEntradaId) + @CantidadInicial > @Capacidad
            THROW 50015, N'La cantidad supera la capacidad máxima disponible de la locación.', 1;

        UPDATE dbo.TiposEntrada
        SET Nombre = @Nombre, Precio = @Precio, ColorHex = @ColorHex, CantidadInicial = @CantidadInicial, Activo = @Activo
        WHERE TipoEntradaId = @TipoEntradaId;
    COMMIT TRANSACTION;
    SELECT 1 AS Filas;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_TipoEntrada_Eliminar
    @TipoEntradaId INT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM dbo.TiposEntrada WHERE TipoEntradaId = @TipoEntradaId)
        THROW 50404, N'El tipo de entrada no existe.', 1;
    IF EXISTS (SELECT 1 FROM dbo.PedidoDetalles WHERE TipoEntradaId = @TipoEntradaId)
    BEGIN
        UPDATE dbo.TiposEntrada SET Activo = 0 WHERE TipoEntradaId = @TipoEntradaId;
        SELECT 'Desactivado' AS Resultado;
        RETURN;
    END
    DELETE FROM dbo.TiposEntrada WHERE TipoEntradaId = @TipoEntradaId;
    SELECT 'Eliminado' AS Resultado;
END
GO

/* =============================================================================
   M3 · Procedimientos
   Módulo: Módulo 3 · Pedidos, historial, cancelación y vencimiento de reservas (HU-12, 13, 14)
   Objetos: sp_Pedido_LiberarVencidos, sp_Pedido_Crear, sp_Pedido_ObtenerPorId, sp_Pedido_ListarPorUsuario, sp_Pedido_Listar, sp_Pedido_ActualizarMetodoPago, sp_Pedido_Cancelar
   Depende de: base/01..06, modulo2 (vistas de eventos)
   ============================================================================= */
USE EventCR_BoleteriaPro_DB;
GO


/* Vence reservas expiradas (PendientePago) y devuelve el cupo comprometido */
CREATE OR ALTER PROCEDURE dbo.sp_Pedido_LiberarVencidos
    @Silencioso BIT = 0   -- 1 cuando lo invoca otro procedimiento (no devuelve resultado)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    DECLARE @Vencidos TABLE (PedidoId INT PRIMARY KEY);
    BEGIN TRANSACTION;
        UPDATE dbo.Pedidos WITH (UPDLOCK)
        SET Estado = 'Vencido'
        OUTPUT inserted.PedidoId INTO @Vencidos
        WHERE Estado = 'PendientePago' AND FechaVencimientoReserva < GETDATE();

        UPDATE t SET CantidadComprometida = t.CantidadComprometida - x.Cantidad
        FROM dbo.TiposEntrada t
        INNER JOIN (SELECT d.TipoEntradaId, SUM(d.Cantidad) AS Cantidad
                    FROM dbo.PedidoDetalles d INNER JOIN @Vencidos v ON v.PedidoId = d.PedidoId
                    GROUP BY d.TipoEntradaId) x ON x.TipoEntradaId = t.TipoEntradaId;
    COMMIT TRANSACTION;
    IF @Silencioso = 0
        SELECT COUNT(*) AS PedidosLiberados FROM @Vencidos;
END
GO

/* Crea un pedido y compromete el inventario de forma atómica.
   @DetallesJson: [{"TipoEntradaId":1,"Cantidad":2}, ...]
   El UPDATE condicional con UPDLOCK evita sobreventa ante compras simultáneas:
   si alguna línea no tiene cupo suficiente, no se actualiza y se revierte todo. */
CREATE OR ALTER PROCEDURE dbo.sp_Pedido_Crear
    @UsuarioId            INT,
    @EventoId             INT,
    @MetodoPago           VARCHAR(60),
    @DetallesJson         NVARCHAR(MAX),
    @PorcentajeCargo      DECIMAL(5,2),
    @PorcentajeIVA        DECIMAL(5,2),
    @MinutosReserva       INT,
    @MaxEntradasPorPedido INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    EXEC dbo.sp_Pedido_LiberarVencidos @Silencioso = 1;

    IF NOT EXISTS (SELECT 1 FROM dbo.Usuarios WHERE UsuarioId = @UsuarioId AND Estado = 'Activo')
        THROW 50403, N'El usuario no está activo para realizar compras.', 1;
    IF NOT EXISTS (SELECT 1 FROM dbo.Eventos WHERE EventoId = @EventoId AND Estado = 'Publicado'
                   AND FechaEvento >= CAST(GETDATE() AS DATE))
        THROW 50021, N'El evento no está disponible para la venta.', 1;
    IF ISJSON(@DetallesJson) <> 1
        THROW 50400, N'El detalle del pedido no tiene un formato válido.', 1;

    DECLARE @Detalle TABLE (TipoEntradaId INT PRIMARY KEY, Cantidad INT NOT NULL);
    INSERT INTO @Detalle (TipoEntradaId, Cantidad)
    SELECT j.TipoEntradaId, SUM(j.Cantidad)
    FROM OPENJSON(@DetallesJson) WITH (TipoEntradaId INT '$.TipoEntradaId', Cantidad INT '$.Cantidad') j
    WHERE j.Cantidad > 0
    GROUP BY j.TipoEntradaId;

    IF NOT EXISTS (SELECT 1 FROM @Detalle)
        THROW 50022, N'Debe seleccionar al menos una entrada.', 1;
    IF (SELECT SUM(Cantidad) FROM @Detalle) > @MaxEntradasPorPedido
        THROW 50023, N'Se superó la cantidad máxima de entradas permitida por pedido.', 1;
    IF EXISTS (SELECT 1 FROM @Detalle d LEFT JOIN dbo.TiposEntrada t
                   ON t.TipoEntradaId = d.TipoEntradaId AND t.EventoId = @EventoId AND t.Activo = 1
               WHERE t.TipoEntradaId IS NULL)
        THROW 50024, N'Alguno de los tipos de entrada no pertenece al evento o no está activo.', 1;

    DECLARE @Ahora DATETIME = GETDATE();
    DECLARE @Lineas INT = (SELECT COUNT(*) FROM @Detalle);
    DECLARE @PedidoId INT, @Subtotal DECIMAL(12,2), @Cargo DECIMAL(12,2), @IVA DECIMAL(12,2);

    BEGIN TRANSACTION;
        UPDATE t
        SET CantidadComprometida = t.CantidadComprometida + d.Cantidad
        FROM dbo.TiposEntrada t WITH (UPDLOCK, ROWLOCK)
        INNER JOIN @Detalle d ON d.TipoEntradaId = t.TipoEntradaId
        WHERE t.EventoId = @EventoId AND t.Activo = 1
          AND t.CantidadInicial - t.CantidadComprometida - t.CantidadVendida >= d.Cantidad;

        IF @@ROWCOUNT <> @Lineas
        BEGIN
            ROLLBACK TRANSACTION;
            THROW 50409, N'No hay suficientes entradas disponibles para completar el pedido.', 1;
        END

        SELECT @Subtotal = SUM(d.Cantidad * t.Precio)
        FROM @Detalle d INNER JOIN dbo.TiposEntrada t ON t.TipoEntradaId = d.TipoEntradaId;
        SET @Cargo = ROUND(@Subtotal * @PorcentajeCargo / 100.0, 2);
        SET @IVA   = ROUND((@Subtotal + @Cargo) * @PorcentajeIVA / 100.0, 2);

        INSERT INTO dbo.Pedidos (CodigoPedido, UsuarioId, EventoId, MetodoPagoSeleccionado, ReferenciaBancariaUnica,
                                 Subtotal, CargoServicio, ImpuestoIVA, ImporteTotal, FechaCreacion,
                                 FechaVencimientoReserva, Estado)
        VALUES (CONCAT('TMP', LEFT(REPLACE(CONVERT(VARCHAR(36), NEWID()), '-', ''), 27)), @UsuarioId, @EventoId, @MetodoPago,
                CONCAT('TMP', REPLACE(CONVERT(VARCHAR(36), NEWID()), '-', '')),
                @Subtotal, @Cargo, @IVA, @Subtotal + @Cargo + @IVA, @Ahora,
                DATEADD(MINUTE, @MinutosReserva, @Ahora), 'PendientePago');

        SET @PedidoId = CAST(SCOPE_IDENTITY() AS INT);

        UPDATE dbo.Pedidos
        SET CodigoPedido = CONCAT('PED-', FORMAT(@PedidoId, '000000')),
            ReferenciaBancariaUnica = CONCAT('BP', FORMAT(@PedidoId, '000000'), RIGHT(CONCAT('0000', ABS(CHECKSUM(NEWID())) % 10000), 4))
        WHERE PedidoId = @PedidoId;

        INSERT INTO dbo.PedidoDetalles (PedidoId, TipoEntradaId, Cantidad, PrecioUnitario, SubtotalLinea)
        SELECT @PedidoId, d.TipoEntradaId, d.Cantidad, t.Precio, d.Cantidad * t.Precio
        FROM @Detalle d INNER JOIN dbo.TiposEntrada t ON t.TipoEntradaId = d.TipoEntradaId;
    COMMIT TRANSACTION;

    SELECT @PedidoId AS PedidoId;
END
GO

/* Devuelve 2 conjuntos: (1) cabecera del pedido, (2) líneas de detalle */
CREATE OR ALTER PROCEDURE dbo.sp_Pedido_ObtenerPorId
    @PedidoId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM dbo.vw_PedidosResumen WHERE PedidoId = @PedidoId;

    SELECT d.PedidoDetalleId, d.PedidoId, d.TipoEntradaId, t.Nombre AS TipoEntradaNombre, t.ColorHex,
           d.Cantidad, d.PrecioUnitario, d.SubtotalLinea
    FROM dbo.PedidoDetalles d
    INNER JOIN dbo.TiposEntrada t ON t.TipoEntradaId = d.TipoEntradaId
    WHERE d.PedidoId = @PedidoId
    ORDER BY d.PedidoDetalleId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Pedido_ListarPorUsuario
    @UsuarioId INT,
    @Estado    VARCHAR(30) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM dbo.vw_PedidosResumen
    WHERE UsuarioId = @UsuarioId AND (@Estado IS NULL OR Estado = @Estado)
    ORDER BY FechaCreacion DESC;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Pedido_Listar
    @Estado   VARCHAR(30)  = NULL,
    @EventoId INT          = NULL,
    @Busqueda VARCHAR(150) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM dbo.vw_PedidosResumen
    WHERE (@Estado IS NULL OR Estado = @Estado)
      AND (@EventoId IS NULL OR EventoId = @EventoId)
      AND (@Busqueda IS NULL OR CodigoPedido LIKE '%' + @Busqueda + '%'
                             OR CompradorNombre LIKE '%' + @Busqueda + '%'
                             OR CompradorCorreo LIKE '%' + @Busqueda + '%')
    ORDER BY FechaCreacion DESC;
END
GO

/* UPDATE del CRUD de pedidos: solo el método de pago es modificable mientras está PendientePago
   (las líneas y precios quedan congelados al crear el pedido). */
CREATE OR ALTER PROCEDURE dbo.sp_Pedido_ActualizarMetodoPago
    @PedidoId   INT,
    @UsuarioId  INT = NULL,     -- NULL = personal administrativo
    @MetodoPago VARCHAR(60)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Estado VARCHAR(30), @Propietario INT;
    SELECT @Estado = Estado, @Propietario = UsuarioId FROM dbo.Pedidos WHERE PedidoId = @PedidoId;
    IF @Estado IS NULL
        THROW 50404, N'El pedido no existe.', 1;
    IF @UsuarioId IS NOT NULL AND @Propietario <> @UsuarioId
        THROW 50403, N'El pedido no pertenece al usuario.', 1;
    IF @Estado <> 'PendientePago'
        THROW 50035, N'Solo se puede cambiar el método de pago de un pedido pendiente de pago.', 1;
    UPDATE dbo.Pedidos SET MetodoPagoSeleccionado = @MetodoPago WHERE PedidoId = @PedidoId;
    SELECT 1 AS Filas;
END
GO

/* Cancela un pedido pendiente de pago y libera el cupo.
   @UsuarioId NULL = cancelación por personal administrativo. */
CREATE OR ALTER PROCEDURE dbo.sp_Pedido_Cancelar
    @PedidoId  INT,
    @UsuarioId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRANSACTION;
        DECLARE @Estado VARCHAR(30), @Propietario INT;
        SELECT @Estado = Estado, @Propietario = UsuarioId
        FROM dbo.Pedidos WITH (UPDLOCK, ROWLOCK) WHERE PedidoId = @PedidoId;

        IF @Estado IS NULL
            THROW 50404, N'El pedido no existe.', 1;
        IF @UsuarioId IS NOT NULL AND @Propietario <> @UsuarioId
            THROW 50403, N'El pedido no pertenece al usuario.', 1;
        IF @Estado <> 'PendientePago'
            THROW 50030, N'Solo se pueden cancelar pedidos pendientes de pago.', 1;

        UPDATE dbo.Pedidos SET Estado = 'Cancelado' WHERE PedidoId = @PedidoId;

        UPDATE t SET CantidadComprometida = t.CantidadComprometida - d.Cantidad
        FROM dbo.TiposEntrada t INNER JOIN dbo.PedidoDetalles d ON d.TipoEntradaId = t.TipoEntradaId
        WHERE d.PedidoId = @PedidoId;
    COMMIT TRANSACTION;
    SELECT 1 AS Filas;
END
GO

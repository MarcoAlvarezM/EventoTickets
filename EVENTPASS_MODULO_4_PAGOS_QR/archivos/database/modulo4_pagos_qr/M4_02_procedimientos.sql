/* =============================================================================
   M4 · Procedimientos
   Módulo: Módulo 4 · Pagos, auditoría, entradas QR y control de acceso (HU-15 a HU-19)
   Objetos:
     sp_Pago_Registrar
     sp_Pago_Listar
     sp_Pago_ListarPorUsuario
     sp_Pago_ListarPorPedido
     sp_Pago_ObtenerPorId
     sp_Pago_Aprobar
     sp_Pago_Rechazar
     sp_Entrada_ListarPorUsuario
     sp_Entrada_ListarPorEvento
     sp_Entrada_ObtenerPorId
     sp_Entrada_Transferir
     sp_Entrada_RegistrarIngreso
   Depende de: base/01..06, modulo3 (vw_PedidosResumen, sp_Pedido_LiberarVencidos)
   ============================================================================= */
USE EventCR_BoleteriaPro_DB;
GO

CREATE OR ALTER PROCEDURE dbo.sp_Pago_Registrar
    @PedidoId              INT,
    @UsuarioId             INT,
    @BancoOrigen           VARCHAR(80),
    @ReferenciaComprobante VARCHAR(100),
    @MontoReportado        DECIMAL(12,2),
    @ComprobanteUrl        VARCHAR(500) = NULL,
    @HashComprobante       VARCHAR(64),
    @NotaComprador         VARCHAR(255) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    EXEC dbo.sp_Pedido_LiberarVencidos @Silencioso = 1;

    DECLARE @PagoId INT;
    BEGIN TRANSACTION;
        DECLARE @Estado VARCHAR(30), @Propietario INT, @Total DECIMAL(12,2);
        SELECT @Estado = Estado, @Propietario = UsuarioId, @Total = ImporteTotal
        FROM dbo.Pedidos WITH (UPDLOCK, ROWLOCK) WHERE PedidoId = @PedidoId;

        IF @Estado IS NULL
            THROW 50404, N'El pedido no existe.', 1;
        IF @Propietario <> @UsuarioId
            THROW 50403, N'El pedido no pertenece al usuario.', 1;
        IF @Estado <> 'PendientePago'
            THROW 50031, N'El pedido no admite reportes de pago en su estado actual (puede estar vencido, cancelado o en validación).', 1;
        IF @MontoReportado < @Total
            THROW 50032, N'El monto reportado no cubre el importe total del pedido.', 1;
        IF EXISTS (SELECT 1 FROM dbo.Pagos WITH (UPDLOCK)
                   WHERE Estado <> 'Rechazado'
                     AND (HashComprobante = @HashComprobante
                          OR (BancoOrigen = @BancoOrigen AND ReferenciaComprobante = @ReferenciaComprobante)))
            THROW 50409, N'Este comprobante o referencia bancaria ya fue registrado en otro pago.', 1;

        INSERT INTO dbo.Pagos (CodigoPago, PedidoId, BancoOrigen, ReferenciaComprobante, MontoReportado,
                               ComprobanteUrl, HashComprobante, NotaComprador, Estado, FechaReporte)
        VALUES (CONCAT('TMP', LEFT(REPLACE(CONVERT(VARCHAR(36), NEWID()), '-', ''), 27)), @PedidoId, @BancoOrigen,
                @ReferenciaComprobante, @MontoReportado, @ComprobanteUrl, @HashComprobante, @NotaComprador,
                'PendienteValidacion', GETDATE());

        SET @PagoId = CAST(SCOPE_IDENTITY() AS INT);
        UPDATE dbo.Pagos SET CodigoPago = CONCAT('PAG-', FORMAT(@PagoId, '000000')) WHERE PagoId = @PagoId;
        UPDATE dbo.Pedidos SET Estado = 'EnValidacion' WHERE PedidoId = @PedidoId;
    COMMIT TRANSACTION;

    SELECT @PagoId AS PagoId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Pago_Listar
    @Estado   VARCHAR(30)  = NULL,
    @Busqueda VARCHAR(150) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM dbo.vw_PagosDetalle
    WHERE (@Estado IS NULL OR Estado = @Estado)
      AND (@Busqueda IS NULL OR CodigoPago LIKE '%' + @Busqueda + '%' OR CodigoPedido LIKE '%' + @Busqueda + '%'
                             OR CompradorNombre LIKE '%' + @Busqueda + '%' OR ReferenciaComprobante LIKE '%' + @Busqueda + '%')
    ORDER BY CASE WHEN Estado = 'PendienteValidacion' THEN 0 ELSE 1 END, FechaReporte DESC;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Pago_ListarPorUsuario
    @UsuarioId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM dbo.vw_PagosDetalle WHERE CompradorUsuarioId = @UsuarioId ORDER BY FechaReporte DESC;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Pago_ListarPorPedido
    @PedidoId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM dbo.vw_PagosDetalle WHERE PedidoId = @PedidoId ORDER BY FechaReporte DESC;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Pago_ObtenerPorId
    @PagoId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM dbo.vw_PagosDetalle WHERE PagoId = @PagoId;
END
GO

/* Aprueba el pago, confirma el pedido, convierte reservas en ventas y emite
   una entrada por cada unidad comprada (código único + hash SHA-256). */
CREATE OR ALTER PROCEDURE dbo.sp_Pago_Aprobar
    @PagoId           INT,
    @AuditorUsuarioId INT,
    @CodigoAprobacion VARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF NOT EXISTS (SELECT 1 FROM dbo.Usuarios WHERE UsuarioId = @AuditorUsuarioId AND Estado = 'Activo'
                   AND Rol IN ('AuditorPagos', 'Superadministrador'))
        THROW 50403, N'El usuario no tiene permisos para validar pagos.', 1;

    BEGIN TRANSACTION;
        DECLARE @PedidoId INT, @EstadoPago VARCHAR(30), @EstadoPedido VARCHAR(30);
        SELECT @PedidoId = pg.PedidoId, @EstadoPago = pg.Estado
        FROM dbo.Pagos pg WITH (UPDLOCK, ROWLOCK) WHERE pg.PagoId = @PagoId;

        IF @PedidoId IS NULL
            THROW 50404, N'El pago no existe.', 1;
        IF @EstadoPago <> 'PendienteValidacion'
            THROW 50033, N'El pago ya fue validado anteriormente.', 1;

        SELECT @EstadoPedido = Estado FROM dbo.Pedidos WITH (UPDLOCK, ROWLOCK) WHERE PedidoId = @PedidoId;
        IF @EstadoPedido <> 'EnValidacion'
            THROW 50034, N'El pedido asociado no está en validación (pudo ser cancelado).', 1;

        UPDATE dbo.Pagos
        SET Estado = 'Aprobado', AuditorUsuarioId = @AuditorUsuarioId, FechaValidacion = GETDATE(),
            CodigoAprobacion = ISNULL(NULLIF(@CodigoAprobacion, ''), CONCAT('APR-', FORMAT(@PagoId, '000000'), '-',
                                      LEFT(REPLACE(CONVERT(VARCHAR(36), NEWID()), '-', ''), 6)))
        WHERE PagoId = @PagoId;

        UPDATE dbo.Pedidos SET Estado = 'Pagado' WHERE PedidoId = @PedidoId;

        -- Emisión: una fila por unidad (tabla de números a partir de catálogos del sistema)
        ;WITH Numeros AS (
            SELECT TOP (1000) ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS N
            FROM sys.all_objects a CROSS JOIN sys.all_objects b
        )
        INSERT INTO dbo.Entradas (CodigoTicket, PedidoId, EventoId, TipoEntradaId, NombreAsistente, DocumentoAsistente,
                                  AsientoAsignado, PuertaAcceso, HashCriptografico, Estado, FechaEmision)
        SELECT
            CONCAT('TKT-', FORMAT(p.PedidoId, '000000'), '-',
                   FORMAT(ROW_NUMBER() OVER (ORDER BY d.PedidoDetalleId, n.N), '000'), '-',
                   LEFT(REPLACE(CONVERT(VARCHAR(36), NEWID()), '-', ''), 6)),
            p.PedidoId, p.EventoId, d.TipoEntradaId, u.NombreCompleto, u.NumeroDocumento,
            CONCAT(UPPER(LEFT(t.Nombre, 3)), '-', t.CantidadVendida + n.N),
            LEFT(l.PabellonPuerta, 50),
            LOWER(CONVERT(VARCHAR(64), HASHBYTES('SHA2_256',
                  CONCAT(p.PedidoId, '|', d.TipoEntradaId, '|', n.N, '|', CONVERT(VARCHAR(36), NEWID()))), 2)),
            'Valida', GETDATE()
        FROM dbo.Pedidos p
        INNER JOIN dbo.Usuarios u       ON u.UsuarioId = p.UsuarioId
        INNER JOIN dbo.PedidoDetalles d ON d.PedidoId = p.PedidoId
        INNER JOIN dbo.TiposEntrada t   ON t.TipoEntradaId = d.TipoEntradaId
        INNER JOIN dbo.Eventos e        ON e.EventoId = p.EventoId
        INNER JOIN dbo.Locaciones l     ON l.LocacionId = e.LocacionId
        INNER JOIN Numeros n            ON n.N <= d.Cantidad
        WHERE p.PedidoId = @PedidoId;

        DECLARE @Emitidas INT = @@ROWCOUNT;

        UPDATE t
        SET CantidadComprometida = t.CantidadComprometida - d.Cantidad,
            CantidadVendida      = t.CantidadVendida + d.Cantidad
        FROM dbo.TiposEntrada t INNER JOIN dbo.PedidoDetalles d ON d.TipoEntradaId = t.TipoEntradaId
        WHERE d.PedidoId = @PedidoId;
    COMMIT TRANSACTION;

    SELECT @Emitidas AS EntradasEmitidas;
END
GO

/* Rechaza el pago; el pedido vuelve a PendientePago con un plazo de gracia
   para que el comprador reporte un comprobante correcto. */
CREATE OR ALTER PROCEDURE dbo.sp_Pago_Rechazar
    @PagoId           INT,
    @AuditorUsuarioId INT,
    @MotivoRechazo    VARCHAR(255),
    @MinutosGracia    INT = 60
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF NOT EXISTS (SELECT 1 FROM dbo.Usuarios WHERE UsuarioId = @AuditorUsuarioId AND Estado = 'Activo'
                   AND Rol IN ('AuditorPagos', 'Superadministrador'))
        THROW 50403, N'El usuario no tiene permisos para validar pagos.', 1;

    BEGIN TRANSACTION;
        DECLARE @PedidoId INT, @EstadoPago VARCHAR(30);
        SELECT @PedidoId = PedidoId, @EstadoPago = Estado FROM dbo.Pagos WITH (UPDLOCK, ROWLOCK) WHERE PagoId = @PagoId;
        IF @PedidoId IS NULL
            THROW 50404, N'El pago no existe.', 1;
        IF @EstadoPago <> 'PendienteValidacion'
            THROW 50033, N'El pago ya fue validado anteriormente.', 1;

        UPDATE dbo.Pagos
        SET Estado = 'Rechazado', MotivoRechazo = @MotivoRechazo, AuditorUsuarioId = @AuditorUsuarioId, FechaValidacion = GETDATE()
        WHERE PagoId = @PagoId;

        UPDATE dbo.Pedidos
        SET Estado = 'PendientePago', FechaVencimientoReserva = DATEADD(MINUTE, @MinutosGracia, GETDATE())
        WHERE PedidoId = @PedidoId AND Estado = 'EnValidacion';
    COMMIT TRANSACTION;
    SELECT 1 AS Filas;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Entrada_ListarPorUsuario
    @UsuarioId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM dbo.vw_EntradasDetalle WHERE UsuarioId = @UsuarioId ORDER BY FechaEvento, EntradaId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Entrada_ListarPorEvento
    @EventoId INT,
    @Estado   VARCHAR(30) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM dbo.vw_EntradasDetalle
    WHERE EventoId = @EventoId AND (@Estado IS NULL OR Estado = @Estado)
    ORDER BY EntradaId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Entrada_ObtenerPorId
    @EntradaId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM dbo.vw_EntradasDetalle WHERE EntradaId = @EntradaId;
END
GO

/* Cambia el titular (nominación) y regenera el hash: el QR anterior deja de ser válido */
CREATE OR ALTER PROCEDURE dbo.sp_Entrada_Transferir
    @EntradaId          INT,
    @UsuarioId          INT,
    @NombreAsistente    VARCHAR(150),
    @DocumentoAsistente VARCHAR(40)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRANSACTION;
        DECLARE @Estado VARCHAR(30), @Propietario INT, @FechaEvento DATE, @EstadoEvento VARCHAR(30);
        SELECT @Estado = en.Estado, @Propietario = p.UsuarioId, @FechaEvento = e.FechaEvento, @EstadoEvento = e.Estado
        FROM dbo.Entradas en WITH (UPDLOCK, ROWLOCK)
        INNER JOIN dbo.Pedidos p ON p.PedidoId = en.PedidoId
        INNER JOIN dbo.Eventos e ON e.EventoId = en.EventoId
        WHERE en.EntradaId = @EntradaId;

        IF @Estado IS NULL
            THROW 50404, N'La entrada no existe.', 1;
        IF @Propietario <> @UsuarioId
            THROW 50403, N'La entrada no pertenece al usuario.', 1;
        IF @Estado <> 'Valida'
            THROW 50040, N'Solo se pueden transferir entradas activas.', 1;
        IF @EstadoEvento = 'Cancelado' OR @FechaEvento < CAST(GETDATE() AS DATE)
            THROW 50041, N'El evento ya ocurrió o fue cancelado; la entrada no puede transferirse.', 1;

        UPDATE dbo.Entradas
        SET NombreAsistente = @NombreAsistente, DocumentoAsistente = @DocumentoAsistente,
            HashCriptografico = LOWER(CONVERT(VARCHAR(64), HASHBYTES('SHA2_256',
                                CONCAT(CodigoTicket, '|', @DocumentoAsistente, '|', CONVERT(VARCHAR(36), NEWID()))), 2))
        WHERE EntradaId = @EntradaId;
    COMMIT TRANSACTION;
    SELECT * FROM dbo.vw_EntradasDetalle WHERE EntradaId = @EntradaId;
END
GO

/* Control de acceso en puerta: marca la entrada como utilizada una sola vez */
CREATE OR ALTER PROCEDURE dbo.sp_Entrada_RegistrarIngreso
    @CodigoTicket        VARCHAR(50),
    @HashCriptografico   VARCHAR(64) = NULL,
    @ValidadorUsuarioId  INT,
    @PuertaAcceso        VARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF NOT EXISTS (SELECT 1 FROM dbo.Usuarios WHERE UsuarioId = @ValidadorUsuarioId AND Estado = 'Activo'
                   AND Rol IN ('ValidadorAcceso', 'Superadministrador'))
        THROW 50403, N'El usuario no tiene permisos para validar accesos.', 1;

    BEGIN TRANSACTION;
        DECLARE @EntradaId INT, @Estado VARCHAR(30), @Hash VARCHAR(64), @Ingreso DATETIME, @EstadoEvento VARCHAR(30);
        SELECT @EntradaId = en.EntradaId, @Estado = en.Estado, @Hash = en.HashCriptografico,
               @Ingreso = en.FechaHoraIngreso, @EstadoEvento = e.Estado
        FROM dbo.Entradas en WITH (UPDLOCK, ROWLOCK)
        INNER JOIN dbo.Eventos e ON e.EventoId = en.EventoId
        WHERE en.CodigoTicket = @CodigoTicket;

        IF @EntradaId IS NULL
            THROW 50404, N'No existe una entrada con ese código.', 1;
        IF @HashCriptografico IS NOT NULL AND @HashCriptografico <> @Hash
            THROW 50045, N'El código QR no es válido (fue regenerado por una transferencia).', 1;
        IF @EstadoEvento = 'Cancelado' OR @Estado = 'Anulada'
            THROW 50042, N'La entrada está anulada.', 1;
        IF @Estado = 'Utilizada'
        BEGIN
            DECLARE @Msg NVARCHAR(200) = CONCAT(N'La entrada ya fue utilizada el ', FORMAT(@Ingreso, 'dd/MM/yyyy HH:mm'), N'.');
            THROW 50043, @Msg, 1;
        END

        UPDATE dbo.Entradas
        SET Estado = 'Utilizada', FechaHoraIngreso = GETDATE(), ValidadorPuertaUsuarioId = @ValidadorUsuarioId,
            PuertaAcceso = ISNULL(NULLIF(@PuertaAcceso, ''), PuertaAcceso)
        WHERE EntradaId = @EntradaId;
    COMMIT TRANSACTION;
    SELECT * FROM dbo.vw_EntradasDetalle WHERE EntradaId = @EntradaId;
END
GO

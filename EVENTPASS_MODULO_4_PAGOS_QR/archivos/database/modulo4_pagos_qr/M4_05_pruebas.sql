/* =============================================================================
   M4 · Pruebas de base de datos (HU-15 a HU-19)
   Ejecutar después de instalar toda la base (00_EJECUTAR_TODO.sql).
   Resultado esperado: todas las filas en 'OK'. No deja cambios.
   ============================================================================= */
USE EventCR_BoleteriaPro_DB;
GO
SET NOCOUNT ON;
DECLARE @Resultados TABLE (Prueba NVARCHAR(200), Resultado VARCHAR(10), Detalle NVARCHAR(400));
DECLARE @Id TABLE (Valor INT);
DECLARE @Auditor INT = (SELECT TOP (1) UsuarioId FROM dbo.Usuarios WHERE Rol = 'AuditorPagos' AND Estado = 'Activo');
DECLARE @Validador INT = (SELECT TOP (1) UsuarioId FROM dbo.Usuarios WHERE Rol = 'ValidadorAcceso' AND Estado = 'Activo');
DECLARE @HashNuevo VARCHAR(64) = LOWER(CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', CONCAT('prueba-m4-', NEWID())), 2));

-- HU-15 → HU-17 → HU-18 → HU-19 · Registrar pago, aprobar, emitir entradas y registrar ingreso
BEGIN TRY
    BEGIN TRANSACTION;
    DECLARE @VendAntes INT = (SELECT CantidadVendida FROM dbo.TiposEntrada WHERE TipoEntradaId = 5);
    INSERT INTO @Id EXEC dbo.sp_Pedido_Crear 6, 2, 'SINPE Movil', N'[{"TipoEntradaId":5,"Cantidad":2}]', 10, 13, 30, 10;
    DECLARE @P INT = (SELECT MAX(Valor) FROM @Id); DELETE FROM @Id;
    DECLARE @Total DECIMAL(12,2) = (SELECT ImporteTotal FROM dbo.Pedidos WHERE PedidoId = @P);
    INSERT INTO @Id EXEC dbo.sp_Pago_Registrar @P, 6, 'BAC Credomatic', 'PRUEBA-M4-001', @Total, NULL, @HashNuevo, NULL;
    DECLARE @Pago INT = (SELECT MAX(Valor) FROM @Id); DELETE FROM @Id;
    INSERT INTO @Resultados
    SELECT N'Registrar pago: PendienteValidacion y pedido EnValidacion',
           CASE WHEN pg.Estado = 'PendienteValidacion' AND p.Estado = 'EnValidacion' AND LEN(pg.HashComprobante) = 64 THEN 'OK' ELSE 'FALLA' END,
           CONCAT(pg.Estado, ' / ', p.Estado)
    FROM dbo.Pagos pg INNER JOIN dbo.Pedidos p ON p.PedidoId = pg.PedidoId WHERE pg.PagoId = @Pago;

    INSERT INTO @Id EXEC dbo.sp_Pago_Aprobar @Pago, @Auditor, NULL;
    DECLARE @Emitidas INT = (SELECT MAX(Valor) FROM @Id); DELETE FROM @Id;
    INSERT INTO @Resultados
    SELECT N'Aprobar: pedido Pagado, auditor registrado, entradas emitidas',
           CASE WHEN pg.Estado = 'Aprobado' AND pg.AuditorUsuarioId = @Auditor AND pg.FechaValidacion IS NOT NULL
                     AND p.Estado = 'Pagado' AND @Emitidas = 2
                     AND (SELECT CantidadVendida FROM dbo.TiposEntrada WHERE TipoEntradaId = 5) = @VendAntes + 2 THEN 'OK' ELSE 'FALLA' END,
           CONCAT(pg.Estado, ' / ', p.Estado, ' / ', @Emitidas, ' entradas')
    FROM dbo.Pagos pg INNER JOIN dbo.Pedidos p ON p.PedidoId = pg.PedidoId WHERE pg.PagoId = @Pago;

    INSERT INTO @Resultados
    SELECT N'Entradas Válidas con código único y hash SHA-256',
           CASE WHEN COUNT(*) = 2 AND MIN(LEN(HashCriptografico)) = 64 AND COUNT(DISTINCT CodigoTicket) = 2
                     AND SUM(CASE WHEN Estado = 'Valida' THEN 1 ELSE 0 END) = 2 THEN 'OK' ELSE 'FALLA' END,
           CONCAT(COUNT(*), ' entradas')
    FROM dbo.Entradas WHERE PedidoId = @P;

    DECLARE @Ent INT = (SELECT MIN(EntradaId) FROM dbo.Entradas WHERE PedidoId = @P);
    DECLARE @HashAntes VARCHAR(64) = (SELECT HashCriptografico FROM dbo.Entradas WHERE EntradaId = @Ent);
    DECLARE @Cod VARCHAR(50) = (SELECT CodigoTicket FROM dbo.Entradas WHERE EntradaId = @Ent);
    EXEC dbo.sp_Entrada_Transferir @Ent, 6, 'Asistente Nominado', '1-2345-6789';
    INSERT INTO @Resultados
    SELECT N'Nominación/transferencia regenera el hash del QR', CASE WHEN HashCriptografico <> @HashAntes AND NombreAsistente = 'Asistente Nominado' THEN 'OK' ELSE 'FALLA' END, NULL
    FROM dbo.Entradas WHERE EntradaId = @Ent;

    DECLARE @HashActual VARCHAR(64) = (SELECT HashCriptografico FROM dbo.Entradas WHERE EntradaId = @Ent);
    EXEC dbo.sp_Entrada_RegistrarIngreso @Cod, @HashActual, @Validador, 'Puerta 1';
    INSERT INTO @Resultados
    SELECT N'Registrar ingreso: Utilizada con validador y hora', CASE WHEN Estado = 'Utilizada' AND ValidadorPuertaUsuarioId = @Validador AND FechaHoraIngreso IS NOT NULL THEN 'OK' ELSE 'FALLA' END, Estado
    FROM dbo.Entradas WHERE EntradaId = @Ent;
    ROLLBACK TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    INSERT INTO @Resultados VALUES (N'Flujo pago → aprobación → QR → ingreso', 'FALLA', ERROR_MESSAGE());
END CATCH

-- HU-17 · Rechazo con motivo: pago Rechazado y pedido vuelve a PendientePago
BEGIN TRY
    BEGIN TRANSACTION;
    INSERT INTO @Id EXEC dbo.sp_Pedido_Crear 6, 2, 'SINPE Movil', N'[{"TipoEntradaId":5,"Cantidad":1}]', 10, 13, 30, 10;
    DECLARE @PR INT = (SELECT MAX(Valor) FROM @Id); DELETE FROM @Id;
    DECLARE @TotalR DECIMAL(12,2) = (SELECT ImporteTotal FROM dbo.Pedidos WHERE PedidoId = @PR);
    DECLARE @HashR VARCHAR(64) = LOWER(CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', CONCAT('prueba-m4-r-', NEWID())), 2));
    INSERT INTO @Id EXEC dbo.sp_Pago_Registrar @PR, 6, 'Banco Nacional', 'PRUEBA-M4-002', @TotalR, NULL, @HashR, NULL;
    DECLARE @PagoR INT = (SELECT MAX(Valor) FROM @Id); DELETE FROM @Id;
    INSERT INTO @Id EXEC dbo.sp_Pago_Rechazar @PagoR, @Auditor, 'Comprobante ilegible', 60; DELETE FROM @Id;
    INSERT INTO @Resultados
    SELECT N'Rechazar pago con motivo', CASE WHEN pg.Estado = 'Rechazado' AND pg.MotivoRechazo IS NOT NULL AND p.Estado = 'PendientePago'
                                              AND NOT EXISTS (SELECT 1 FROM dbo.Entradas WHERE PedidoId = @PR) THEN 'OK' ELSE 'FALLA' END,
           CONCAT(pg.Estado, ' / ', p.Estado)
    FROM dbo.Pagos pg INNER JOIN dbo.Pedidos p ON p.PedidoId = pg.PedidoId WHERE pg.PagoId = @PagoR;
    ROLLBACK TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    INSERT INTO @Resultados VALUES (N'Rechazar pago con motivo', 'FALLA', ERROR_MESSAGE());
END CATCH

-- Las dos pruebas siguientes crean su propio pedido dentro de una transacción; el error esperado
-- (XACT_ABORT) la deja no confirmable y el CATCH la revierte, así que no quedan cambios.

-- HU-15 · Comprobante duplicado (mismo SHA-256 que un pago ya aprobado)
BEGIN TRY
    BEGIN TRANSACTION;
    INSERT INTO @Id EXEC dbo.sp_Pedido_Crear 7, 2, 'SINPE Movil', N'[{"TipoEntradaId":5,"Cantidad":1}]', 10, 13, 30, 10;
    DECLARE @PD INT = (SELECT MAX(Valor) FROM @Id); DELETE FROM @Id;
    DECLARE @HashUsado VARCHAR(64) = (SELECT TOP (1) HashComprobante FROM dbo.Pagos WHERE Estado = 'Aprobado');
    EXEC dbo.sp_Pago_Registrar @PD, 7, 'BAC Credomatic', 'OTRA-REF-1', 999999, NULL, @HashUsado, NULL;
    INSERT INTO @Resultados VALUES (N'Rechaza comprobante duplicado', 'FALLA', N'Se aceptó');
    ROLLBACK TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    INSERT INTO @Resultados VALUES (N'Rechaza comprobante duplicado', CASE WHEN ERROR_NUMBER() = 50409 THEN 'OK' ELSE 'FALLA' END, ERROR_MESSAGE());
END CATCH
DELETE FROM @Id;

-- HU-15 · Monto inferior al total
BEGIN TRY
    BEGIN TRANSACTION;
    INSERT INTO @Id EXEC dbo.sp_Pedido_Crear 7, 2, 'SINPE Movil', N'[{"TipoEntradaId":5,"Cantidad":1}]', 10, 13, 30, 10;
    DECLARE @PM INT = (SELECT MAX(Valor) FROM @Id); DELETE FROM @Id;
    EXEC dbo.sp_Pago_Registrar @PM, 7, 'BAC Credomatic', 'MONTO-BAJO-1', 1, NULL, @HashNuevo, NULL;
    INSERT INTO @Resultados VALUES (N'Rechaza monto insuficiente', 'FALLA', N'Se aceptó');
    ROLLBACK TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    INSERT INTO @Resultados VALUES (N'Rechaza monto insuficiente', CASE WHEN ERROR_NUMBER() = 50032 THEN 'OK' ELSE 'FALLA' END, ERROR_MESSAGE());
END CATCH
DELETE FROM @Id;

-- HU-16 · Un comprador no puede auditar pagos (verificación de rol en el SP)
BEGIN TRY
    DECLARE @PagoPend INT = (SELECT TOP (1) PagoId FROM dbo.Pagos WHERE Estado = 'PendienteValidacion');
    EXEC dbo.sp_Pago_Aprobar @PagoPend, 5, NULL;
    INSERT INTO @Resultados VALUES (N'Solo auditores aprueban pagos', 'FALLA', N'Se aprobó');
END TRY
BEGIN CATCH
    INSERT INTO @Resultados VALUES (N'Solo auditores aprueban pagos', CASE WHEN ERROR_NUMBER() = 50403 THEN 'OK' ELSE 'FALLA' END, ERROR_MESSAGE());
END CATCH

-- HU-17 · Un pago ya validado no se vuelve a aprobar
BEGIN TRY
    DECLARE @PagoAprobado INT = (SELECT TOP (1) PagoId FROM dbo.Pagos WHERE Estado = 'Aprobado');
    EXEC dbo.sp_Pago_Aprobar @PagoAprobado, @Auditor, NULL;
    INSERT INTO @Resultados VALUES (N'Pago validado no se reprocesa', 'FALLA', N'Se aprobó dos veces');
END TRY
BEGIN CATCH
    INSERT INTO @Resultados VALUES (N'Pago validado no se reprocesa', CASE WHEN ERROR_NUMBER() = 50033 THEN 'OK' ELSE 'FALLA' END, ERROR_MESSAGE());
END CATCH

-- HU-19 · Validador sin rol
BEGIN TRY
    DECLARE @CodigoCualquiera VARCHAR(50) = (SELECT TOP (1) CodigoTicket FROM dbo.Entradas);
    EXEC dbo.sp_Entrada_RegistrarIngreso @CodigoCualquiera, NULL, 5, NULL;
    INSERT INTO @Resultados VALUES (N'Solo validadores registran ingresos', 'FALLA', N'Se aceptó');
END TRY
BEGIN CATCH
    INSERT INTO @Resultados VALUES (N'Solo validadores registran ingresos', CASE WHEN ERROR_NUMBER() = 50403 THEN 'OK' ELSE 'FALLA' END, ERROR_MESSAGE());
END CATCH

-- HU-19 · Código inexistente
BEGIN TRY
    EXEC dbo.sp_Entrada_RegistrarIngreso 'NO-EXISTE-000', NULL, @Validador, NULL;
    INSERT INTO @Resultados VALUES (N'Código de entrada inexistente', 'FALLA', N'Se aceptó');
END TRY
BEGIN CATCH
    INSERT INTO @Resultados VALUES (N'Código de entrada inexistente', CASE WHEN ERROR_NUMBER() = 50404 THEN 'OK' ELSE 'FALLA' END, ERROR_MESSAGE());
END CATCH

-- HU-19 · Doble ingreso con la misma entrada (se restaura la entrada al final)
DECLARE @EntradaPrueba INT = (SELECT TOP (1) EntradaId FROM dbo.Entradas WHERE Estado = 'Valida' ORDER BY EntradaId);
DECLARE @Codigo VARCHAR(50) = (SELECT CodigoTicket FROM dbo.Entradas WHERE EntradaId = @EntradaPrueba);
DECLARE @PuertaOriginal VARCHAR(50) = (SELECT PuertaAcceso FROM dbo.Entradas WHERE EntradaId = @EntradaPrueba);
BEGIN TRY
    EXEC dbo.sp_Entrada_RegistrarIngreso @Codigo, NULL, @Validador, NULL;
    EXEC dbo.sp_Entrada_RegistrarIngreso @Codigo, NULL, @Validador, NULL;
    INSERT INTO @Resultados VALUES (N'Impide doble ingreso', 'FALLA', N'Se permitió reingresar');
END TRY
BEGIN CATCH
    INSERT INTO @Resultados VALUES (N'Impide doble ingreso', CASE WHEN ERROR_NUMBER() = 50043 THEN 'OK' ELSE 'FALLA' END, ERROR_MESSAGE());
END CATCH
UPDATE dbo.Entradas SET Estado = 'Valida', FechaHoraIngreso = NULL, ValidadorPuertaUsuarioId = NULL, PuertaAcceso = @PuertaOriginal
WHERE EntradaId = @EntradaPrueba;

-- HU-18 · Entradas emitidas = unidades pagadas
INSERT INTO @Resultados
SELECT N'Entradas emitidas = unidades pagadas', CASE WHEN COUNT(*) = 0 THEN 'OK' ELSE 'FALLA' END, CONCAT(COUNT(*), ' pedidos inconsistentes')
FROM dbo.Pedidos p
WHERE p.Estado = 'Pagado'
  AND (SELECT COUNT(*) FROM dbo.Entradas e WHERE e.PedidoId = p.PedidoId) <> (SELECT SUM(d.Cantidad) FROM dbo.PedidoDetalles d WHERE d.PedidoId = p.PedidoId);

SELECT Modulo = 'M4', Prueba, Resultado, Detalle FROM @Resultados;
GO

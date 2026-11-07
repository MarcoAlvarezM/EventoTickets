/* =============================================================================
   M3 · Pruebas de base de datos (HU-12, HU-13, HU-14)
   Ejecutar después de instalar toda la base (00_EJECUTAR_TODO.sql).
   Las pruebas de éxito corren en una transacción que se revierte; las que esperan
   un error se ejecutan fuera de transacción (los SP revierten su propio trabajo).
   Nota: sp_Pedido_Crear vence primero las reservas expiradas (regla de negocio).
   Resultado esperado: todas las filas en 'OK'.
   ============================================================================= */
USE EventCR_BoleteriaPro_DB;
GO
SET NOCOUNT ON;
DECLARE @Resultados TABLE (Prueba NVARCHAR(200), Resultado VARCHAR(10), Detalle NVARCHAR(400));
DECLARE @Id TABLE (Valor INT);

-- HU-12 / HU-14 · Crear → actualizar método de pago → cancelar (libera el cupo una sola vez)
BEGIN TRY
    BEGIN TRANSACTION;
    DECLARE @Antes INT = (SELECT CantidadComprometida FROM dbo.TiposEntrada WHERE TipoEntradaId = 5);
    INSERT INTO @Id EXEC dbo.sp_Pedido_Crear 6, 2, 'SINPE Movil', N'[{"TipoEntradaId":5,"Cantidad":2}]', 10, 13, 30, 10;
    DECLARE @P INT = (SELECT MAX(Valor) FROM @Id); DELETE FROM @Id;
    DECLARE @Durante INT = (SELECT CantidadComprometida FROM dbo.TiposEntrada WHERE TipoEntradaId = 5);
    INSERT INTO @Resultados
    SELECT N'Crear pedido compromete cupo y congela precios',
           CASE WHEN @Durante = @Antes + 2 AND p.Estado = 'PendientePago' AND p.Subtotal = 60000
                     AND p.ImporteTotal = p.Subtotal + p.CargoServicio + p.ImpuestoIVA
                     AND p.FechaVencimientoReserva > GETDATE()
                     AND EXISTS (SELECT 1 FROM dbo.PedidoDetalles d WHERE d.PedidoId = @P AND d.PrecioUnitario = 30000 AND d.Cantidad = 2)
                THEN 'OK' ELSE 'FALLA' END,
           CONCAT('Comprometidas ', @Antes, ' -> ', @Durante, ' · Total ', p.ImporteTotal)
    FROM dbo.Pedidos p WHERE p.PedidoId = @P;

    INSERT INTO @Id EXEC dbo.sp_Pedido_ActualizarMetodoPago @P, 6, 'Transferencia Bancaria'; DELETE FROM @Id;
    INSERT INTO @Resultados
    SELECT N'Actualizar método de pago (PendientePago)', CASE WHEN MetodoPagoSeleccionado = 'Transferencia Bancaria' THEN 'OK' ELSE 'FALLA' END, MetodoPagoSeleccionado
    FROM dbo.Pedidos WHERE PedidoId = @P;

    INSERT INTO @Id EXEC dbo.sp_Pedido_Cancelar @P, 6; DELETE FROM @Id;
    DECLARE @Despues INT = (SELECT CantidadComprometida FROM dbo.TiposEntrada WHERE TipoEntradaId = 5);
    INSERT INTO @Resultados
    SELECT N'Cancelar pedido libera el cupo (registro conservado)',
           CASE WHEN Estado = 'Cancelado' AND @Despues = @Antes THEN 'OK' ELSE 'FALLA' END, CONCAT(Estado, ' · comprometidas ', @Despues)
    FROM dbo.Pedidos WHERE PedidoId = @P;
    ROLLBACK TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    INSERT INTO @Resultados VALUES (N'Ciclo de vida del pedido', 'FALLA', ERROR_MESSAGE());
END CATCH

-- HU-14 · Vencimiento: libera el cupo exactamente una vez
BEGIN TRY
    BEGIN TRANSACTION;
    DECLARE @Base INT = (SELECT CantidadComprometida FROM dbo.TiposEntrada WHERE TipoEntradaId = 9);
    INSERT INTO @Id EXEC dbo.sp_Pedido_Crear 7, 4, 'SINPE Movil', N'[{"TipoEntradaId":9,"Cantidad":3}]', 10, 13, 30, 10;
    DECLARE @PV INT = (SELECT MAX(Valor) FROM @Id); DELETE FROM @Id;
    UPDATE dbo.Pedidos SET FechaVencimientoReserva = DATEADD(MINUTE, -1, GETDATE()) WHERE PedidoId = @PV;
    INSERT INTO @Id EXEC dbo.sp_Pedido_LiberarVencidos; DELETE FROM @Id;
    DECLARE @Tras1 INT = (SELECT CantidadComprometida FROM dbo.TiposEntrada WHERE TipoEntradaId = 9);
    INSERT INTO @Id EXEC dbo.sp_Pedido_LiberarVencidos; DELETE FROM @Id;
    DECLARE @Tras2 INT = (SELECT CantidadComprometida FROM dbo.TiposEntrada WHERE TipoEntradaId = 9);
    INSERT INTO @Resultados
    SELECT N'Vencimiento libera el cupo una sola vez',
           CASE WHEN Estado = 'Vencido' AND @Tras1 = @Base AND @Tras2 = @Base THEN 'OK' ELSE 'FALLA' END,
           CONCAT(Estado, ' · ', @Base, '/', @Tras1, '/', @Tras2)
    FROM dbo.Pedidos WHERE PedidoId = @PV;
    ROLLBACK TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    INSERT INTO @Resultados VALUES (N'Vencimiento libera el cupo una sola vez', 'FALLA', ERROR_MESSAGE());
END CATCH

-- HU-12 · Sobreventa (UPDATE condicional con UPDLOCK)
BEGIN TRY
    DECLARE @Disp INT = dbo.fn_DisponibilidadTipoEntrada(1);
    DECLARE @J NVARCHAR(100) = CONCAT(N'[{"TipoEntradaId":1,"Cantidad":', @Disp + 1, N'}]');
    EXEC dbo.sp_Pedido_Crear 6, 1, 'SINPE Movil', @J, 10, 13, 30, 100000;
    INSERT INTO @Resultados VALUES (N'Bloquea sobreventa', 'FALLA', N'Se permitió vender más de lo disponible');
END TRY
BEGIN CATCH
    INSERT INTO @Resultados VALUES (N'Bloquea sobreventa', CASE WHEN ERROR_NUMBER() = 50409 THEN 'OK' ELSE 'FALLA' END, ERROR_MESSAGE());
END CATCH

-- HU-12 · Límite de entradas por pedido
BEGIN TRY
    EXEC dbo.sp_Pedido_Crear 6, 2, 'SINPE Movil', N'[{"TipoEntradaId":5,"Cantidad":11}]', 10, 13, 30, 10;
    INSERT INTO @Resultados VALUES (N'Límite por pedido', 'FALLA', N'Se aceptaron 11 entradas');
END TRY
BEGIN CATCH
    INSERT INTO @Resultados VALUES (N'Límite por pedido', CASE WHEN ERROR_NUMBER() = 50023 THEN 'OK' ELSE 'FALLA' END, ERROR_MESSAGE());
END CATCH

-- HU-12 · Tipo de entrada de otro evento
BEGIN TRY
    EXEC dbo.sp_Pedido_Crear 6, 2, 'SINPE Movil', N'[{"TipoEntradaId":1,"Cantidad":1}]', 10, 13, 30, 10;
    INSERT INTO @Resultados VALUES (N'Tipo de entrada de otro evento', 'FALLA', N'Se aceptó');
END TRY
BEGIN CATCH
    INSERT INTO @Resultados VALUES (N'Tipo de entrada de otro evento', CASE WHEN ERROR_NUMBER() = 50024 THEN 'OK' ELSE 'FALLA' END, ERROR_MESSAGE());
END CATCH

-- HU-12 · Usuario bloqueado no compra
BEGIN TRY
    DECLARE @Bloqueado INT = (SELECT TOP (1) UsuarioId FROM dbo.Usuarios WHERE Estado = 'Bloqueado');
    EXEC dbo.sp_Pedido_Crear @Bloqueado, 2, 'SINPE Movil', N'[{"TipoEntradaId":5,"Cantidad":1}]', 10, 13, 30, 10;
    INSERT INTO @Resultados VALUES (N'Usuario bloqueado no compra', 'FALLA', N'Se aceptó');
END TRY
BEGIN CATCH
    INSERT INTO @Resultados VALUES (N'Usuario bloqueado no compra', CASE WHEN ERROR_NUMBER() = 50403 THEN 'OK' ELSE 'FALLA' END, ERROR_MESSAGE());
END CATCH

-- HU-12 · Evento no publicado
BEGIN TRY
    DECLARE @EvNoPub INT = (SELECT TOP (1) EventoId FROM dbo.Eventos WHERE Estado IN ('Borrador', 'Finalizado'));
    DECLARE @TipoNoPub INT = (SELECT TOP (1) TipoEntradaId FROM dbo.TiposEntrada WHERE EventoId = @EvNoPub);
    DECLARE @JNoPub NVARCHAR(100) = CONCAT(N'[{"TipoEntradaId":', @TipoNoPub, N',"Cantidad":1}]');
    EXEC dbo.sp_Pedido_Crear 6, @EvNoPub, 'SINPE Movil', @JNoPub, 10, 13, 30, 10;
    INSERT INTO @Resultados VALUES (N'Evento no disponible para la venta', 'FALLA', N'Se aceptó');
END TRY
BEGIN CATCH
    INSERT INTO @Resultados VALUES (N'Evento no disponible para la venta', CASE WHEN ERROR_NUMBER() = 50021 THEN 'OK' ELSE 'FALLA' END, ERROR_MESSAGE());
END CATCH

-- HU-14 · Un pedido pagado no se cancela ni cambia de método de pago
DECLARE @Pagado INT = (SELECT TOP (1) PedidoId FROM dbo.Pedidos WHERE Estado = 'Pagado');
BEGIN TRY
    EXEC dbo.sp_Pedido_Cancelar @Pagado, NULL;
    INSERT INTO @Resultados VALUES (N'Pedido pagado no se cancela', 'FALLA', N'Se canceló');
END TRY
BEGIN CATCH
    INSERT INTO @Resultados VALUES (N'Pedido pagado no se cancela', CASE WHEN ERROR_NUMBER() = 50030 THEN 'OK' ELSE 'FALLA' END, ERROR_MESSAGE());
END CATCH
BEGIN TRY
    EXEC dbo.sp_Pedido_ActualizarMetodoPago @Pagado, NULL, 'SINPE Movil';
    INSERT INTO @Resultados VALUES (N'Pedido pagado no cambia método de pago', 'FALLA', N'Se cambió');
END TRY
BEGIN CATCH
    INSERT INTO @Resultados VALUES (N'Pedido pagado no cambia método de pago', CASE WHEN ERROR_NUMBER() = 50035 THEN 'OK' ELSE 'FALLA' END, ERROR_MESSAGE());
END CATCH

-- HU-13 · Un comprador no cancela pedidos ajenos
-- (crea su propio pedido; el error esperado deja la transacción no confirmable y el CATCH la revierte)
BEGIN TRY
    BEGIN TRANSACTION;
    INSERT INTO @Id EXEC dbo.sp_Pedido_Crear 7, 2, 'SINPE Movil', N'[{"TipoEntradaId":5,"Cantidad":1}]', 10, 13, 30, 10;
    DECLARE @Ajeno INT = (SELECT MAX(Valor) FROM @Id); DELETE FROM @Id;
    EXEC dbo.sp_Pedido_Cancelar @Ajeno, 6;
    INSERT INTO @Resultados VALUES (N'No cancela pedidos ajenos', 'FALLA', N'Se canceló');
    ROLLBACK TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    INSERT INTO @Resultados VALUES (N'No cancela pedidos ajenos', CASE WHEN ERROR_NUMBER() = 50403 THEN 'OK' ELSE 'FALLA' END, ERROR_MESSAGE());
END CATCH
DELETE FROM @Id;

-- HU-13 · Los pedidos nunca se eliminan físicamente (ningún módulo programado borra Pedidos)
INSERT INTO @Resultados
SELECT N'Sin DELETE físico sobre Pedidos/PedidoDetalles', CASE WHEN COUNT(*) = 0 THEN 'OK' ELSE 'FALLA' END, STRING_AGG(OBJECT_NAME(m.object_id), ', ')
FROM sys.sql_modules AS m
WHERE m.[definition] LIKE '%DELETE FROM dbo.Pedidos%' OR m.[definition] LIKE '%DELETE FROM dbo.PedidoDetalles%';

-- HU-13 · Estados permitidos
INSERT INTO @Resultados
SELECT N'Estados de pedido válidos', CASE WHEN COUNT(*) = 0 THEN 'OK' ELSE 'FALLA' END, CONCAT(COUNT(*), ' fuera de dominio')
FROM dbo.Pedidos WHERE Estado NOT IN ('PendientePago', 'EnValidacion', 'Pagado', 'Cancelado', 'Vencido');

SELECT Modulo = 'M3', Prueba, Resultado, Detalle FROM @Resultados;
GO

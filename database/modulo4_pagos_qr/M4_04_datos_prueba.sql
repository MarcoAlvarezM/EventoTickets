/* =============================================================================
   M4 · Datos de prueba transaccionales (pedidos, pagos, entradas)
   Módulo: Módulo 4 · Pagos, auditoría, entradas QR y control de acceso (HU-15 a HU-19)
   Se generan llamando a los MISMOS procedimientos que usa la API (M3 y M4),
   para que inventario y estados queden coherentes.
   Depende de: M2_04_datos_iniciales.sql, procedimientos M3 y M4
   ============================================================================= */
USE EventCR_BoleteriaPro_DB;
GO
SET NOCOUNT ON;
GO

/* ---------- Transacciones de prueba (vía procedimientos) ---------- */
DECLARE @Cargo DECIMAL(5,2) = 10.00, @IVA DECIMAL(5,2) = 13.00, @Minutos INT = 30, @Max INT = 10;
DECLARE @P1 INT, @P2 INT, @P3 INT, @P4 INT, @Pago INT;
DECLARE @h1 VARCHAR(64) = LOWER(CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', 'seed-comprobante-1'), 2));
DECLARE @h2 VARCHAR(64) = LOWER(CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', 'seed-comprobante-2'), 2));
DECLARE @h3 VARCHAR(64) = LOWER(CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', 'seed-comprobante-3'), 2));
DECLARE @T TABLE (Id INT);

-- Pedido 1: Mariana, 2 VIP de la Cumbre -> pagado y con entradas emitidas
INSERT INTO @T EXEC dbo.sp_Pedido_Crear 5, 1, 'SINPE Movil', N'[{"TipoEntradaId":1,"Cantidad":2}]', @Cargo, @IVA, @Minutos, @Max;
SELECT @P1 = MAX(Id) FROM @T; DELETE FROM @T;
DECLARE @Total1 DECIMAL(12,2) = (SELECT ImporteTotal FROM dbo.Pedidos WHERE PedidoId = @P1);
INSERT INTO @T EXEC dbo.sp_Pago_Registrar @P1, 5, 'BAC Credomatic', 'SINPE-884920', @Total1, NULL, @h1, N'Transferencia realizada desde la app del banco.';
SELECT @Pago = MAX(Id) FROM @T; DELETE FROM @T;
INSERT INTO @T EXEC dbo.sp_Pago_Aprobar @Pago, 3, NULL; DELETE FROM @T;

-- Pedido 2: Carlos, 3 generales del Festival -> pago reportado, pendiente de validación
INSERT INTO @T EXEC dbo.sp_Pedido_Crear 6, 2, 'Transferencia Bancaria', N'[{"TipoEntradaId":5,"Cantidad":3}]', @Cargo, @IVA, @Minutos, @Max;
SELECT @P2 = MAX(Id) FROM @T; DELETE FROM @T;
DECLARE @Total2 DECIMAL(12,2) = (SELECT ImporteTotal FROM dbo.Pedidos WHERE PedidoId = @P2);
INSERT INTO @T EXEC dbo.sp_Pago_Registrar @P2, 6, 'Banco Nacional', 'TRANSF-449102', @Total2, NULL, @h2, N'Adjunto comprobante de la transferencia.';
DELETE FROM @T;

-- Pedido 3: Valeria, 1 platea + 1 balcón del Concierto -> pendiente de pago (reserva vigente)
INSERT INTO @T EXEC dbo.sp_Pedido_Crear 7, 3, 'SINPE Movil', N'[{"TipoEntradaId":6,"Cantidad":1},{"TipoEntradaId":7,"Cantidad":1}]', @Cargo, @IVA, 1440, @Max;
SELECT @P3 = MAX(Id) FROM @T; DELETE FROM @T;

-- Pedido 4: Mariana, 2 accesos corporativos del Foro -> pago rechazado (vuelve a pendiente)
INSERT INTO @T EXEC dbo.sp_Pedido_Crear 5, 4, 'Transferencia Bancaria', N'[{"TipoEntradaId":8,"Cantidad":2}]', @Cargo, @IVA, @Minutos, @Max;
SELECT @P4 = MAX(Id) FROM @T; DELETE FROM @T;
DECLARE @Total4 DECIMAL(12,2) = (SELECT ImporteTotal FROM dbo.Pedidos WHERE PedidoId = @P4);
INSERT INTO @T EXEC dbo.sp_Pago_Registrar @P4, 5, 'Banco de Costa Rica', 'DEP-00291884', @Total4, NULL, @h3, NULL;
SELECT @Pago = MAX(Id) FROM @T; DELETE FROM @T;
EXEC dbo.sp_Pago_Rechazar @Pago, 3, 'El comprobante no corresponde a la cuenta destino oficial.', 120;

-- Transferencia de una entrada del pedido 1 a un acompañante
DECLARE @E INT = (SELECT MAX(EntradaId) FROM dbo.Entradas WHERE PedidoId = @P1);
EXEC dbo.sp_Entrada_Transferir @E, 5, 'Carlos Andrés Méndez', '155812345678';
GO

PRINT N'Datos iniciales y de prueba cargados.';
GO

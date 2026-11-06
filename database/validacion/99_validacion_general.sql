/* =============================================================================
   99. Validación general (integridad de datos) · Responsable: integración
   Las pruebas de reglas por módulo están en moduloN/Mx_pruebas.sql.
   Resultado esperado: todas las filas de "Resultado" en 'OK'.
   ============================================================================= */
USE EventCR_BoleteriaPro_DB;
GO
SET NOCOUNT ON;
GO

/* ---------- 1. Inventario de objetos ---------- */
SELECT TipoObjeto = type_desc, Cantidad = COUNT(*)
FROM sys.objects
WHERE is_ms_shipped = 0 AND type IN ('U','V','P','FN','TR','F','PK','UQ','C','D')
GROUP BY type_desc ORDER BY type_desc;

/* ---------- 2. Registros por tabla ---------- */
SELECT 'Usuarios' AS Tabla, COUNT(*) AS Filas FROM dbo.Usuarios UNION ALL
SELECT 'Categorias', COUNT(*) FROM dbo.Categorias UNION ALL
SELECT 'Locaciones', COUNT(*) FROM dbo.Locaciones UNION ALL
SELECT 'Eventos', COUNT(*) FROM dbo.Eventos UNION ALL
SELECT 'TiposEntrada', COUNT(*) FROM dbo.TiposEntrada UNION ALL
SELECT 'Pedidos', COUNT(*) FROM dbo.Pedidos UNION ALL
SELECT 'PedidoDetalles', COUNT(*) FROM dbo.PedidoDetalles UNION ALL
SELECT 'Pagos', COUNT(*) FROM dbo.Pagos UNION ALL
SELECT 'Entradas', COUNT(*) FROM dbo.Entradas;

/* ---------- 3. Integridad de datos (deben devolver OK) ---------- */
DECLARE @Resultados TABLE (Prueba NVARCHAR(200), Resultado VARCHAR(10), Detalle NVARCHAR(400));

-- 3.1 Contador comprometido = suma de pedidos pendientes / en validación
INSERT INTO @Resultados
SELECT N'Comprometidas coinciden con pedidos abiertos',
       CASE WHEN COUNT(*) = 0 THEN 'OK' ELSE 'FALLA' END, CONCAT(COUNT(*), N' tipos con diferencias')
FROM dbo.TiposEntrada t
OUTER APPLY (SELECT ISNULL(SUM(d.Cantidad), 0) AS Cant FROM dbo.PedidoDetalles d
             INNER JOIN dbo.Pedidos p ON p.PedidoId = d.PedidoId
             WHERE d.TipoEntradaId = t.TipoEntradaId AND p.Estado IN ('PendientePago','EnValidacion')) x
WHERE t.CantidadComprometida <> x.Cant;

-- 3.2 Contador vendido = entradas emitidas no anuladas por cancelación
INSERT INTO @Resultados
SELECT N'Vendidas coinciden con pedidos pagados',
       CASE WHEN COUNT(*) = 0 THEN 'OK' ELSE 'FALLA' END, CONCAT(COUNT(*), N' tipos con diferencias')
FROM dbo.TiposEntrada t
OUTER APPLY (SELECT ISNULL(SUM(d.Cantidad), 0) AS Cant FROM dbo.PedidoDetalles d
             INNER JOIN dbo.Pedidos p ON p.PedidoId = d.PedidoId
             WHERE d.TipoEntradaId = t.TipoEntradaId AND p.Estado = 'Pagado') x
WHERE t.CantidadVendida <> x.Cant;

-- 3.3 Un pedido pagado tiene tantas entradas como unidades compradas
INSERT INTO @Resultados
SELECT N'Entradas emitidas = unidades de pedidos pagados',
       CASE WHEN COUNT(*) = 0 THEN 'OK' ELSE 'FALLA' END, CONCAT(COUNT(*), N' pedidos inconsistentes')
FROM dbo.Pedidos p
WHERE p.Estado = 'Pagado'
  AND (SELECT SUM(Cantidad) FROM dbo.PedidoDetalles WHERE PedidoId = p.PedidoId)
      <> (SELECT COUNT(*) FROM dbo.Entradas WHERE PedidoId = p.PedidoId);

-- 3.4 Totales de pedido = suma de sus líneas
INSERT INTO @Resultados
SELECT N'Subtotal del pedido = suma de líneas',
       CASE WHEN COUNT(*) = 0 THEN 'OK' ELSE 'FALLA' END, CONCAT(COUNT(*), N' pedidos inconsistentes')
FROM dbo.Pedidos p
WHERE p.Subtotal <> (SELECT SUM(SubtotalLinea) FROM dbo.PedidoDetalles WHERE PedidoId = p.PedidoId);

-- 3.5 Aforo asignado nunca supera la capacidad de la locación
INSERT INTO @Resultados
SELECT N'Aforo asignado <= capacidad de locación',
       CASE WHEN COUNT(*) = 0 THEN 'OK' ELSE 'FALLA' END, CONCAT(COUNT(*), N' eventos excedidos')
FROM dbo.Eventos e INNER JOIN dbo.Locaciones l ON l.LocacionId = e.LocacionId
WHERE dbo.fn_CapacidadAsignadaEvento(e.EventoId, NULL) > l.CapacidadMaxima;

-- 3.6 Ningún pago aprobado sobre un pedido no pagado
INSERT INTO @Resultados
SELECT N'Pagos aprobados corresponden a pedidos pagados',
       CASE WHEN COUNT(*) = 0 THEN 'OK' ELSE 'FALLA' END, CONCAT(COUNT(*), N' pagos inconsistentes')
FROM dbo.Pagos pg INNER JOIN dbo.Pedidos p ON p.PedidoId = pg.PedidoId
WHERE pg.Estado = 'Aprobado' AND p.Estado <> 'Pagado';

-- 3.7 Contraseñas nunca en texto plano
INSERT INTO @Resultados
SELECT N'Contraseñas almacenadas como hash PBKDF2',
       CASE WHEN COUNT(*) = 0 THEN 'OK' ELSE 'FALLA' END, CONCAT(COUNT(*), N' usuarios sin hash')
FROM dbo.Usuarios WHERE PasswordHash NOT LIKE 'PBKDF2-SHA256$%';

SELECT Prueba, Resultado, Detalle FROM @Resultados;
GO

/* ---------- 5. Consultas de ejemplo usadas por la API ---------- */
EXEC dbo.sp_Evento_Listar @SoloVigentes = 1, @Estado = 'Publicado';
EXEC dbo.sp_TipoEntrada_ListarPorEvento @EventoId = 1;
EXEC dbo.sp_Reporte_Dashboard;
EXEC dbo.sp_Reporte_VentasPorEvento;
GO

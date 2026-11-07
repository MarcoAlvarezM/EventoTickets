/* =============================================================================
   M2 · Pruebas de base de datos (HU-04, HU-05, HU-06, HU-09, HU-10, HU-11)
   Ejecutar después de instalar toda la base (00_EJECUTAR_TODO.sql).
   Resultado esperado: todas las filas en 'OK'. No deja cambios.
   ============================================================================= */
USE EventCR_BoleteriaPro_DB;
GO
SET NOCOUNT ON;
DECLARE @Resultados TABLE (Prueba NVARCHAR(200), Resultado VARCHAR(10), Detalle NVARCHAR(400));
DECLARE @Id TABLE (Valor INT);
DECLARE @Txt TABLE (Valor VARCHAR(20));
DECLARE @Manana DATE = DATEADD(DAY, 30, CAST(GETDATE() AS DATE));

-- HU-04 · Crear evento con código autogenerado, agregar tipo de entrada y publicar
BEGIN TRY
    BEGIN TRANSACTION;
    INSERT INTO @Id EXEC dbo.sp_Evento_Crear NULL, 1, 1, 'Evento Prueba M2', 'Corta', 'Descripción completa', 'Equipo', NULL, @Manana, '19:00', 'Borrador';
    DECLARE @E INT = (SELECT MAX(Valor) FROM @Id); DELETE FROM @Id;
    DECLARE @Codigo VARCHAR(50) = (SELECT CodigoReferencia FROM dbo.Eventos WHERE EventoId = @E);
    INSERT INTO @Id EXEC dbo.sp_TipoEntrada_Crear @E, 'General', 10000, '#0051D5', 5, 1;
    DECLARE @T INT = (SELECT MAX(Valor) FROM @Id); DELETE FROM @Id;
    EXEC dbo.sp_Evento_CambiarEstado @E, 'Publicado';
    INSERT INTO @Resultados
    SELECT N'Crear y publicar evento (código EVT-AAAA-00000)',
           CASE WHEN @Codigo = CONCAT('EVT-', YEAR(@Manana), '-', FORMAT(@E, '00000')) AND Estado = 'Publicado' THEN 'OK' ELSE 'FALLA' END,
           CONCAT(@Codigo, ' · ', Estado)
    FROM dbo.Eventos WHERE EventoId = @E;

    -- HU-05 · Editar conservando el código
    EXEC dbo.sp_Evento_Actualizar @E, NULL, 1, 1, 'Evento Prueba M2 Editado', 'Corta', 'Descripción completa', 'Equipo', NULL, @Manana, '20:00';
    INSERT INTO @Resultados
    SELECT N'Editar evento conserva el código', CASE WHEN CodigoReferencia = @Codigo AND Nombre = 'Evento Prueba M2 Editado' THEN 'OK' ELSE 'FALLA' END, CodigoReferencia
    FROM dbo.Eventos WHERE EventoId = @E;

    -- HU-11 · Agotado automático (trigger) cuando la disponibilidad llega a 0 y regreso a Publicado
    UPDATE dbo.TiposEntrada SET CantidadVendida = 5 WHERE TipoEntradaId = @T;
    DECLARE @EstadoAgotado VARCHAR(30) = (SELECT Estado FROM dbo.Eventos WHERE EventoId = @E);
    UPDATE dbo.TiposEntrada SET CantidadVendida = 4 WHERE TipoEntradaId = @T;
    DECLARE @EstadoRepuesto VARCHAR(30) = (SELECT Estado FROM dbo.Eventos WHERE EventoId = @E);
    INSERT INTO @Resultados VALUES (N'Agotado automático por disponibilidad', CASE WHEN @EstadoAgotado = 'Agotado' AND @EstadoRepuesto = 'Publicado' THEN 'OK' ELSE 'FALLA' END,
                                    CONCAT(@EstadoAgotado, ' -> ', @EstadoRepuesto));
    INSERT INTO @Resultados
    SELECT N'fn_DisponibilidadEvento', CASE WHEN dbo.fn_DisponibilidadEvento(@E) = 1 THEN 'OK' ELSE 'FALLA' END, CONCAT(dbo.fn_DisponibilidadEvento(@E), '');
    UPDATE dbo.TiposEntrada SET CantidadVendida = 0 WHERE TipoEntradaId = @T;

    -- HU-10 · Editar tipo de entrada y eliminarlo (sin ventas = físico)
    EXEC dbo.sp_TipoEntrada_Actualizar @T, 'General', 12000, '#0051D5', 8, 1;
    INSERT INTO @Resultados
    SELECT N'Editar tipo de entrada', CASE WHEN Precio = 12000 AND CantidadInicial = 8 THEN 'OK' ELSE 'FALLA' END, CONCAT(Precio, ' / ', CantidadInicial)
    FROM dbo.TiposEntrada WHERE TipoEntradaId = @T;

    -- HU-06 · La cartelera solo muestra Publicado/Agotado
    EXEC dbo.sp_Evento_CambiarEstado @E, 'Borrador';
    DECLARE @Cartelera TABLE (EventoId INT);
    INSERT INTO @Cartelera (EventoId)
    SELECT EventoId FROM dbo.vw_EventosCatalogo WHERE Estado IN ('Publicado', 'Agotado');
    INSERT INTO @Resultados VALUES (N'Borrador fuera de la cartelera', CASE WHEN NOT EXISTS (SELECT 1 FROM @Cartelera WHERE EventoId = @E) THEN 'OK' ELSE 'FALLA' END, NULL);

    -- HU-05 · Eliminar evento sin pedidos
    INSERT INTO @Txt EXEC dbo.sp_Evento_Eliminar @E;
    INSERT INTO @Resultados SELECT N'Eliminar evento sin pedidos', CASE WHEN Valor = 'Eliminado' THEN 'OK' ELSE 'FALLA' END, Valor FROM @Txt;
    ROLLBACK TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    INSERT INTO @Resultados VALUES (N'Ciclo de vida del evento', 'FALLA', ERROR_MESSAGE());
END CATCH
DELETE FROM @Txt;

-- HU-04 · Fecha pasada
BEGIN TRY
    EXEC dbo.sp_Evento_Crear NULL, 1, 1, 'Evento pasado', NULL, 'x', NULL, NULL, '2020-01-01', '19:00', 'Borrador';
    INSERT INTO @Resultados VALUES (N'Rechaza fecha pasada', 'FALLA', N'Se aceptó');
END TRY
BEGIN CATCH
    INSERT INTO @Resultados VALUES (N'Rechaza fecha pasada', CASE WHEN ERROR_NUMBER() = 50017 THEN 'OK' ELSE 'FALLA' END, ERROR_MESSAGE());
END CATCH

-- HU-04 · Categoría inactiva / inexistente
BEGIN TRY
    EXEC dbo.sp_Evento_Crear NULL, -1, 1, 'Sin categoría', NULL, 'x', NULL, NULL, @Manana, '19:00', 'Borrador';
    INSERT INTO @Resultados VALUES (N'Rechaza categoría inexistente', 'FALLA', N'Se aceptó');
END TRY
BEGIN CATCH
    INSERT INTO @Resultados VALUES (N'Rechaza categoría inexistente', CASE WHEN ERROR_NUMBER() = 50011 THEN 'OK' ELSE 'FALLA' END, ERROR_MESSAGE());
END CATCH

-- HU-05 · Publicar sin tipos de entrada
BEGIN TRY
    EXEC dbo.sp_Evento_Crear NULL, 1, 1, 'Publicado directo', NULL, 'x', NULL, NULL, @Manana, '19:00', 'Publicado';
    INSERT INTO @Resultados VALUES (N'No publica sin tipos de entrada', 'FALLA', N'Se aceptó');
END TRY
BEGIN CATCH
    INSERT INTO @Resultados VALUES (N'No publica sin tipos de entrada', CASE WHEN ERROR_NUMBER() = 50013 THEN 'OK' ELSE 'FALLA' END, ERROR_MESSAGE());
END CATCH

-- HU-05 · Evento con pedidos no se elimina físicamente
BEGIN TRY
    EXEC dbo.sp_Evento_Eliminar 1;
    INSERT INTO @Resultados VALUES (N'Evento con pedidos no se elimina', 'FALLA', N'Se eliminó');
END TRY
BEGIN CATCH
    INSERT INTO @Resultados VALUES (N'Evento con pedidos no se elimina', CASE WHEN ERROR_NUMBER() = 50409 THEN 'OK' ELSE 'FALLA' END, ERROR_MESSAGE());
END CATCH

-- HU-09 · Aforo: la suma de tipos no supera la capacidad de la locación
BEGIN TRY
    EXEC dbo.sp_TipoEntrada_Crear 4, 'Excede aforo', 1000, NULL, 100000, 1;
    INSERT INTO @Resultados VALUES (N'Aforo de tipos ≤ capacidad', 'FALLA', N'Se aceptó');
END TRY
BEGIN CATCH
    INSERT INTO @Resultados VALUES (N'Aforo de tipos ≤ capacidad', CASE WHEN ERROR_NUMBER() = 50015 THEN 'OK' ELSE 'FALLA' END, ERROR_MESSAGE());
END CATCH

-- HU-10 · Nombre de tipo duplicado en el mismo evento
BEGIN TRY
    EXEC dbo.sp_TipoEntrada_Crear 1, 'Preferencial', 1000, NULL, 1, 1;
    INSERT INTO @Resultados VALUES (N'Tipo de entrada duplicado', 'FALLA', N'Se aceptó');
END TRY
BEGIN CATCH
    INSERT INTO @Resultados VALUES (N'Tipo de entrada duplicado', CASE WHEN ERROR_NUMBER() = 50409 THEN 'OK' ELSE 'FALLA' END, ERROR_MESSAGE());
END CATCH

-- HU-11 · CHECK anti-sobreventa a nivel de tabla
BEGIN TRY
    UPDATE dbo.TiposEntrada SET CantidadVendida = CantidadInicial + 1 WHERE TipoEntradaId = 2;
    INSERT INTO @Resultados VALUES (N'CHECK CK_TiposEntrada_NoSobreventa', 'FALLA', N'Se aceptó');
END TRY
BEGIN CATCH
    INSERT INTO @Resultados VALUES (N'CHECK CK_TiposEntrada_NoSobreventa', CASE WHEN ERROR_NUMBER() = 547 THEN 'OK' ELSE 'FALLA' END, ERROR_MESSAGE());
END CATCH

-- HU-11 · Disponibilidad calculada = Inicial − Comprometida − Vendida
INSERT INTO @Resultados
SELECT N'Disponibilidad calculada', CASE WHEN COUNT(*) = 0 THEN 'OK' ELSE 'FALLA' END, CONCAT(COUNT(*), ' diferencias')
FROM dbo.TiposEntrada t
WHERE dbo.fn_DisponibilidadTipoEntrada(t.TipoEntradaId) <> t.CantidadInicial - t.CantidadComprometida - t.CantidadVendida;

SELECT Modulo = 'M2', Prueba, Resultado, Detalle FROM @Resultados;
GO

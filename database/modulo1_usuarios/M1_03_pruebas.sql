/* =============================================================================
   M1 · Pruebas de base de datos (HU-01, HU-02, HU-03, HU-07, HU-08)
   Ejecutar después de instalar toda la base (00_EJECUTAR_TODO.sql).
   Las pruebas que modifican datos corren dentro de una transacción que se revierte;
   las que esperan un error se ejecutan fuera de transacción (los SP no dejan cambios).
   Resultado esperado: todas las filas en 'OK'.
   ============================================================================= */
USE EventCR_BoleteriaPro_DB;
GO
SET NOCOUNT ON;
DECLARE @Resultados TABLE (Prueba NVARCHAR(200), Resultado VARCHAR(10), Detalle NVARCHAR(400));
DECLARE @Id TABLE (Valor INT);
DECLARE @Txt TABLE (Valor VARCHAR(20));
DECLARE @Hash VARCHAR(255) = 'PBKDF2-SHA256$100000$o/HCnntdTo+aCxwtPk9QYQ==$vsUzxa7rfGiibxm2Y7XA9cwZOvA4dBrZU3+4HlUkSJ8=';

-- HU-01 · Registro: crear, leer, actualizar y eliminar (lógico) un usuario
BEGIN TRY
    BEGIN TRANSACTION;
    INSERT INTO @Id EXEC dbo.sp_Usuario_Crear 'CEDULA', '9-9999-9999', N'Usuario Prueba M1', 'prueba.m1@correo.com', NULL, @Hash, 'Comprador', 'Activo';
    DECLARE @U INT = (SELECT MAX(Valor) FROM @Id);
    EXEC dbo.sp_Usuario_Actualizar @U, 'CEDULA', '9-9999-9999', N'Usuario Prueba M1 Editado', 'prueba.m1@correo.com', '+506 8000-0000', 'Comprador', 'Activo';
    EXEC dbo.sp_Usuario_CambiarEstado @U, 'Bloqueado';
    DECLARE @EstadoBloq VARCHAR(30) = (SELECT Estado FROM dbo.Usuarios WHERE UsuarioId = @U);
    INSERT INTO @Txt EXEC dbo.sp_Usuario_Eliminar @U;
    INSERT INTO @Resultados
    SELECT N'CRUD de usuario (crear, editar, bloquear, eliminar lógico)',
           CASE WHEN u.NombreCompleto = N'Usuario Prueba M1 Editado' AND @EstadoBloq = 'Bloqueado' AND u.Estado = 'Inactivo'
                     AND EXISTS (SELECT 1 FROM @Txt WHERE Valor = 'Desactivado') THEN 'OK' ELSE 'FALLA' END,
           CONCAT('Estado final: ', u.Estado)
    FROM dbo.Usuarios u WHERE u.UsuarioId = @U;
    ROLLBACK TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    INSERT INTO @Resultados VALUES (N'CRUD de usuario (crear, editar, bloquear, eliminar lógico)', 'FALLA', ERROR_MESSAGE());
END CATCH
DELETE FROM @Id; DELETE FROM @Txt;

-- HU-01 · Correo duplicado
BEGIN TRY
    EXEC dbo.sp_Usuario_Crear 'CEDULA', '9-8888-8888', N'Duplicado', 'admin@boleteriapro.cr', NULL, @Hash, 'Comprador', 'Activo';
    INSERT INTO @Resultados VALUES (N'Rechaza correo duplicado', 'FALLA', N'Se aceptó');
END TRY
BEGIN CATCH
    INSERT INTO @Resultados VALUES (N'Rechaza correo duplicado', CASE WHEN ERROR_NUMBER() = 50409 THEN 'OK' ELSE 'FALLA' END, ERROR_MESSAGE());
END CATCH

-- HU-01 · Documento duplicado
BEGIN TRY
    EXEC dbo.sp_Usuario_Crear 'CEDULA', '1-1111-1111', N'Duplicado', 'otro.correo@correo.com', NULL, @Hash, 'Comprador', 'Activo';
    INSERT INTO @Resultados VALUES (N'Rechaza documento duplicado', 'FALLA', N'Se aceptó');
END TRY
BEGIN CATCH
    INSERT INTO @Resultados VALUES (N'Rechaza documento duplicado', CASE WHEN ERROR_NUMBER() = 50409 THEN 'OK' ELSE 'FALLA' END, ERROR_MESSAGE());
END CATCH

-- HU-02 · Credenciales: devuelve el hash (nunca texto plano) y el estado para bloquear el acceso
INSERT INTO @Resultados
SELECT N'Credenciales con hash PBKDF2', CASE WHEN COUNT(*) = 0 THEN 'OK' ELSE 'FALLA' END, CONCAT(COUNT(*), ' usuarios sin hash')
FROM dbo.Usuarios WHERE PasswordHash NOT LIKE 'PBKDF2-SHA256$%';

-- HU-03 · Estado inválido rechazado por CHECK
BEGIN TRY
    EXEC dbo.sp_Usuario_CambiarEstado 1, 'Suspendido';
    INSERT INTO @Resultados VALUES (N'CHECK de estados de usuario', 'FALLA', N'Se aceptó un estado no definido');
END TRY
BEGIN CATCH
    INSERT INTO @Resultados VALUES (N'CHECK de estados de usuario', CASE WHEN ERROR_NUMBER() = 547 THEN 'OK' ELSE 'FALLA' END, ERROR_MESSAGE());
END CATCH

-- HU-03 · Usuario inexistente
BEGIN TRY
    EXEC dbo.sp_Usuario_Eliminar -1;
    INSERT INTO @Resultados VALUES (N'Eliminar usuario inexistente', 'FALLA', N'Se aceptó');
END TRY
BEGIN CATCH
    INSERT INTO @Resultados VALUES (N'Eliminar usuario inexistente', CASE WHEN ERROR_NUMBER() = 50404 THEN 'OK' ELSE 'FALLA' END, ERROR_MESSAGE());
END CATCH

-- HU-07 · CRUD de categoría sin eventos (eliminación física permitida)
BEGIN TRY
    BEGIN TRANSACTION;
    INSERT INTO @Id EXEC dbo.sp_Categoria_Crear 'Categoria Prueba M1', 'Temporal', 1;
    DECLARE @C INT = (SELECT MAX(Valor) FROM @Id);
    EXEC dbo.sp_Categoria_Actualizar @C, 'Categoria Prueba M1 Editada', 'Temporal', 1;
    INSERT INTO @Txt EXEC dbo.sp_Categoria_Eliminar @C;
    INSERT INTO @Resultados
    SELECT N'CRUD de categoría sin eventos', CASE WHEN Valor = 'Eliminada' AND NOT EXISTS (SELECT 1 FROM dbo.Categorias WHERE CategoriaId = @C) THEN 'OK' ELSE 'FALLA' END, Valor FROM @Txt;
    ROLLBACK TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    INSERT INTO @Resultados VALUES (N'CRUD de categoría sin eventos', 'FALLA', ERROR_MESSAGE());
END CATCH
DELETE FROM @Id; DELETE FROM @Txt;

-- HU-07 · Categoría con eventos: borrado lógico
BEGIN TRY
    BEGIN TRANSACTION;
    INSERT INTO @Txt EXEC dbo.sp_Categoria_Eliminar 1;
    INSERT INTO @Resultados
    SELECT N'Categoría con eventos se desactiva', CASE WHEN Valor = 'Desactivada' AND (SELECT Activo FROM dbo.Categorias WHERE CategoriaId = 1) = 0 THEN 'OK' ELSE 'FALLA' END, Valor FROM @Txt;
    ROLLBACK TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    INSERT INTO @Resultados VALUES (N'Categoría con eventos se desactiva', 'FALLA', ERROR_MESSAGE());
END CATCH
DELETE FROM @Txt;

-- HU-07 · Nombre de categoría duplicado
BEGIN TRY
    EXEC dbo.sp_Categoria_Crear 'Conciertos', NULL, 1;
    INSERT INTO @Resultados VALUES (N'Rechaza categoría duplicada', 'FALLA', N'Se aceptó');
END TRY
BEGIN CATCH
    INSERT INTO @Resultados VALUES (N'Rechaza categoría duplicada', CASE WHEN ERROR_NUMBER() = 50409 THEN 'OK' ELSE 'FALLA' END, ERROR_MESSAGE());
END CATCH

-- HU-08 · CRUD de locación sin eventos
BEGIN TRY
    BEGIN TRANSACTION;
    INSERT INTO @Id EXEC dbo.sp_Locacion_Crear 'Locacion Prueba M1', 'Heredia', 'Dirección de prueba', NULL, 500, 1;
    DECLARE @L INT = (SELECT MAX(Valor) FROM @Id);
    EXEC dbo.sp_Locacion_Actualizar @L, 'Locacion Prueba M1', 'Heredia', 'Dirección editada', 'Puerta 1', 600, 1;
    DECLARE @Cap INT = (SELECT CapacidadMaxima FROM dbo.Locaciones WHERE LocacionId = @L);
    INSERT INTO @Txt EXEC dbo.sp_Locacion_Eliminar @L;
    INSERT INTO @Resultados
    SELECT N'CRUD de locación sin eventos', CASE WHEN @Cap = 600 AND Valor = 'Eliminada' THEN 'OK' ELSE 'FALLA' END, Valor FROM @Txt;
    ROLLBACK TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    INSERT INTO @Resultados VALUES (N'CRUD de locación sin eventos', 'FALLA', ERROR_MESSAGE());
END CATCH
DELETE FROM @Id; DELETE FROM @Txt;

-- HU-08 · Locación con eventos: borrado lógico
BEGIN TRY
    BEGIN TRANSACTION;
    INSERT INTO @Txt EXEC dbo.sp_Locacion_Eliminar 1;
    INSERT INTO @Resultados
    SELECT N'Locación con eventos se desactiva', CASE WHEN Valor = 'Desactivada' THEN 'OK' ELSE 'FALLA' END, Valor FROM @Txt;
    ROLLBACK TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    INSERT INTO @Resultados VALUES (N'Locación con eventos se desactiva', 'FALLA', ERROR_MESSAGE());
END CATCH

-- HU-08 · No reducir la capacidad por debajo del aforo asignado
BEGIN TRY
    EXEC dbo.sp_Locacion_Actualizar 1, 'Centro de Convenciones de Costa Rica', 'Heredia', 'Cariari, Belén, Heredia', 'Pabellón Central · Puerta 1', 10, 1;
    INSERT INTO @Resultados VALUES (N'Capacidad ≥ aforo asignado', 'FALLA', N'Se aceptó');
END TRY
BEGIN CATCH
    INSERT INTO @Resultados VALUES (N'Capacidad ≥ aforo asignado', CASE WHEN ERROR_NUMBER() = 50010 THEN 'OK' ELSE 'FALLA' END, ERROR_MESSAGE());
END CATCH

SELECT Modulo = 'M1', Prueba, Resultado, Detalle FROM @Resultados;
GO

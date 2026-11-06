/* =============================================================================
   M1 · Procedimientos
   Módulo: Módulo 1 · Usuarios, autenticación, categorías y locaciones (HU-01, 02, 03, 07, 08)
   Objetos:
     sp_Usuario_Listar
     sp_Usuario_ObtenerPorId
     sp_Usuario_ObtenerCredenciales
     sp_Usuario_Crear
     sp_Usuario_Actualizar
     sp_Usuario_CambiarEstado
     sp_Usuario_CambiarPassword
     sp_Usuario_Eliminar
     sp_Categoria_Listar
     sp_Categoria_ObtenerPorId
     sp_Categoria_Crear
     sp_Categoria_Actualizar
     sp_Categoria_Eliminar
     sp_Locacion_Listar
     sp_Locacion_ObtenerPorId
     sp_Locacion_Crear
     sp_Locacion_Actualizar
     sp_Locacion_Eliminar
   Depende de: base/01..06
   ============================================================================= */
USE EventCR_BoleteriaPro_DB;
GO

CREATE OR ALTER PROCEDURE dbo.sp_Usuario_Listar
    @Busqueda VARCHAR(150) = NULL,
    @Rol      VARCHAR(40)  = NULL,
    @Estado   VARCHAR(30)  = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT UsuarioId, TipoDocumento, NumeroDocumento, NombreCompleto, CorreoElectronico,
           Telefono, Rol, Estado, FechaRegistro
    FROM dbo.Usuarios
    WHERE (@Busqueda IS NULL OR NombreCompleto LIKE '%' + @Busqueda + '%'
                             OR CorreoElectronico LIKE '%' + @Busqueda + '%'
                             OR NumeroDocumento LIKE '%' + @Busqueda + '%')
      AND (@Rol IS NULL OR Rol = @Rol)
      AND (@Estado IS NULL OR Estado = @Estado)
    ORDER BY NombreCompleto;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Usuario_ObtenerPorId
    @UsuarioId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT UsuarioId, TipoDocumento, NumeroDocumento, NombreCompleto, CorreoElectronico,
           Telefono, Rol, Estado, FechaRegistro
    FROM dbo.Usuarios
    WHERE UsuarioId = @UsuarioId;
END
GO

/* Solo para autenticación: devuelve el hash para verificarlo en la capa Reglas */
CREATE OR ALTER PROCEDURE dbo.sp_Usuario_ObtenerCredenciales
    @CorreoElectronico VARCHAR(150)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT UsuarioId, NombreCompleto, CorreoElectronico, PasswordHash, Rol, Estado
    FROM dbo.Usuarios
    WHERE CorreoElectronico = @CorreoElectronico;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Usuario_Crear
    @TipoDocumento     VARCHAR(10),
    @NumeroDocumento   VARCHAR(30),
    @NombreCompleto    VARCHAR(150),
    @CorreoElectronico VARCHAR(150),
    @Telefono          VARCHAR(30) = NULL,
    @PasswordHash      VARCHAR(255),
    @Rol               VARCHAR(40) = 'Comprador',
    @Estado            VARCHAR(30) = 'Activo'
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM dbo.Usuarios WHERE CorreoElectronico = @CorreoElectronico)
        THROW 50409, N'Ya existe un usuario registrado con ese correo electrónico.', 1;
    IF EXISTS (SELECT 1 FROM dbo.Usuarios WHERE NumeroDocumento = @NumeroDocumento)
        THROW 50409, N'Ya existe un usuario registrado con ese número de documento.', 1;

    INSERT INTO dbo.Usuarios (TipoDocumento, NumeroDocumento, NombreCompleto, CorreoElectronico,
                              Telefono, PasswordHash, Rol, Estado, FechaRegistro)
    VALUES (@TipoDocumento, @NumeroDocumento, @NombreCompleto, @CorreoElectronico,
            @Telefono, @PasswordHash, @Rol, @Estado, GETDATE());

    SELECT CAST(SCOPE_IDENTITY() AS INT) AS UsuarioId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Usuario_Actualizar
    @UsuarioId         INT,
    @TipoDocumento     VARCHAR(10),
    @NumeroDocumento   VARCHAR(30),
    @NombreCompleto    VARCHAR(150),
    @CorreoElectronico VARCHAR(150),
    @Telefono          VARCHAR(30) = NULL,
    @Rol               VARCHAR(40),
    @Estado            VARCHAR(30)
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM dbo.Usuarios WHERE UsuarioId = @UsuarioId)
        THROW 50404, N'El usuario no existe.', 1;
    IF EXISTS (SELECT 1 FROM dbo.Usuarios WHERE CorreoElectronico = @CorreoElectronico AND UsuarioId <> @UsuarioId)
        THROW 50409, N'El correo electrónico ya pertenece a otro usuario.', 1;
    IF EXISTS (SELECT 1 FROM dbo.Usuarios WHERE NumeroDocumento = @NumeroDocumento AND UsuarioId <> @UsuarioId)
        THROW 50409, N'El número de documento ya pertenece a otro usuario.', 1;

    UPDATE dbo.Usuarios
    SET TipoDocumento = @TipoDocumento, NumeroDocumento = @NumeroDocumento, NombreCompleto = @NombreCompleto,
        CorreoElectronico = @CorreoElectronico, Telefono = @Telefono, Rol = @Rol, Estado = @Estado
    WHERE UsuarioId = @UsuarioId;

    SELECT @@ROWCOUNT AS Filas;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Usuario_CambiarEstado
    @UsuarioId INT,
    @Estado    VARCHAR(30)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Usuarios SET Estado = @Estado WHERE UsuarioId = @UsuarioId;
    IF @@ROWCOUNT = 0
        THROW 50404, N'El usuario no existe.', 1;
    SELECT 1 AS Filas;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Usuario_CambiarPassword
    @UsuarioId    INT,
    @PasswordHash VARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Usuarios SET PasswordHash = @PasswordHash WHERE UsuarioId = @UsuarioId;
    IF @@ROWCOUNT = 0
        THROW 50404, N'El usuario no existe.', 1;
    SELECT 1 AS Filas;
END
GO

/* Borrado lógico: nunca se elimina físicamente para preservar el historial transaccional */
CREATE OR ALTER PROCEDURE dbo.sp_Usuario_Eliminar
    @UsuarioId INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Usuarios SET Estado = 'Inactivo' WHERE UsuarioId = @UsuarioId;
    IF @@ROWCOUNT = 0
        THROW 50404, N'El usuario no existe.', 1;
    SELECT 'Desactivado' AS Resultado;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Categoria_Listar
    @SoloActivas BIT = 0
AS
BEGIN
    SET NOCOUNT ON;
    SELECT c.CategoriaId, c.Nombre, c.Descripcion, c.Activo,
           CantidadEventos = (SELECT COUNT(*) FROM dbo.Eventos e WHERE e.CategoriaId = c.CategoriaId)
    FROM dbo.Categorias c
    WHERE (@SoloActivas = 0 OR c.Activo = 1)
    ORDER BY c.Nombre;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Categoria_ObtenerPorId
    @CategoriaId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT c.CategoriaId, c.Nombre, c.Descripcion, c.Activo,
           CantidadEventos = (SELECT COUNT(*) FROM dbo.Eventos e WHERE e.CategoriaId = c.CategoriaId)
    FROM dbo.Categorias c
    WHERE c.CategoriaId = @CategoriaId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Categoria_Crear
    @Nombre      VARCHAR(100),
    @Descripcion VARCHAR(255) = NULL,
    @Activo      BIT = 1
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM dbo.Categorias WHERE Nombre = @Nombre)
        THROW 50409, N'Ya existe una categoría con ese nombre.', 1;
    INSERT INTO dbo.Categorias (Nombre, Descripcion, Activo) VALUES (@Nombre, @Descripcion, @Activo);
    SELECT CAST(SCOPE_IDENTITY() AS INT) AS CategoriaId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Categoria_Actualizar
    @CategoriaId INT,
    @Nombre      VARCHAR(100),
    @Descripcion VARCHAR(255) = NULL,
    @Activo      BIT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM dbo.Categorias WHERE CategoriaId = @CategoriaId)
        THROW 50404, N'La categoría no existe.', 1;
    IF EXISTS (SELECT 1 FROM dbo.Categorias WHERE Nombre = @Nombre AND CategoriaId <> @CategoriaId)
        THROW 50409, N'Ya existe otra categoría con ese nombre.', 1;
    UPDATE dbo.Categorias SET Nombre = @Nombre, Descripcion = @Descripcion, Activo = @Activo
    WHERE CategoriaId = @CategoriaId;
    SELECT @@ROWCOUNT AS Filas;
END
GO

/* Elimina si no tiene eventos; si los tiene, la desactiva (borrado lógico) */
CREATE OR ALTER PROCEDURE dbo.sp_Categoria_Eliminar
    @CategoriaId INT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM dbo.Categorias WHERE CategoriaId = @CategoriaId)
        THROW 50404, N'La categoría no existe.', 1;
    IF EXISTS (SELECT 1 FROM dbo.Eventos WHERE CategoriaId = @CategoriaId)
    BEGIN
        UPDATE dbo.Categorias SET Activo = 0 WHERE CategoriaId = @CategoriaId;
        SELECT 'Desactivada' AS Resultado;
        RETURN;
    END
    DELETE FROM dbo.Categorias WHERE CategoriaId = @CategoriaId;
    SELECT 'Eliminada' AS Resultado;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Locacion_Listar
    @SoloActivas BIT = 0,
    @Busqueda    VARCHAR(150) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT l.LocacionId, l.Nombre, l.Ciudad, l.Direccion, l.PabellonPuerta, l.CapacidadMaxima, l.Activo,
           CantidadEventos = (SELECT COUNT(*) FROM dbo.Eventos e WHERE e.LocacionId = l.LocacionId)
    FROM dbo.Locaciones l
    WHERE (@SoloActivas = 0 OR l.Activo = 1)
      AND (@Busqueda IS NULL OR l.Nombre LIKE '%' + @Busqueda + '%' OR l.Ciudad LIKE '%' + @Busqueda + '%')
    ORDER BY l.Nombre;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Locacion_ObtenerPorId
    @LocacionId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT l.LocacionId, l.Nombre, l.Ciudad, l.Direccion, l.PabellonPuerta, l.CapacidadMaxima, l.Activo,
           CantidadEventos = (SELECT COUNT(*) FROM dbo.Eventos e WHERE e.LocacionId = l.LocacionId)
    FROM dbo.Locaciones l
    WHERE l.LocacionId = @LocacionId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Locacion_Crear
    @Nombre          VARCHAR(150),
    @Ciudad          VARCHAR(100),
    @Direccion       VARCHAR(255),
    @PabellonPuerta  VARCHAR(100) = NULL,
    @CapacidadMaxima INT,
    @Activo          BIT = 1
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.Locaciones (Nombre, Ciudad, Direccion, PabellonPuerta, CapacidadMaxima, Activo)
    VALUES (@Nombre, @Ciudad, @Direccion, @PabellonPuerta, @CapacidadMaxima, @Activo);
    SELECT CAST(SCOPE_IDENTITY() AS INT) AS LocacionId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Locacion_Actualizar
    @LocacionId      INT,
    @Nombre          VARCHAR(150),
    @Ciudad          VARCHAR(100),
    @Direccion       VARCHAR(255),
    @PabellonPuerta  VARCHAR(100) = NULL,
    @CapacidadMaxima INT,
    @Activo          BIT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM dbo.Locaciones WHERE LocacionId = @LocacionId)
        THROW 50404, N'La locación no existe.', 1;

    DECLARE @MaxAsignada INT =
        (SELECT MAX(dbo.fn_CapacidadAsignadaEvento(e.EventoId, NULL)) FROM dbo.Eventos e WHERE e.LocacionId = @LocacionId);
    IF @CapacidadMaxima < ISNULL(@MaxAsignada, 0)
        THROW 50010, N'La capacidad máxima no puede ser menor que el aforo ya asignado a un evento de esta locación.', 1;

    UPDATE dbo.Locaciones
    SET Nombre = @Nombre, Ciudad = @Ciudad, Direccion = @Direccion, PabellonPuerta = @PabellonPuerta,
        CapacidadMaxima = @CapacidadMaxima, Activo = @Activo
    WHERE LocacionId = @LocacionId;
    SELECT @@ROWCOUNT AS Filas;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Locacion_Eliminar
    @LocacionId INT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM dbo.Locaciones WHERE LocacionId = @LocacionId)
        THROW 50404, N'La locación no existe.', 1;
    IF EXISTS (SELECT 1 FROM dbo.Eventos WHERE LocacionId = @LocacionId)
    BEGIN
        UPDATE dbo.Locaciones SET Activo = 0 WHERE LocacionId = @LocacionId;
        SELECT 'Desactivada' AS Resultado;
        RETURN;
    END
    DELETE FROM dbo.Locaciones WHERE LocacionId = @LocacionId;
    SELECT 'Eliminada' AS Resultado;
END
GO

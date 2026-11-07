/* EventCR_BoleteriaPro_DB — instalación completa en un solo archivo (generado desde orden_ejecucion.txt).
   No editar a mano: cada módulo mantiene sus scripts en su carpeta. Regenerar tras cambiar un script. */

/* >>>>>>>>>>>>>>>>>>>> base/01_crear_base_datos.sql <<<<<<<<<<<<<<<<<<<< */
/* =============================================================================
   Estructura general de la base de datos · Responsable: Módulo 2 (Donnkan Cervantes Peraza)
   EventoTickets / BoleteríaPro — 01. Creación de la base de datos
   Motor: Microsoft SQL Server 2019+ (compatible con SQL Server 2022 / Express)
   Ejecutar conectado a la instancia (base master).
   ============================================================================= */
SET NOCOUNT ON;
GO

IF DB_ID(N'EventCR_BoleteriaPro_DB') IS NULL
BEGIN
    CREATE DATABASE EventCR_BoleteriaPro_DB COLLATE Modern_Spanish_CI_AS;
END
GO

ALTER DATABASE EventCR_BoleteriaPro_DB SET READ_COMMITTED_SNAPSHOT ON WITH ROLLBACK IMMEDIATE;
GO

USE EventCR_BoleteriaPro_DB;
GO

PRINT N'Base de datos EventCR_BoleteriaPro_DB lista.';
GO


/* >>>>>>>>>>>>>>>>>>>> base/02_tablas.sql <<<<<<<<<<<<<<<<<<<< */
/* =============================================================================
   Estructura general de la base de datos · Responsable: Módulo 2 (Donnkan Cervantes Peraza)
   02. Creación de tablas (columnas, tipos, NULL/NOT NULL y claves primarias)
   Fuente: diagrama ER (diagramaER.pdf). 9 entidades, 3FN.
   Mapeo de tipos del diagrama a SQL Server:
     boolean -> BIT · text -> VARCHAR(MAX) · datetime -> DATETIME
   ADVERTENCIA: el script elimina las tablas si ya existen (reinstalación limpia).
   ============================================================================= */
USE EventCR_BoleteriaPro_DB;
GO
SET NOCOUNT ON;
GO

-- Orden inverso de dependencias para poder reinstalar
DROP TABLE IF EXISTS dbo.Entradas;
DROP TABLE IF EXISTS dbo.Pagos;
DROP TABLE IF EXISTS dbo.PedidoDetalles;
DROP TABLE IF EXISTS dbo.Pedidos;
DROP TABLE IF EXISTS dbo.TiposEntrada;
DROP TABLE IF EXISTS dbo.Eventos;
DROP TABLE IF EXISTS dbo.Locaciones;
DROP TABLE IF EXISTS dbo.Categorias;
DROP TABLE IF EXISTS dbo.Usuarios;
GO

CREATE TABLE dbo.Usuarios
(
    UsuarioId           INT IDENTITY(1,1) NOT NULL,
    TipoDocumento       VARCHAR(10)  NOT NULL,
    NumeroDocumento     VARCHAR(30)  NOT NULL,
    NombreCompleto      VARCHAR(150) NOT NULL,
    CorreoElectronico   VARCHAR(150) NOT NULL,
    Telefono            VARCHAR(30)  NULL,
    PasswordHash        VARCHAR(255) NOT NULL,
    Rol                 VARCHAR(40)  NOT NULL,
    Estado              VARCHAR(30)  NOT NULL,
    FechaRegistro       DATETIME     NOT NULL,
    CONSTRAINT PK_Usuarios PRIMARY KEY CLUSTERED (UsuarioId)
);
GO

CREATE TABLE dbo.Categorias
(
    CategoriaId         INT IDENTITY(1,1) NOT NULL,
    Nombre              VARCHAR(100) NOT NULL,
    Descripcion         VARCHAR(255) NULL,
    Activo              BIT          NOT NULL,
    CONSTRAINT PK_Categorias PRIMARY KEY CLUSTERED (CategoriaId)
);
GO

CREATE TABLE dbo.Locaciones
(
    LocacionId          INT IDENTITY(1,1) NOT NULL,
    Nombre              VARCHAR(150) NOT NULL,
    Ciudad              VARCHAR(100) NOT NULL,
    Direccion           VARCHAR(255) NOT NULL,
    PabellonPuerta      VARCHAR(100) NULL,
    CapacidadMaxima     INT          NOT NULL,
    Activo              BIT          NOT NULL,
    CONSTRAINT PK_Locaciones PRIMARY KEY CLUSTERED (LocacionId)
);
GO

CREATE TABLE dbo.Eventos
(
    EventoId            INT IDENTITY(1,1) NOT NULL,
    CodigoReferencia    VARCHAR(50)  NOT NULL,
    CategoriaId         INT          NOT NULL,
    LocacionId          INT          NOT NULL,
    Nombre              VARCHAR(150) NOT NULL,
    DescripcionCorta    VARCHAR(300) NULL,
    DescripcionCompleta VARCHAR(MAX) NOT NULL,
    Organizador         VARCHAR(150) NULL,
    ImagenUrl           VARCHAR(500) NULL,
    FechaEvento         DATE         NOT NULL,
    HoraEvento          TIME(0)      NOT NULL,
    Estado              VARCHAR(30)  NOT NULL,
    FechaCreacion       DATETIME     NOT NULL,
    CONSTRAINT PK_Eventos PRIMARY KEY CLUSTERED (EventoId)
);
GO

CREATE TABLE dbo.TiposEntrada
(
    TipoEntradaId        INT IDENTITY(1,1) NOT NULL,
    EventoId             INT           NOT NULL,
    Nombre               VARCHAR(80)   NOT NULL,
    Precio               DECIMAL(12,2) NOT NULL,
    ColorHex             VARCHAR(10)   NULL,
    CantidadInicial      INT           NOT NULL,
    CantidadComprometida INT           NOT NULL,
    CantidadVendida      INT           NOT NULL,
    Activo               BIT           NOT NULL,
    CONSTRAINT PK_TiposEntrada PRIMARY KEY CLUSTERED (TipoEntradaId)
);
GO

CREATE TABLE dbo.Pedidos
(
    PedidoId                INT IDENTITY(1,1) NOT NULL,
    CodigoPedido            VARCHAR(30)   NOT NULL,
    UsuarioId               INT           NOT NULL,
    EventoId                INT           NOT NULL,
    MetodoPagoSeleccionado  VARCHAR(60)   NOT NULL,
    ReferenciaBancariaUnica VARCHAR(50)   NOT NULL,
    Subtotal                DECIMAL(12,2) NOT NULL,
    CargoServicio           DECIMAL(12,2) NOT NULL,
    ImpuestoIVA             DECIMAL(12,2) NOT NULL,
    ImporteTotal            DECIMAL(12,2) NOT NULL,
    FechaCreacion           DATETIME      NOT NULL,
    FechaVencimientoReserva DATETIME      NOT NULL,
    Estado                  VARCHAR(30)   NOT NULL,
    CONSTRAINT PK_Pedidos PRIMARY KEY CLUSTERED (PedidoId)
);
GO

CREATE TABLE dbo.PedidoDetalles
(
    PedidoDetalleId     INT IDENTITY(1,1) NOT NULL,
    PedidoId            INT           NOT NULL,
    TipoEntradaId       INT           NOT NULL,
    Cantidad            INT           NOT NULL,
    PrecioUnitario      DECIMAL(12,2) NOT NULL,
    SubtotalLinea       DECIMAL(12,2) NOT NULL,
    CONSTRAINT PK_PedidoDetalles PRIMARY KEY CLUSTERED (PedidoDetalleId)
);
GO

CREATE TABLE dbo.Pagos
(
    PagoId                INT IDENTITY(1,1) NOT NULL,
    CodigoPago            VARCHAR(30)   NOT NULL,
    PedidoId              INT           NOT NULL,
    AuditorUsuarioId      INT           NULL,
    BancoOrigen           VARCHAR(80)   NOT NULL,
    ReferenciaComprobante VARCHAR(100)  NOT NULL,
    CodigoAprobacion      VARCHAR(50)   NULL,
    MontoReportado        DECIMAL(12,2) NOT NULL,
    ComprobanteUrl        VARCHAR(500)  NULL,
    HashComprobante       VARCHAR(64)   NOT NULL,
    NotaComprador         VARCHAR(255)  NULL,
    Estado                VARCHAR(30)   NOT NULL,
    MotivoRechazo         VARCHAR(255)  NULL,
    FechaReporte          DATETIME      NOT NULL,
    FechaValidacion       DATETIME      NULL,
    CONSTRAINT PK_Pagos PRIMARY KEY CLUSTERED (PagoId)
);
GO

CREATE TABLE dbo.Entradas
(
    EntradaId                 INT IDENTITY(1,1) NOT NULL,
    CodigoTicket              VARCHAR(50)  NOT NULL,
    PedidoId                  INT          NOT NULL,
    EventoId                  INT          NOT NULL,
    TipoEntradaId             INT          NOT NULL,
    NombreAsistente           VARCHAR(150) NOT NULL,
    DocumentoAsistente        VARCHAR(40)  NOT NULL,
    AsientoAsignado           VARCHAR(50)  NULL,
    PuertaAcceso              VARCHAR(50)  NULL,
    HashCriptografico         VARCHAR(64)  NOT NULL,
    Estado                    VARCHAR(30)  NOT NULL,
    FechaEmision              DATETIME     NOT NULL,
    FechaHoraIngreso          DATETIME     NULL,
    ValidadorPuertaUsuarioId  INT          NULL,
    CONSTRAINT PK_Entradas PRIMARY KEY CLUSTERED (EntradaId)
);
GO

PRINT N'Tablas creadas.';
GO


/* >>>>>>>>>>>>>>>>>>>> base/03_llaves_foraneas.sql <<<<<<<<<<<<<<<<<<<< */
/* =============================================================================
   Estructura general de la base de datos · Responsable: Módulo 2 (Donnkan Cervantes Peraza)
   03. Claves foráneas (relaciones y cardinalidades del diagrama ER)
   Todas las relaciones son 1 : N. Se usa ON DELETE NO ACTION: los registros
   con historial (ventas, pagos, entradas) no se borran en cascada; los
   catálogos se desactivan (Activo = 0) desde los procedimientos almacenados.
   ============================================================================= */
USE EventCR_BoleteriaPro_DB;
GO

-- Categorias 1 : N Eventos
ALTER TABLE dbo.Eventos ADD CONSTRAINT FK_Eventos_Categorias
    FOREIGN KEY (CategoriaId) REFERENCES dbo.Categorias (CategoriaId);

-- Locaciones 1 : N Eventos
ALTER TABLE dbo.Eventos ADD CONSTRAINT FK_Eventos_Locaciones
    FOREIGN KEY (LocacionId) REFERENCES dbo.Locaciones (LocacionId);

-- Eventos 1 : N TiposEntrada
ALTER TABLE dbo.TiposEntrada ADD CONSTRAINT FK_TiposEntrada_Eventos
    FOREIGN KEY (EventoId) REFERENCES dbo.Eventos (EventoId);

-- Usuarios 1 : N Pedidos (comprador)
ALTER TABLE dbo.Pedidos ADD CONSTRAINT FK_Pedidos_Usuarios
    FOREIGN KEY (UsuarioId) REFERENCES dbo.Usuarios (UsuarioId);

-- Eventos 1 : N Pedidos
ALTER TABLE dbo.Pedidos ADD CONSTRAINT FK_Pedidos_Eventos
    FOREIGN KEY (EventoId) REFERENCES dbo.Eventos (EventoId);

-- Pedidos 1 : N PedidoDetalles
ALTER TABLE dbo.PedidoDetalles ADD CONSTRAINT FK_PedidoDetalles_Pedidos
    FOREIGN KEY (PedidoId) REFERENCES dbo.Pedidos (PedidoId);

-- TiposEntrada 1 : N PedidoDetalles
ALTER TABLE dbo.PedidoDetalles ADD CONSTRAINT FK_PedidoDetalles_TiposEntrada
    FOREIGN KEY (TipoEntradaId) REFERENCES dbo.TiposEntrada (TipoEntradaId);

-- Pedidos 1 : N Pagos
ALTER TABLE dbo.Pagos ADD CONSTRAINT FK_Pagos_Pedidos
    FOREIGN KEY (PedidoId) REFERENCES dbo.Pedidos (PedidoId);

-- Usuarios 1 : N Pagos (auditor que valida, opcional)
ALTER TABLE dbo.Pagos ADD CONSTRAINT FK_Pagos_UsuariosAuditor
    FOREIGN KEY (AuditorUsuarioId) REFERENCES dbo.Usuarios (UsuarioId);

-- Pedidos 1 : N Entradas
ALTER TABLE dbo.Entradas ADD CONSTRAINT FK_Entradas_Pedidos
    FOREIGN KEY (PedidoId) REFERENCES dbo.Pedidos (PedidoId);

-- Eventos 1 : N Entradas
ALTER TABLE dbo.Entradas ADD CONSTRAINT FK_Entradas_Eventos
    FOREIGN KEY (EventoId) REFERENCES dbo.Eventos (EventoId);

-- TiposEntrada 1 : N Entradas
ALTER TABLE dbo.Entradas ADD CONSTRAINT FK_Entradas_TiposEntrada
    FOREIGN KEY (TipoEntradaId) REFERENCES dbo.TiposEntrada (TipoEntradaId);

-- Usuarios 1 : N Entradas (validador de puerta, opcional)
ALTER TABLE dbo.Entradas ADD CONSTRAINT FK_Entradas_UsuariosValidador
    FOREIGN KEY (ValidadorPuertaUsuarioId) REFERENCES dbo.Usuarios (UsuarioId);
GO

PRINT N'Claves foráneas creadas (13 relaciones).';
GO


/* >>>>>>>>>>>>>>>>>>>> base/04_restricciones.sql <<<<<<<<<<<<<<<<<<<< */
/* =============================================================================
   Estructura general de la base de datos · Responsable: Módulo 2 (Donnkan Cervantes Peraza)
   04. Restricciones UNIQUE, CHECK y DEFAULT
   - UNIQUE: atributos marcados como únicos en el diagrama ER.
   - CHECK: dominios de valores (estados, roles) y reglas aritméticas.
   - DEFAULT: valores iniciales de estados, fechas y contadores.
   ============================================================================= */
USE EventCR_BoleteriaPro_DB;
GO

/* ---------- Usuarios ---------- */
ALTER TABLE dbo.Usuarios ADD CONSTRAINT UQ_Usuarios_NumeroDocumento   UNIQUE (NumeroDocumento);
ALTER TABLE dbo.Usuarios ADD CONSTRAINT UQ_Usuarios_CorreoElectronico UNIQUE (CorreoElectronico);
ALTER TABLE dbo.Usuarios ADD CONSTRAINT CK_Usuarios_TipoDocumento CHECK (TipoDocumento IN ('CEDULA','DIMEX','PASAPORTE'));
ALTER TABLE dbo.Usuarios ADD CONSTRAINT CK_Usuarios_Rol CHECK (Rol IN ('Superadministrador','AdministradorEventos','AuditorPagos','ValidadorAcceso','Comprador'));
ALTER TABLE dbo.Usuarios ADD CONSTRAINT CK_Usuarios_Estado CHECK (Estado IN ('Activo','Bloqueado','Inactivo'));  -- Inactivo = borrado lógico
ALTER TABLE dbo.Usuarios ADD CONSTRAINT CK_Usuarios_Correo CHECK (CorreoElectronico LIKE '%_@_%._%');
ALTER TABLE dbo.Usuarios ADD CONSTRAINT DF_Usuarios_Rol DEFAULT ('Comprador') FOR Rol;
ALTER TABLE dbo.Usuarios ADD CONSTRAINT DF_Usuarios_Estado DEFAULT ('Activo') FOR Estado;
ALTER TABLE dbo.Usuarios ADD CONSTRAINT DF_Usuarios_FechaRegistro DEFAULT (GETDATE()) FOR FechaRegistro;
GO

/* ---------- Categorias ---------- */
ALTER TABLE dbo.Categorias ADD CONSTRAINT UQ_Categorias_Nombre UNIQUE (Nombre);
ALTER TABLE dbo.Categorias ADD CONSTRAINT DF_Categorias_Activo DEFAULT (1) FOR Activo;
GO

/* ---------- Locaciones ---------- */
ALTER TABLE dbo.Locaciones ADD CONSTRAINT CK_Locaciones_Capacidad CHECK (CapacidadMaxima > 0);
ALTER TABLE dbo.Locaciones ADD CONSTRAINT DF_Locaciones_Activo DEFAULT (1) FOR Activo;
GO

/* ---------- Eventos ---------- */
ALTER TABLE dbo.Eventos ADD CONSTRAINT UQ_Eventos_CodigoReferencia UNIQUE (CodigoReferencia);
ALTER TABLE dbo.Eventos ADD CONSTRAINT CK_Eventos_Estado CHECK (Estado IN ('Borrador','Publicado','Agotado','Cancelado','Finalizado'));
ALTER TABLE dbo.Eventos ADD CONSTRAINT DF_Eventos_Estado DEFAULT ('Borrador') FOR Estado;
ALTER TABLE dbo.Eventos ADD CONSTRAINT DF_Eventos_FechaCreacion DEFAULT (GETDATE()) FOR FechaCreacion;
GO

/* ---------- TiposEntrada ---------- */
ALTER TABLE dbo.TiposEntrada ADD CONSTRAINT UQ_TiposEntrada_Evento_Nombre UNIQUE (EventoId, Nombre);
ALTER TABLE dbo.TiposEntrada ADD CONSTRAINT CK_TiposEntrada_Precio CHECK (Precio >= 0);
ALTER TABLE dbo.TiposEntrada ADD CONSTRAINT CK_TiposEntrada_CantidadInicial CHECK (CantidadInicial > 0);
ALTER TABLE dbo.TiposEntrada ADD CONSTRAINT CK_TiposEntrada_Contadores CHECK (CantidadComprometida >= 0 AND CantidadVendida >= 0);
-- Regla central anti-sobreventa: nunca se compromete/vende más que el inventario
ALTER TABLE dbo.TiposEntrada ADD CONSTRAINT CK_TiposEntrada_NoSobreventa CHECK (CantidadComprometida + CantidadVendida <= CantidadInicial);
ALTER TABLE dbo.TiposEntrada ADD CONSTRAINT CK_TiposEntrada_ColorHex CHECK (ColorHex IS NULL OR ColorHex LIKE '#[0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f]');
ALTER TABLE dbo.TiposEntrada ADD CONSTRAINT DF_TiposEntrada_Comprometida DEFAULT (0) FOR CantidadComprometida;
ALTER TABLE dbo.TiposEntrada ADD CONSTRAINT DF_TiposEntrada_Vendida DEFAULT (0) FOR CantidadVendida;
ALTER TABLE dbo.TiposEntrada ADD CONSTRAINT DF_TiposEntrada_Activo DEFAULT (1) FOR Activo;
GO

/* ---------- Pedidos ---------- */
ALTER TABLE dbo.Pedidos ADD CONSTRAINT UQ_Pedidos_CodigoPedido UNIQUE (CodigoPedido);
ALTER TABLE dbo.Pedidos ADD CONSTRAINT UQ_Pedidos_ReferenciaBancariaUnica UNIQUE (ReferenciaBancariaUnica);
ALTER TABLE dbo.Pedidos ADD CONSTRAINT CK_Pedidos_Estado CHECK (Estado IN ('PendientePago','EnValidacion','Pagado','Cancelado','Vencido'));
ALTER TABLE dbo.Pedidos ADD CONSTRAINT CK_Pedidos_Metodo CHECK (MetodoPagoSeleccionado IN ('SINPE Movil','Transferencia Bancaria'));
ALTER TABLE dbo.Pedidos ADD CONSTRAINT CK_Pedidos_Montos CHECK (Subtotal >= 0 AND CargoServicio >= 0 AND ImpuestoIVA >= 0);
ALTER TABLE dbo.Pedidos ADD CONSTRAINT CK_Pedidos_Total CHECK (ImporteTotal = Subtotal + CargoServicio + ImpuestoIVA);
ALTER TABLE dbo.Pedidos ADD CONSTRAINT CK_Pedidos_Vencimiento CHECK (FechaVencimientoReserva > FechaCreacion);
ALTER TABLE dbo.Pedidos ADD CONSTRAINT DF_Pedidos_Estado DEFAULT ('PendientePago') FOR Estado;
ALTER TABLE dbo.Pedidos ADD CONSTRAINT DF_Pedidos_FechaCreacion DEFAULT (GETDATE()) FOR FechaCreacion;
GO

/* ---------- PedidoDetalles ---------- */
ALTER TABLE dbo.PedidoDetalles ADD CONSTRAINT UQ_PedidoDetalles_Pedido_Tipo UNIQUE (PedidoId, TipoEntradaId);
ALTER TABLE dbo.PedidoDetalles ADD CONSTRAINT CK_PedidoDetalles_Cantidad CHECK (Cantidad > 0);
ALTER TABLE dbo.PedidoDetalles ADD CONSTRAINT CK_PedidoDetalles_Precio CHECK (PrecioUnitario >= 0);
ALTER TABLE dbo.PedidoDetalles ADD CONSTRAINT CK_PedidoDetalles_Subtotal CHECK (SubtotalLinea = Cantidad * PrecioUnitario);
GO

/* ---------- Pagos ---------- */
ALTER TABLE dbo.Pagos ADD CONSTRAINT UQ_Pagos_CodigoPago UNIQUE (CodigoPago);
ALTER TABLE dbo.Pagos ADD CONSTRAINT CK_Pagos_Monto CHECK (MontoReportado > 0);
ALTER TABLE dbo.Pagos ADD CONSTRAINT CK_Pagos_Estado CHECK (Estado IN ('PendienteValidacion','Aprobado','Rechazado'));
ALTER TABLE dbo.Pagos ADD CONSTRAINT CK_Pagos_Hash CHECK (LEN(HashComprobante) = 64);
ALTER TABLE dbo.Pagos ADD CONSTRAINT CK_Pagos_Rechazo CHECK (Estado <> 'Rechazado' OR MotivoRechazo IS NOT NULL);
ALTER TABLE dbo.Pagos ADD CONSTRAINT CK_Pagos_Validacion CHECK (Estado = 'PendienteValidacion' OR (AuditorUsuarioId IS NOT NULL AND FechaValidacion IS NOT NULL));
ALTER TABLE dbo.Pagos ADD CONSTRAINT DF_Pagos_Estado DEFAULT ('PendienteValidacion') FOR Estado;
ALTER TABLE dbo.Pagos ADD CONSTRAINT DF_Pagos_FechaReporte DEFAULT (GETDATE()) FOR FechaReporte;
GO

/* ---------- Entradas ---------- */
ALTER TABLE dbo.Entradas ADD CONSTRAINT UQ_Entradas_CodigoTicket UNIQUE (CodigoTicket);
ALTER TABLE dbo.Entradas ADD CONSTRAINT UQ_Entradas_HashCriptografico UNIQUE (HashCriptografico);
ALTER TABLE dbo.Entradas ADD CONSTRAINT CK_Entradas_Estado CHECK (Estado IN ('Valida','Utilizada','Anulada'));
ALTER TABLE dbo.Entradas ADD CONSTRAINT CK_Entradas_Ingreso CHECK (Estado <> 'Utilizada' OR (FechaHoraIngreso IS NOT NULL AND ValidadorPuertaUsuarioId IS NOT NULL));
ALTER TABLE dbo.Entradas ADD CONSTRAINT DF_Entradas_Estado DEFAULT ('Valida') FOR Estado;
ALTER TABLE dbo.Entradas ADD CONSTRAINT DF_Entradas_FechaEmision DEFAULT (GETDATE()) FOR FechaEmision;
GO

PRINT N'Restricciones UNIQUE / CHECK / DEFAULT creadas.';
GO


/* >>>>>>>>>>>>>>>>>>>> base/05_indices.sql <<<<<<<<<<<<<<<<<<<< */
/* =============================================================================
   Estructura general de la base de datos · Responsable: Módulo 2 (Donnkan Cervantes Peraza)
   05. Índices no agrupados
   Cubren claves foráneas y los filtros más usados por la API
   (catálogo público, bandeja de pagos, pedidos por usuario, entradas).
   ============================================================================= */
USE EventCR_BoleteriaPro_DB;
GO

CREATE INDEX IX_Usuarios_Rol_Estado           ON dbo.Usuarios (Rol, Estado);
CREATE INDEX IX_Eventos_Categoria             ON dbo.Eventos (CategoriaId);
CREATE INDEX IX_Eventos_Locacion              ON dbo.Eventos (LocacionId);
CREATE INDEX IX_Eventos_Estado_Fecha          ON dbo.Eventos (Estado, FechaEvento) INCLUDE (Nombre, CategoriaId, LocacionId);
CREATE INDEX IX_TiposEntrada_Evento           ON dbo.TiposEntrada (EventoId) INCLUDE (Precio, CantidadInicial, CantidadComprometida, CantidadVendida, Activo);
CREATE INDEX IX_Pedidos_Usuario_Estado        ON dbo.Pedidos (UsuarioId, Estado);
CREATE INDEX IX_Pedidos_Evento                ON dbo.Pedidos (EventoId);
CREATE INDEX IX_Pedidos_Estado_Vencimiento    ON dbo.Pedidos (Estado, FechaVencimientoReserva);
CREATE INDEX IX_PedidoDetalles_TipoEntrada    ON dbo.PedidoDetalles (TipoEntradaId);
CREATE INDEX IX_Pagos_Pedido                  ON dbo.Pagos (PedidoId);
CREATE INDEX IX_Pagos_Estado_Fecha            ON dbo.Pagos (Estado, FechaReporte);
CREATE INDEX IX_Pagos_HashComprobante         ON dbo.Pagos (HashComprobante);
CREATE INDEX IX_Pagos_Auditor                 ON dbo.Pagos (AuditorUsuarioId);
CREATE INDEX IX_Entradas_Pedido               ON dbo.Entradas (PedidoId);
CREATE INDEX IX_Entradas_Evento_Estado        ON dbo.Entradas (EventoId, Estado);
CREATE INDEX IX_Entradas_TipoEntrada          ON dbo.Entradas (TipoEntradaId);
CREATE INDEX IX_Entradas_Validador            ON dbo.Entradas (ValidadorPuertaUsuarioId);
GO

PRINT N'Índices creados.';
GO


/* >>>>>>>>>>>>>>>>>>>> base/06_funciones.sql <<<<<<<<<<<<<<<<<<<< */
/* =============================================================================
   Estructura general de la base de datos · Responsable: Módulo 2 (Donnkan Cervantes Peraza)
   06. Funciones escalares
   Se crean antes que las vistas porque éstas las utilizan.
   ============================================================================= */
USE EventCR_BoleteriaPro_DB;
GO

/* Entradas disponibles de un tipo: inicial - comprometidas (reservas vigentes) - vendidas */
CREATE OR ALTER FUNCTION dbo.fn_DisponibilidadTipoEntrada (@TipoEntradaId INT)
RETURNS INT
AS
BEGIN
    DECLARE @Disponible INT;
    SELECT @Disponible = CantidadInicial - CantidadComprometida - CantidadVendida
    FROM dbo.TiposEntrada
    WHERE TipoEntradaId = @TipoEntradaId;
    RETURN ISNULL(@Disponible, 0);
END
GO

/* Capacidad ya repartida entre los tipos de entrada de un evento.
   @ExcluirTipoEntradaId permite validar una edición sin contar el propio tipo. */
CREATE OR ALTER FUNCTION dbo.fn_CapacidadAsignadaEvento (@EventoId INT, @ExcluirTipoEntradaId INT = NULL)
RETURNS INT
AS
BEGIN
    DECLARE @Total INT;
    SELECT @Total = SUM(CantidadInicial)
    FROM dbo.TiposEntrada
    WHERE EventoId = @EventoId
      AND (@ExcluirTipoEntradaId IS NULL OR TipoEntradaId <> @ExcluirTipoEntradaId);
    RETURN ISNULL(@Total, 0);
END
GO

/* Entradas disponibles de todo el evento (tipos activos). Base del estado Agotado. */
CREATE OR ALTER FUNCTION dbo.fn_DisponibilidadEvento (@EventoId INT)
RETURNS INT
AS
BEGIN
    DECLARE @Total INT;
    SELECT @Total = SUM(CantidadInicial - CantidadComprometida - CantidadVendida)
    FROM dbo.TiposEntrada
    WHERE EventoId = @EventoId AND Activo = 1;
    RETURN ISNULL(@Total, 0);
END
GO

PRINT N'Funciones creadas.';
GO


/* >>>>>>>>>>>>>>>>>>>> modulo1_usuarios/M1_01_procedimientos.sql <<<<<<<<<<<<<<<<<<<< */
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


/* >>>>>>>>>>>>>>>>>>>> modulo2_eventos/M2_01_vistas.sql <<<<<<<<<<<<<<<<<<<< */
/* =============================================================================
   M2 · Vistas
   Módulo: Módulo 2 · Eventos, cartelera, tipos de entrada y disponibilidad (HU-04, 05, 06, 09, 10, 11)
   Objetos: vw_EventosCatalogo, vw_TiposEntradaDisponibilidad
   Depende de: base/01..06
   ============================================================================= */
USE EventCR_BoleteriaPro_DB;
GO

/* Catálogo de eventos con categoría, locación, precio mínimo y disponibilidad */
CREATE OR ALTER VIEW dbo.vw_EventosCatalogo
AS
SELECT
    e.EventoId, e.CodigoReferencia, e.Nombre, e.DescripcionCorta, e.DescripcionCompleta,
    e.Organizador, e.ImagenUrl, e.FechaEvento, e.HoraEvento, e.Estado, e.FechaCreacion,
    e.CategoriaId, c.Nombre AS CategoriaNombre,
    e.LocacionId, l.Nombre AS LocacionNombre, l.Ciudad, l.Direccion, l.PabellonPuerta, l.CapacidadMaxima,
    PrecioDesde          = (SELECT MIN(t.Precio) FROM dbo.TiposEntrada t WHERE t.EventoId = e.EventoId AND t.Activo = 1),
    EntradasDisponibles  = ISNULL((SELECT SUM(t.CantidadInicial - t.CantidadComprometida - t.CantidadVendida)
                                   FROM dbo.TiposEntrada t WHERE t.EventoId = e.EventoId AND t.Activo = 1), 0),
    EntradasVendidas     = ISNULL((SELECT SUM(t.CantidadVendida) FROM dbo.TiposEntrada t WHERE t.EventoId = e.EventoId), 0),
    CapacidadAsignada    = dbo.fn_CapacidadAsignadaEvento(e.EventoId, NULL)
FROM dbo.Eventos e
INNER JOIN dbo.Categorias c ON c.CategoriaId = e.CategoriaId
INNER JOIN dbo.Locaciones l ON l.LocacionId = e.LocacionId;
GO

/* Tipos de entrada con disponibilidad calculada */
CREATE OR ALTER VIEW dbo.vw_TiposEntradaDisponibilidad
AS
SELECT
    t.TipoEntradaId, t.EventoId, e.Nombre AS EventoNombre, t.Nombre, t.Precio, t.ColorHex,
    t.CantidadInicial, t.CantidadComprometida, t.CantidadVendida, t.Activo,
    Disponible        = t.CantidadInicial - t.CantidadComprometida - t.CantidadVendida,
    PorcentajeVendido = CAST(ROUND(100.0 * t.CantidadVendida / NULLIF(t.CantidadInicial, 0), 1) AS DECIMAL(5,1))
FROM dbo.TiposEntrada t
INNER JOIN dbo.Eventos e ON e.EventoId = t.EventoId;
GO


/* >>>>>>>>>>>>>>>>>>>> modulo2_eventos/M2_02_procedimientos.sql <<<<<<<<<<<<<<<<<<<< */
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


/* >>>>>>>>>>>>>>>>>>>> modulo2_eventos/M2_03_triggers.sql <<<<<<<<<<<<<<<<<<<< */
/* =============================================================================
   M2 · Triggers
   Módulo: Módulo 2 · Eventos, cartelera, tipos de entrada y disponibilidad (HU-04, 05, 06, 09, 10, 11)
   Objetos: trg_TiposEntrada_SincronizarAgotado
   Depende de: base/01..06
   ============================================================================= */
USE EventCR_BoleteriaPro_DB;
GO

/* Sincroniza el estado Agotado del evento con su inventario (HU-06 / HU-11):
   Publicado -> Agotado cuando no queda cupo; Agotado -> Publicado cuando se libera cupo
   (expiración o cancelación de reservas). Los demás estados no se tocan. */
CREATE OR ALTER TRIGGER dbo.trg_TiposEntrada_SincronizarAgotado
ON dbo.TiposEntrada
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Afectados TABLE (EventoId INT PRIMARY KEY);
    INSERT INTO @Afectados (EventoId)
    SELECT EventoId FROM inserted UNION SELECT EventoId FROM deleted;

    UPDATE e SET Estado = 'Agotado'
    FROM dbo.Eventos e INNER JOIN @Afectados a ON a.EventoId = e.EventoId
    WHERE e.Estado = 'Publicado' AND dbo.fn_DisponibilidadEvento(e.EventoId) = 0;

    UPDATE e SET Estado = 'Publicado'
    FROM dbo.Eventos e INNER JOIN @Afectados a ON a.EventoId = e.EventoId
    WHERE e.Estado = 'Agotado' AND dbo.fn_DisponibilidadEvento(e.EventoId) > 0;
END
GO


/* >>>>>>>>>>>>>>>>>>>> modulo3_pedidos/M3_01_vistas.sql <<<<<<<<<<<<<<<<<<<< */
/* =============================================================================
   M3 · Vistas
   Módulo: Módulo 3 · Pedidos, historial, cancelación y vencimiento de reservas (HU-12, 13, 14)
   Objetos: vw_PedidosResumen
   Depende de: base/01..06, modulo2 (vistas de eventos)
   ============================================================================= */
USE EventCR_BoleteriaPro_DB;
GO

/* Pedidos con comprador, evento y último estado de pago */
CREATE OR ALTER VIEW dbo.vw_PedidosResumen
AS
SELECT
    p.PedidoId, p.CodigoPedido, p.UsuarioId, p.EventoId, p.MetodoPagoSeleccionado, p.ReferenciaBancariaUnica,
    p.Subtotal, p.CargoServicio, p.ImpuestoIVA, p.ImporteTotal, p.FechaCreacion, p.FechaVencimientoReserva, p.Estado,
    u.NombreCompleto AS CompradorNombre, u.CorreoElectronico AS CompradorCorreo,
    u.TipoDocumento AS CompradorTipoDocumento, u.NumeroDocumento AS CompradorDocumento, u.Telefono AS CompradorTelefono,
    e.Nombre AS EventoNombre, e.FechaEvento, e.HoraEvento, e.ImagenUrl AS EventoImagenUrl,
    l.Nombre AS LocacionNombre, l.Ciudad,
    CantidadEntradas = ISNULL((SELECT SUM(d.Cantidad) FROM dbo.PedidoDetalles d WHERE d.PedidoId = p.PedidoId), 0),
    EstadoUltimoPago = (SELECT TOP (1) pg.Estado FROM dbo.Pagos pg WHERE pg.PedidoId = p.PedidoId ORDER BY pg.FechaReporte DESC, pg.PagoId DESC)
FROM dbo.Pedidos p
INNER JOIN dbo.Usuarios   u ON u.UsuarioId = p.UsuarioId
INNER JOIN dbo.Eventos    e ON e.EventoId  = p.EventoId
INNER JOIN dbo.Locaciones l ON l.LocacionId = e.LocacionId;
GO


/* >>>>>>>>>>>>>>>>>>>> modulo3_pedidos/M3_02_procedimientos.sql <<<<<<<<<<<<<<<<<<<< */
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


/* >>>>>>>>>>>>>>>>>>>> modulo3_pedidos/M3_03_triggers.sql <<<<<<<<<<<<<<<<<<<< */
/* =============================================================================
   M3 · Triggers
   Módulo: Módulo 3 · Pedidos, historial, cancelación y vencimiento de reservas (HU-12, 13, 14)
   Objetos: trg_PedidoDetalles_ValidarEvento
   Depende de: base/01..06, modulo2 (vistas de eventos)
   ============================================================================= */
USE EventCR_BoleteriaPro_DB;
GO

/* Las líneas de un pedido solo pueden usar tipos de entrada del evento del pedido */
CREATE OR ALTER TRIGGER dbo.trg_PedidoDetalles_ValidarEvento
ON dbo.PedidoDetalles
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (
        SELECT 1
        FROM inserted i
        INNER JOIN dbo.Pedidos p      ON p.PedidoId = i.PedidoId
        INNER JOIN dbo.TiposEntrada t ON t.TipoEntradaId = i.TipoEntradaId
        WHERE t.EventoId <> p.EventoId)
    BEGIN
        ROLLBACK TRANSACTION;
        THROW 50050, N'El tipo de entrada no pertenece al evento del pedido.', 1;
    END
END
GO


/* >>>>>>>>>>>>>>>>>>>> modulo4_pagos_qr/M4_01_vistas.sql <<<<<<<<<<<<<<<<<<<< */
/* =============================================================================
   M4 · Vistas
   Módulo: Módulo 4 · Pagos, auditoría, entradas QR y control de acceso (HU-15 a HU-19)
   Objetos: vw_PagosDetalle, vw_EntradasDetalle
   Depende de: base/01..06, modulo3 (vw_PedidosResumen, sp_Pedido_LiberarVencidos)
   ============================================================================= */
USE EventCR_BoleteriaPro_DB;
GO

/* Pagos con pedido, comprador, evento y auditor */
CREATE OR ALTER VIEW dbo.vw_PagosDetalle
AS
SELECT
    pg.PagoId, pg.CodigoPago, pg.PedidoId, pg.AuditorUsuarioId, pg.BancoOrigen, pg.ReferenciaComprobante,
    pg.CodigoAprobacion, pg.MontoReportado, pg.ComprobanteUrl, pg.HashComprobante, pg.NotaComprador,
    pg.Estado, pg.MotivoRechazo, pg.FechaReporte, pg.FechaValidacion,
    p.CodigoPedido, p.ImporteTotal AS PedidoImporteTotal, p.Estado AS PedidoEstado, p.ReferenciaBancariaUnica,
    p.MetodoPagoSeleccionado, p.UsuarioId AS CompradorUsuarioId,
    u.NombreCompleto AS CompradorNombre, u.CorreoElectronico AS CompradorCorreo, u.NumeroDocumento AS CompradorDocumento,
    u.Telefono AS CompradorTelefono,
    e.EventoId, e.Nombre AS EventoNombre, e.FechaEvento,
    a.NombreCompleto AS AuditorNombre,
    MontoCoincide = CAST(CASE WHEN pg.MontoReportado = p.ImporteTotal THEN 1 ELSE 0 END AS BIT)
FROM dbo.Pagos pg
INNER JOIN dbo.Pedidos  p ON p.PedidoId  = pg.PedidoId
INNER JOIN dbo.Usuarios u ON u.UsuarioId = p.UsuarioId
INNER JOIN dbo.Eventos  e ON e.EventoId  = p.EventoId
LEFT  JOIN dbo.Usuarios a ON a.UsuarioId = pg.AuditorUsuarioId;
GO

/* Entradas emitidas con evento, tipo, locación y validador */
CREATE OR ALTER VIEW dbo.vw_EntradasDetalle
AS
SELECT
    en.EntradaId, en.CodigoTicket, en.PedidoId, en.EventoId, en.TipoEntradaId, en.NombreAsistente,
    en.DocumentoAsistente, en.AsientoAsignado, en.PuertaAcceso, en.HashCriptografico, en.Estado,
    en.FechaEmision, en.FechaHoraIngreso, en.ValidadorPuertaUsuarioId,
    p.CodigoPedido, p.UsuarioId,
    e.Nombre AS EventoNombre, e.FechaEvento, e.HoraEvento, e.ImagenUrl AS EventoImagenUrl, e.Estado AS EventoEstado,
    l.Nombre AS LocacionNombre, l.Ciudad, l.Direccion,
    t.Nombre AS TipoEntradaNombre, t.ColorHex,
    v.NombreCompleto AS ValidadorNombre
FROM dbo.Entradas en
INNER JOIN dbo.Pedidos      p ON p.PedidoId = en.PedidoId
INNER JOIN dbo.Eventos      e ON e.EventoId = en.EventoId
INNER JOIN dbo.Locaciones   l ON l.LocacionId = e.LocacionId
INNER JOIN dbo.TiposEntrada t ON t.TipoEntradaId = en.TipoEntradaId
LEFT  JOIN dbo.Usuarios     v ON v.UsuarioId = en.ValidadorPuertaUsuarioId;
GO


/* >>>>>>>>>>>>>>>>>>>> modulo4_pagos_qr/M4_02_procedimientos.sql <<<<<<<<<<<<<<<<<<<< */
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


/* >>>>>>>>>>>>>>>>>>>> modulo4_pagos_qr/M4_03_triggers.sql <<<<<<<<<<<<<<<<<<<< */
/* =============================================================================
   M4 · Triggers
   Módulo: Módulo 4 · Pagos, auditoría, entradas QR y control de acceso (HU-15 a HU-19)
   Objetos: trg_Entradas_ValidarConsistencia
   Depende de: base/01..06, modulo3 (vw_PedidosResumen, sp_Pedido_LiberarVencidos)
   ============================================================================= */
USE EventCR_BoleteriaPro_DB;
GO

/* Una entrada debe coincidir con el evento de su pedido y de su tipo de entrada */
CREATE OR ALTER TRIGGER dbo.trg_Entradas_ValidarConsistencia
ON dbo.Entradas
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (
        SELECT 1
        FROM inserted i
        INNER JOIN dbo.Pedidos p      ON p.PedidoId = i.PedidoId
        INNER JOIN dbo.TiposEntrada t ON t.TipoEntradaId = i.TipoEntradaId
        WHERE i.EventoId <> p.EventoId OR i.EventoId <> t.EventoId)
    BEGIN
        ROLLBACK TRANSACTION;
        THROW 50051, N'La entrada no es consistente con el evento de su pedido o de su tipo de entrada.', 1;
    END
END
GO


/* >>>>>>>>>>>>>>>>>>>> modulo2_eventos/M2_05_reportes.sql <<<<<<<<<<<<<<<<<<<< */
/* =============================================================================
   M2 · Reportes (panel y ventas por evento)
   Módulo: Módulo 2 · Eventos, cartelera, tipos de entrada y disponibilidad (HU-04, 05, 06, 09, 10, 11)
   Se ejecuta después de los módulos 3 y 4 porque el panel lee vw_PedidosResumen y vw_PagosDetalle.
   Objetos: vw_VentasPorEvento, sp_Reporte_Dashboard, sp_Reporte_VentasPorEvento
   Depende de: modulo2, modulo3, modulo4
   ============================================================================= */
USE EventCR_BoleteriaPro_DB;
GO

/* Ventas consolidadas por evento (reportes y panel) */
CREATE OR ALTER VIEW dbo.vw_VentasPorEvento
AS
SELECT
    e.EventoId, e.CodigoReferencia, e.Nombre AS EventoNombre, e.FechaEvento, e.Estado,
    e.CategoriaId, c.Nombre AS CategoriaNombre, l.Nombre AS LocacionNombre,
    CapacidadAsignada     = ISNULL(t.CapacidadAsignada, 0),
    EntradasVendidas      = ISNULL(t.Vendidas, 0),
    EntradasComprometidas = ISNULL(t.Comprometidas, 0),
    EntradasDisponibles   = ISNULL(t.CapacidadAsignada, 0) - ISNULL(t.Vendidas, 0) - ISNULL(t.Comprometidas, 0),
    PedidosPagados        = ISNULL(v.PedidosPagados, 0),
    IngresosTotales       = ISNULL(v.Ingresos, 0),
    PorcentajeOcupacion   = CAST(ROUND(100.0 * ISNULL(t.Vendidas, 0) / NULLIF(t.CapacidadAsignada, 0), 1) AS DECIMAL(5,1))
FROM dbo.Eventos e
INNER JOIN dbo.Categorias c ON c.CategoriaId = e.CategoriaId
INNER JOIN dbo.Locaciones l ON l.LocacionId = e.LocacionId
LEFT JOIN (SELECT EventoId, SUM(CantidadInicial) AS CapacidadAsignada, SUM(CantidadVendida) AS Vendidas,
                  SUM(CantidadComprometida) AS Comprometidas
           FROM dbo.TiposEntrada GROUP BY EventoId) t ON t.EventoId = e.EventoId
LEFT JOIN (SELECT EventoId, COUNT(*) AS PedidosPagados, SUM(ImporteTotal) AS Ingresos
           FROM dbo.Pedidos WHERE Estado = 'Pagado' GROUP BY EventoId) v ON v.EventoId = e.EventoId;
GO

/* Panel principal: 4 conjuntos de resultados */
CREATE OR ALTER PROCEDURE dbo.sp_Reporte_Dashboard
AS
BEGIN
    SET NOCOUNT ON;
    -- 1. Indicadores
    SELECT
        EventosPublicados  = (SELECT COUNT(*) FROM dbo.Eventos WHERE Estado IN ('Publicado', 'Agotado')),
        EventosBorrador    = (SELECT COUNT(*) FROM dbo.Eventos WHERE Estado = 'Borrador'),
        EventosFinalizados = (SELECT COUNT(*) FROM dbo.Eventos WHERE Estado = 'Finalizado'),
        UsuariosRegistrados = (SELECT COUNT(*) FROM dbo.Usuarios),
        UsuariosNuevosHoy  = (SELECT COUNT(*) FROM dbo.Usuarios WHERE CAST(FechaRegistro AS DATE) = CAST(GETDATE() AS DATE)),
        PedidosTotales     = (SELECT COUNT(*) FROM dbo.Pedidos),
        PedidosEnProceso   = (SELECT COUNT(*) FROM dbo.Pedidos WHERE Estado IN ('PendientePago', 'EnValidacion')),
        PagosPorValidar    = (SELECT COUNT(*) FROM dbo.Pagos WHERE Estado = 'PendienteValidacion'),
        EntradasEmitidas   = (SELECT COUNT(*) FROM dbo.Entradas WHERE Estado IN ('Valida', 'Utilizada')),
        IngresosTotales    = ISNULL((SELECT SUM(ImporteTotal) FROM dbo.Pedidos WHERE Estado = 'Pagado'), 0);

    -- 2. Pagos pendientes más antiguos
    SELECT TOP (5) * FROM dbo.vw_PagosDetalle WHERE Estado = 'PendienteValidacion' ORDER BY FechaReporte;

    -- 3. Próximos eventos publicados con ocupación
    SELECT TOP (4) * FROM dbo.vw_VentasPorEvento
    WHERE Estado IN ('Publicado', 'Agotado') AND FechaEvento >= CAST(GETDATE() AS DATE)
    ORDER BY FechaEvento;

    -- 4. Pedidos recientes
    SELECT TOP (5) * FROM dbo.vw_PedidosResumen ORDER BY FechaCreacion DESC;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Reporte_VentasPorEvento
    @FechaDesde  DATE = NULL,
    @FechaHasta  DATE = NULL,
    @CategoriaId INT  = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM dbo.vw_VentasPorEvento
    WHERE (@FechaDesde IS NULL OR FechaEvento >= @FechaDesde)
      AND (@FechaHasta IS NULL OR FechaEvento <= @FechaHasta)
      AND (@CategoriaId IS NULL OR CategoriaId = @CategoriaId)
    ORDER BY FechaEvento;
END
GO


/* >>>>>>>>>>>>>>>>>>>> modulo1_usuarios/M1_02_datos_iniciales.sql <<<<<<<<<<<<<<<<<<<< */
/* =============================================================================
   M1 · Datos iniciales: usuarios por rol, categorías y locaciones
   Módulo: Módulo 1 · Usuarios, autenticación, categorías y locaciones (HU-01, 02, 03, 07, 08)
   Contraseña de TODOS los usuarios de prueba: Boleteria2026! (hash PBKDF2-SHA256, 100 000 iteraciones).
   El hash se genera con la API; nunca se guardan contraseñas en texto plano.
   Depende de: base
   ============================================================================= */
USE EventCR_BoleteriaPro_DB;
GO
SET NOCOUNT ON;
GO

DECLARE @Hash VARCHAR(255) = 'PBKDF2-SHA256$100000$o/HCnntdTo+aCxwtPk9QYQ==$vsUzxa7rfGiibxm2Y7XA9cwZOvA4dBrZU3+4HlUkSJ8=';

/* ---------- Usuarios (uno por rol + compradores) ---------- */
INSERT INTO dbo.Usuarios (TipoDocumento, NumeroDocumento, NombreCompleto, CorreoElectronico, Telefono, PasswordHash, Rol, Estado, FechaRegistro)
VALUES
 ('CEDULA',   '1-1111-1111', 'Administrador General',   'admin@boleteriapro.cr',      '+506 8800-0001', @Hash, 'Superadministrador',   'Activo', DATEADD(DAY,-60,GETDATE())),
 ('CEDULA',   '1-2222-2222', 'Laura Vargas Solano',     'eventos@boleteriapro.cr',    '+506 8800-0002', @Hash, 'AdministradorEventos', 'Activo', DATEADD(DAY,-55,GETDATE())),
 ('CEDULA',   '1-3333-3333', 'Carlos Mora Jiménez',     'auditor@boleteriapro.cr',    '+506 8800-0003', @Hash, 'AuditorPagos',         'Activo', DATEADD(DAY,-50,GETDATE())),
 ('CEDULA',   '1-4444-4444', 'Andrea Rojas Quesada',    'acceso@boleteriapro.cr',     '+506 8800-0004', @Hash, 'ValidadorAcceso',      'Activo', DATEADD(DAY,-45,GETDATE())),
 ('CEDULA',   '1-0987-0654', 'Mariana Gómez Restrepo',  'mariana.gomez@correo.com',   '+506 8888-1234', @Hash, 'Comprador',            'Activo', DATEADD(DAY,-20,GETDATE())),
 ('DIMEX',    '155812345678','Carlos Andrés Méndez',    'carlos.mendez@correo.com',   '+506 8777-4321', @Hash, 'Comprador',            'Activo', DATEADD(DAY,-12,GETDATE())),
 ('PASAPORTE','P1234567',    'Valeria Ruiz Castro',     'valeria.ruiz@correo.com',    NULL,             @Hash, 'Comprador',            'Activo', DATEADD(DAY,-3,GETDATE())),
 ('CEDULA',   '2-0555-0777', 'Roberto Silva Arias',     'roberto.silva@correo.com',   '+506 8666-9090', @Hash, 'Comprador',            'Bloqueado', DATEADD(DAY,-30,GETDATE()));

/* ---------- Categorías ---------- */
INSERT INTO dbo.Categorias (Nombre, Descripcion, Activo) VALUES
 ('Tecnología',       'Congresos, cumbres y conferencias de tecnología e innovación', 1),
 ('Festivales',       'Festivales musicales y culturales al aire libre', 1),
 ('Conciertos',       'Conciertos sinfónicos, populares y giras', 1),
 ('Negocios',         'Foros empresariales y de emprendimiento', 1),
 ('Teatro',           'Obras de teatro, danza y artes escénicas', 1),
 ('Deportes & Gaming','Competencias deportivas y torneos de e-sports', 1);

/* ---------- Locaciones ---------- */
INSERT INTO dbo.Locaciones (Nombre, Ciudad, Direccion, PabellonPuerta, CapacidadMaxima, Activo) VALUES
 ('Centro de Convenciones de Costa Rica', 'Heredia',  'Cariari, Belén, Heredia',                 'Pabellón Central · Puerta 1', 5000, 1),
 ('Parque Metropolitano La Sabana',       'San José', 'Paseo Colón final, La Sabana',            'Acceso Norte',               8000, 1),
 ('Teatro Nacional de Costa Rica',        'San José', 'Avenida 2, calles 3 y 5',                 'Puerta Principal',            1100, 1),
 ('Auditorio Empresarial Norte',          'Alajuela', 'Zona Franca Coyol, Alajuela',             'Puerta B',                     800, 1),
 ('Arena Central Gaming',                 'Cartago',  'Paseo Metrópoli, Cartago',                'Puerta 3',                    3000, 1);
GO


/* >>>>>>>>>>>>>>>>>>>> modulo2_eventos/M2_04_datos_iniciales.sql <<<<<<<<<<<<<<<<<<<< */
/* =============================================================================
   M2 · Datos iniciales: eventos y tipos de entrada
   Módulo: Módulo 2 · Eventos, cartelera, tipos de entrada y disponibilidad (HU-04, 05, 06, 09, 10, 11)
   Las fechas son relativas a la fecha de instalación (siempre futuras).
   Depende de: M1_02_datos_iniciales.sql
   ============================================================================= */
USE EventCR_BoleteriaPro_DB;
GO
SET NOCOUNT ON;
GO

/* ---------- Eventos ---------- */
DECLARE @Hoy DATE = CAST(GETDATE() AS DATE);
INSERT INTO dbo.Eventos (CodigoReferencia, CategoriaId, LocacionId, Nombre, DescripcionCorta, DescripcionCompleta, Organizador, ImagenUrl, FechaEvento, HoraEvento, Estado, FechaCreacion)
VALUES
 ('EVT-TEC-2026-001', 1, 1, 'Cumbre Latinoamericana de Tecnología e IA',
  'Dos días de conferencias, talleres y networking sobre inteligencia artificial aplicada.',
  'La cumbre reúne a investigadores, desarrolladores y líderes de producto de la región. Incluye charlas magistrales, talleres prácticos de IA aplicada, zona de exhibición de startups y espacios de networking.',
  'Alianza Tech Centroamérica', 'https://lh3.googleusercontent.com/aida-public/AB6AXuDO87WVwwpGO7K_973jS-FMu5ON7pZeyl06vbwMV3TkZjhFcNPO5TghL0GThxqz1fHfCqo-ydtbI-C_KDjD3ClmADFyj1uUfC_TYrKfnajdforTcJxxvNduJGKO0ksuIBHs-6SnTFL2nBzlD8RMVrz5trqKmP08cbZ9ayaBJTgq_t5jWEOco3QNH4rBwjRfMCNOZS20RcaBvtt1TiNpGVOT0apb9t3mjZYWOV7aRMmoMNKBXWHrNffANw', DATEADD(DAY, 25, @Hoy), '09:00', 'Publicado', DATEADD(DAY,-40,GETDATE())),
 ('EVT-FES-2026-002', 2, 2, 'Festival de Jazz & Blues al Parque',
  'Una tarde de jazz y blues al aire libre con artistas nacionales e internacionales.',
  'Festival familiar al aire libre con tres escenarios, food trucks y zona de picnic. Entrada general y palcos especiales con vista preferencial al escenario principal.',
  'Producciones Sabana Viva', 'https://lh3.googleusercontent.com/aida-public/AB6AXuAJM63Ydf9NAqtiUo3wh_fF-gNUU9bmd1SEjxQkvji-m8M8teehKyuKq84zdgHHYxh6eGaz657TTmxwgDH2sjFyejt16V7j61al9zRCoEJVnm_RZgcNVjwfgLEu_j4cI1UhnePGeb4fsqsYKmoJ7Qp420JOty-7n8pi5lQKP2bUysmhqDMdm702_qJCovs13KVu1Yx0C9Yx6TApNSDu3oUx3phXZDrRZ38oTMQmkMyzcpwk_mO0pLlL_w', DATEADD(DAY, 32, @Hoy), '14:00', 'Publicado', DATEADD(DAY,-35,GETDATE())),
 ('EVT-CON-2026-003', 3, 3, 'Sinfonía Urbana & Filarmónica en Concierto',
  'La orquesta filarmónica interpreta clásicos del pop y del rock en formato sinfónico.',
  'Un recorrido por grandes éxitos contemporáneos con arreglos para orquesta completa, coro y solistas invitados en el escenario histórico del Teatro Nacional.',
  'Orquesta Filarmónica de Costa Rica', 'https://lh3.googleusercontent.com/aida-public/AB6AXuBdpayF2Y6uthAKH76SSwg-EZg9ZD9nyuA7kxB56va1WEOfpaxM150JpzREHbq_L1g7DneCxW5sErjWuKZcQYdl53KEOgctm-0w5fUYgNI2ThvtminehgSIRtXns9D-EOl73PYCcswi_Q3eewv4twyMb2cJI4BOYhJ915F9f43fEeUfd4ZaI22OBoiK30Y9To0vGGRxJmD96ZyU7RUb3NZVzRUhNK4sHqZrxUIWPkJtZmAkf3TXqBbrVw', DATEADD(DAY, 40, @Hoy), '20:00', 'Publicado', DATEADD(DAY,-30,GETDATE())),
 ('EVT-NEG-2026-004', 4, 4, 'Foro de Emprendimiento y Finanzas',
  'Conferencias y mentorías para emprendedores, inversionistas y equipos de finanzas.',
  'Jornada completa con paneles de inversión, rondas de mentoría uno a uno y espacio de networking con café incluido.',
  'Cámara de Emprendedores', 'https://lh3.googleusercontent.com/aida-public/AB6AXuByZrgPug6nqG27emHYH0Xrar3EYln-VMzgLVvXoRWNHUz0m9Q2mkhPwiWx5mbALMuFYLZ3ZG2Mp5fkPmY-CWdjSBx9YnGHDv5Af7mLP8_LDJIQs0W5IMqJad2s2DtCK_LqnAEk7pt3U_MKiBFhncVDNeIeIJYbSPysn-M12bGArvn-0r4qVS-1JpoGh5Gf83UIej5mMe7LzNOE6dxZf8dzZO8wZrW_vAqMAYUyyf3geYAKK8fNJ37UzA', DATEADD(DAY, 18, @Hoy), '08:30', 'Publicado', DATEADD(DAY,-25,GETDATE())),
 ('EVT-TEA-2026-005', 5, 3, 'Obra Teatral Clásica: Noche de Dramaturgia',
  'Puesta en escena de un clásico de la dramaturgia universal con elenco nacional.',
  'Montaje de gran formato con escenografía original y música en vivo. Función con elenco estelar y conversatorio posterior con el director.',
  'Compañía Nacional de Teatro', 'https://lh3.googleusercontent.com/aida-public/AB6AXuAg8wLc_7_Ot9kYe_OBnMXF65Qhr2juv11bTLGeShuL-rzhpZw3Ej753qZ_fFrcedeFs648XW090P4xXPjqLsktYLZPdlWBoRH6g9jCLV9fQSqWHIVD0F5EXBF2akzmBWhWXfrVwCbSZx7l40GemqC6-Tp_t-gQ0Fnnq2u6Jzai79h_vxlFhweQOXJER8t74RGJgBfGh8Ny2GOaXeJlOQMEe6J3J3iQpp5LyZXq3x2eGKMcAAmG0RqBdg', DATEADD(DAY, 50, @Hoy), '19:30', 'Borrador', DATEADD(DAY,-10,GETDATE())),
 ('EVT-ESP-2026-006', 6, 5, 'Torneo Internacional de Esports Grand Finals',
  'La gran final regional de esports con equipos profesionales.',
  'Final presencial con transmisión en vivo, zona de exhibición de hardware y meet & greet con los equipos finalistas.',
  'Liga Gamer Centroamericana', 'https://lh3.googleusercontent.com/aida-public/AB6AXuAqSpZruLt2ykOPvioe7KeaHI92TC-cqmRpkTyrjcBdJKvlFtit7ATW4o8jrCUfdWMuWYMX9fK20O9wJE2ObJ_mISVKgvGRcfLTz5MD1Vc_YHMFPBAGmkgtqp5hBZExIFlX8FM7YnygBlU2JmmS2WpayyEHmywc7OI7yKBaV-DdY6EQFJMokxQh3p_OmOx9YgItD_WchGuqnvAjtN5K33GY1UVpMV3ChEI-fRv5xSIrvZ3iyoztl9ljvg', DATEADD(DAY, -15, @Hoy), '14:00', 'Finalizado', DATEADD(DAY,-90,GETDATE()));

/* ---------- Tipos de entrada ---------- */
INSERT INTO dbo.TiposEntrada (EventoId, Nombre, Precio, ColorHex, CantidadInicial, CantidadComprometida, CantidadVendida, Activo) VALUES
 (1, 'VIP Front Stage',        180000.00, '#0051D5',  150, 0, 0, 1),
 (1, 'Preferencial',            120000.00, '#316BF3',  600, 0, 0, 1),
 (1, 'General Early Bird',       85000.00, '#7BD0FF', 1500, 0, 0, 1),
 (2, 'Palco Especial',           60000.00, '#0051D5',  300, 0, 0, 1),
 (2, 'Entrada General',          30000.00, '#7BD0FF', 3000, 0, 0, 1),
 (3, 'Platea Preferencial',      55000.00, '#0051D5',  350, 0, 0, 1),
 (3, 'Balcón',                   35000.00, '#7BD0FF',  450, 0, 0, 1),
 (4, 'Acceso Corporativo',       75000.00, '#0051D5',  200, 0, 0, 1),
 (4, 'Estudiante Acreditado',    25000.00, '#7BD0FF',  150, 0, 0, 1),
 (5, 'Platea',                   35000.00, '#316BF3',  700, 0, 0, 1),
 (6, 'General Arena',            35000.00, '#316BF3', 2500, 0, 0, 1);
GO


/* >>>>>>>>>>>>>>>>>>>> modulo4_pagos_qr/M4_04_datos_prueba.sql <<<<<<<<<<<<<<<<<<<< */
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


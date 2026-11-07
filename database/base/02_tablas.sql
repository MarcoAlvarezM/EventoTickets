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

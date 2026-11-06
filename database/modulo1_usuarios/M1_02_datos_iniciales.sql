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

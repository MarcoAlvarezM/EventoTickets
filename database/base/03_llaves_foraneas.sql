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

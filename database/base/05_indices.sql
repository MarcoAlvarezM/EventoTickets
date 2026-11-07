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

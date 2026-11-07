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

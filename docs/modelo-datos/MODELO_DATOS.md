# Modelo de datos · EventCR_BoleteriaPro_DB

**Responsable:** Módulo 2 (Donnkan Cervantes Peraza) — base de datos general.
Motor SQL Server 2019+ · intercalación `Modern_Spanish_CI_AS` · `READ_COMMITTED_SNAPSHOT ON`.

## 1. Fidelidad con el diagrama ER

Las **9 tablas**, sus columnas, tipos, longitudes y nulabilidad son las del diagrama ER (no se agregaron entidades ni relaciones). Conversión de tipos genéricos: `boolean` → `BIT`; `text` → `VARCHAR(MAX)` (`TEXT` está obsoleto); `datetime`/`date`/`time` → `DATETIME`/`DATE`/`TIME(0)`.

### Relaciones (13, todas 1:N, `ON DELETE NO ACTION`)

| Padre | Hijo (FK) | Obligatoria |
|---|---|---|
| Categorias | Eventos.CategoriaId | Sí |
| Locaciones | Eventos.LocacionId | Sí |
| Eventos | TiposEntrada.EventoId | Sí |
| Usuarios | Pedidos.UsuarioId | Sí |
| Eventos | Pedidos.EventoId | Sí |
| Pedidos | PedidoDetalles.PedidoId | Sí |
| TiposEntrada | PedidoDetalles.TipoEntradaId | Sí |
| Pedidos | Pagos.PedidoId | Sí |
| Usuarios | Pagos.AuditorUsuarioId | No (se llena al validar) |
| Pedidos | Entradas.PedidoId | Sí |
| Eventos | Entradas.EventoId | Sí |
| TiposEntrada | Entradas.TipoEntradaId | Sí |
| Usuarios | Entradas.ValidadorPuertaUsuarioId | No (se llena al ingresar) |

### Normalización (3FN)
Cada atributo no clave depende solo de la clave de su tabla. `Subtotal`, `CargoServicio`, `ImpuestoIVA`, `ImporteTotal` y `PrecioUnitario` se guardan porque el ER los define y son **valores históricos** (HU-10: el precio puede cambiar después de la compra). La redundancia de `EventoId` en `Pedidos` y `Entradas` es del diagrama y se protege con triggers.

## 2. Objetos de la base

| Tipo | Cantidad | Dónde |
|---|---|---|
| Tablas | 9 | `base/02_tablas.sql` |
| Claves foráneas | 13 | `base/03_llaves_foraneas.sql` |
| UNIQUE / CHECK / DEFAULT | 11 (9 del ER + 2 de negocio) / 26 / 16 | `base/04_restricciones.sql` |
| Índices no agrupados | 17 | `base/05_indices.sql` |
| Funciones | 3 (`fn_DisponibilidadTipoEntrada`, `fn_CapacidadAsignadaEvento`, `fn_DisponibilidadEvento`) | `base/06_funciones.sql` |
| Vistas | 6 | M2: `vw_EventosCatalogo`, `vw_TiposEntradaDisponibilidad`, `vw_VentasPorEvento` · M3: `vw_PedidosResumen` · M4: `vw_PagosDetalle`, `vw_EntradasDetalle` |
| Procedimientos | 50 | M1: 18 · M2: 11 + 2 de reportes · M3: 7 · M4: 12 |
| Triggers | 3 | M2: `trg_TiposEntrada_SincronizarAgotado` · M3: `trg_PedidoDetalles_ValidarEvento` · M4: `trg_Entradas_ValidarConsistencia` |

## 3. Reglas de negocio garantizadas por la base

| Regla | Implementación |
|---|---|
| No sobrevender / no doble reserva | `sp_Pedido_Crear`: un único `UPDATE … WITH (UPDLOCK, ROWLOCK)` condicionado a `Inicial − Comprometida − Vendida ≥ cantidad`; si alguna línea no se actualiza, revierte todo y lanza 50409. Respaldo: `CHECK CK_TiposEntrada_NoSobreventa`. |
| Reserva temporal y vencimiento (HU-14) | El pedido compromete cupo hasta `FechaVencimientoReserva`; `sp_Pedido_LiberarVencidos` marca `Vencido` y devuelve el cupo **una sola vez** (solo actúa sobre `PendientePago`). Lo ejecutan el servicio del API, `sp_Pedido_Crear`, `sp_Pago_Registrar` y, opcionalmente, un job de SQL Agent. |
| Agotado automático (HU-11) | `trg_TiposEntrada_SincronizarAgotado`: Publicado → Agotado cuando la disponibilidad del evento llega a 0, y vuelve a Publicado si se libera cupo. |
| Aforo (HU-09) | Suma de `CantidadInicial` del evento ≤ `CapacidadMaxima` de la locación (crear/editar tipo, cambiar locación, reducir capacidad). |
| Cupo mínimo (HU-10) | `CantidadInicial` no puede bajar de `Comprometida + Vendida` (50016). |
| Código de evento (HU-04) | Si no se envía, `sp_Evento_Crear` genera `EVT-AAAA-00000` único. |
| Comprobante duplicado (HU-15) | Se rechaza el mismo `HashComprobante` (SHA-256) o banco + referencia de otro pago no rechazado (50409). |
| Aprobación atómica (HU-17/18) | En una transacción: pago Aprobado → pedido Pagado → comprometidas pasan a vendidas → una entrada por unidad en estado `Valida`, con código único y hash SHA-256. |
| Doble ingreso (HU-19) | `sp_Entrada_RegistrarIngreso` bloquea la fila (`UPDLOCK`) y rechaza entradas `Utilizada` (50043), `Anulada` o con hash distinto (QR regenerado). |
| Cancelación de evento (HU-05) | Cancela pedidos abiertos (libera cupo) y anula entradas válidas. |
| Historial | Los pedidos nunca se borran (la prueba `M3_04` verifica que ningún módulo programado haga `DELETE` sobre Pedidos). Usuarios, categorías, locaciones y tipos con historial se desactivan. |
| Roles en operaciones sensibles | `sp_Pago_Aprobar/Rechazar` y `sp_Entrada_RegistrarIngreso` verifican el rol del usuario, además del `[Authorize]` del API. |

Errores: `THROW 50404` (no existe), `50409` (conflicto), `50403` (sin permiso), `50400` (formato), `50001–50059` (reglas por módulo). La capa DA los traduce a 404/409/403/400.

## 4. Decisiones sobre inconsistencias del ER

| Hallazgo | Solución adoptada (sin inventar entidades) |
|---|---|
| `EventoId` repetido en Pedidos/PedidoDetalles→TiposEntrada/Entradas | 2 triggers de consistencia |
| Stitch muestra mapa de asientos; el ER no tiene entidad Asiento | `AsientoAsignado` se genera al emitir (`VIP-1`, `VIP-2`…) |
| El ER no guarda asistentes por pedido | Las entradas se emiten a nombre del comprador y se **nominan** después (`sp_Entrada_Transferir`, regenera el hash del QR) |
| `Pagos.CodigoAprobacion` | Código de conciliación del auditor (manual o autogenerado) |
| El ER no fija dominios de estados | `CHECK` con los valores del anteproyecto (ver guía general §6.3) |
| Duplicados de nombre de tipo por evento y de tipo por pedido | `UQ(EventoId, Nombre)` y `UQ(PedidoId, TipoEntradaId)` (restricciones, no entidades) |

## 5. Diccionario de datos

### Usuarios

| Columna | Tipo | Nulo | Clave / restricción |
|---|---|---|---|
| UsuarioId | INT | No | PK (IDENTITY) |
| TipoDocumento | VARCHAR(10) | No | CHECK `TipoDocumento IN ('CEDULA','DIMEX','PASAPORTE')` |
| NumeroDocumento | VARCHAR(30) | No | UNIQUE |
| NombreCompleto | VARCHAR(150) | No |  |
| CorreoElectronico | VARCHAR(150) | No | UNIQUE; CHECK `CorreoElectronico LIKE '%_@_%._%'` |
| Telefono | VARCHAR(30) | Sí |  |
| PasswordHash | VARCHAR(255) | No |  |
| Rol | VARCHAR(40) | No | CHECK `Rol IN ('Superadministrador','AdministradorEventos','AuditorPagos','ValidadorAcceso','Comprador')`; DEFAULT 'Comprador' |
| Estado | VARCHAR(30) | No | CHECK `Estado IN ('Activo','Bloqueado','Inactivo')`; DEFAULT 'Activo' |
| FechaRegistro | DATETIME | No | DEFAULT GETDATE() |

### Categorias

| Columna | Tipo | Nulo | Clave / restricción |
|---|---|---|---|
| CategoriaId | INT | No | PK (IDENTITY) |
| Nombre | VARCHAR(100) | No | UNIQUE |
| Descripcion | VARCHAR(255) | Sí |  |
| Activo | BIT | No | DEFAULT 1 |

### Locaciones

| Columna | Tipo | Nulo | Clave / restricción |
|---|---|---|---|
| LocacionId | INT | No | PK (IDENTITY) |
| Nombre | VARCHAR(150) | No |  |
| Ciudad | VARCHAR(100) | No |  |
| Direccion | VARCHAR(255) | No |  |
| PabellonPuerta | VARCHAR(100) | Sí |  |
| CapacidadMaxima | INT | No | CHECK `CapacidadMaxima > 0` |
| Activo | BIT | No | DEFAULT 1 |

### Eventos

| Columna | Tipo | Nulo | Clave / restricción |
|---|---|---|---|
| EventoId | INT | No | PK (IDENTITY) |
| CodigoReferencia | VARCHAR(50) | No | UNIQUE |
| CategoriaId | INT | No | FK → Categorias |
| LocacionId | INT | No | FK → Locaciones |
| Nombre | VARCHAR(150) | No |  |
| DescripcionCorta | VARCHAR(300) | Sí |  |
| DescripcionCompleta | VARCHAR(MAX) | No |  |
| Organizador | VARCHAR(150) | Sí |  |
| ImagenUrl | VARCHAR(500) | Sí |  |
| FechaEvento | DATE | No |  |
| HoraEvento | TIME(0) | No |  |
| Estado | VARCHAR(30) | No | CHECK `Estado IN ('Borrador','Publicado','Agotado','Cancelado','Finalizado')`; DEFAULT 'Borrador' |
| FechaCreacion | DATETIME | No | DEFAULT GETDATE() |

### TiposEntrada

| Columna | Tipo | Nulo | Clave / restricción |
|---|---|---|---|
| TipoEntradaId | INT | No | PK (IDENTITY) |
| EventoId | INT | No | FK → Eventos; UNIQUE(EventoId, Nombre) |
| Nombre | VARCHAR(80) | No | UNIQUE(EventoId, Nombre) |
| Precio | DECIMAL(12,2) | No | CHECK `Precio >= 0` |
| ColorHex | VARCHAR(10) | Sí | CHECK `ColorHex IS NULL OR ColorHex LIKE '#[0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f]'` |
| CantidadInicial | INT | No | CHECK `CantidadInicial > 0`; CHECK `CantidadComprometida + CantidadVendida <= CantidadInicial` |
| CantidadComprometida | INT | No | CHECK `CantidadComprometida >= 0 AND CantidadVendida >= 0`; CHECK `CantidadComprometida + CantidadVendida <= CantidadInicial`; DEFAULT 0 |
| CantidadVendida | INT | No | CHECK `CantidadComprometida >= 0 AND CantidadVendida >= 0`; CHECK `CantidadComprometida + CantidadVendida <= CantidadInicial`; DEFAULT 0 |
| Activo | BIT | No | DEFAULT 1 |

### Pedidos

| Columna | Tipo | Nulo | Clave / restricción |
|---|---|---|---|
| PedidoId | INT | No | PK (IDENTITY) |
| CodigoPedido | VARCHAR(30) | No | UNIQUE |
| UsuarioId | INT | No | FK → Usuarios |
| EventoId | INT | No | FK → Eventos |
| MetodoPagoSeleccionado | VARCHAR(60) | No | CHECK `MetodoPagoSeleccionado IN ('SINPE Movil','Transferencia Bancaria')` |
| ReferenciaBancariaUnica | VARCHAR(50) | No | UNIQUE |
| Subtotal | DECIMAL(12,2) | No | CHECK `Subtotal >= 0 AND CargoServicio >= 0 AND ImpuestoIVA >= 0`; CHECK `ImporteTotal = Subtotal + CargoServicio + ImpuestoIVA` |
| CargoServicio | DECIMAL(12,2) | No | CHECK `Subtotal >= 0 AND CargoServicio >= 0 AND ImpuestoIVA >= 0`; CHECK `ImporteTotal = Subtotal + CargoServicio + ImpuestoIVA` |
| ImpuestoIVA | DECIMAL(12,2) | No | CHECK `Subtotal >= 0 AND CargoServicio >= 0 AND ImpuestoIVA >= 0`; CHECK `ImporteTotal = Subtotal + CargoServicio + ImpuestoIVA` |
| ImporteTotal | DECIMAL(12,2) | No | CHECK `ImporteTotal = Subtotal + CargoServicio + ImpuestoIVA` |
| FechaCreacion | DATETIME | No | CHECK `FechaVencimientoReserva > FechaCreacion`; DEFAULT GETDATE() |
| FechaVencimientoReserva | DATETIME | No | CHECK `FechaVencimientoReserva > FechaCreacion` |
| Estado | VARCHAR(30) | No | CHECK `Estado IN ('PendientePago','EnValidacion','Pagado','Cancelado','Vencido')`; DEFAULT 'PendientePago' |

### PedidoDetalles

| Columna | Tipo | Nulo | Clave / restricción |
|---|---|---|---|
| PedidoDetalleId | INT | No | PK (IDENTITY) |
| PedidoId | INT | No | FK → Pedidos; UNIQUE(PedidoId, TipoEntradaId) |
| TipoEntradaId | INT | No | FK → TiposEntrada; UNIQUE(PedidoId, TipoEntradaId) |
| Cantidad | INT | No | CHECK `Cantidad > 0`; CHECK `SubtotalLinea = Cantidad * PrecioUnitario` |
| PrecioUnitario | DECIMAL(12,2) | No | CHECK `PrecioUnitario >= 0`; CHECK `SubtotalLinea = Cantidad * PrecioUnitario` |
| SubtotalLinea | DECIMAL(12,2) | No | CHECK `SubtotalLinea = Cantidad * PrecioUnitario` |

### Pagos

| Columna | Tipo | Nulo | Clave / restricción |
|---|---|---|---|
| PagoId | INT | No | PK (IDENTITY) |
| CodigoPago | VARCHAR(30) | No | UNIQUE |
| PedidoId | INT | No | FK → Pedidos |
| AuditorUsuarioId | INT | Sí | FK → Usuarios; CHECK `Estado = 'PendienteValidacion' OR (AuditorUsuarioId IS NOT NULL AND FechaValidacion IS NOT NULL)` |
| BancoOrigen | VARCHAR(80) | No |  |
| ReferenciaComprobante | VARCHAR(100) | No |  |
| CodigoAprobacion | VARCHAR(50) | Sí |  |
| MontoReportado | DECIMAL(12,2) | No | CHECK `MontoReportado > 0` |
| ComprobanteUrl | VARCHAR(500) | Sí |  |
| HashComprobante | VARCHAR(64) | No | CHECK `LEN(HashComprobante) = 64` |
| NotaComprador | VARCHAR(255) | Sí |  |
| Estado | VARCHAR(30) | No | CHECK `Estado IN ('PendienteValidacion','Aprobado','Rechazado')`; CHECK `Estado <> 'Rechazado' OR MotivoRechazo IS NOT NULL`; CHECK `Estado = 'PendienteValidacion' OR (AuditorUsuarioId IS NOT NULL AND FechaValidacion IS NOT NULL)`; DEFAULT 'PendienteValidacion' |
| MotivoRechazo | VARCHAR(255) | Sí | CHECK `Estado <> 'Rechazado' OR MotivoRechazo IS NOT NULL` |
| FechaReporte | DATETIME | No | DEFAULT GETDATE() |
| FechaValidacion | DATETIME | Sí | CHECK `Estado = 'PendienteValidacion' OR (AuditorUsuarioId IS NOT NULL AND FechaValidacion IS NOT NULL)` |

### Entradas

| Columna | Tipo | Nulo | Clave / restricción |
|---|---|---|---|
| EntradaId | INT | No | PK (IDENTITY) |
| CodigoTicket | VARCHAR(50) | No | UNIQUE |
| PedidoId | INT | No | FK → Pedidos |
| EventoId | INT | No | FK → Eventos |
| TipoEntradaId | INT | No | FK → TiposEntrada |
| NombreAsistente | VARCHAR(150) | No |  |
| DocumentoAsistente | VARCHAR(40) | No |  |
| AsientoAsignado | VARCHAR(50) | Sí |  |
| PuertaAcceso | VARCHAR(50) | Sí |  |
| HashCriptografico | VARCHAR(64) | No | UNIQUE |
| Estado | VARCHAR(30) | No | CHECK `Estado IN ('Valida','Utilizada','Anulada')`; CHECK `Estado <> 'Utilizada' OR (FechaHoraIngreso IS NOT NULL AND ValidadorPuertaUsuarioId IS NOT NULL)`; DEFAULT 'Valida' |
| FechaEmision | DATETIME | No | DEFAULT GETDATE() |
| FechaHoraIngreso | DATETIME | Sí | CHECK `Estado <> 'Utilizada' OR (FechaHoraIngreso IS NOT NULL AND ValidadorPuertaUsuarioId IS NOT NULL)` |
| ValidadorPuertaUsuarioId | INT | Sí | FK → Usuarios; CHECK `Estado <> 'Utilizada' OR (FechaHoraIngreso IS NOT NULL AND ValidadorPuertaUsuarioId IS NOT NULL)` |

# Guía de integración general · EventCR / BoleteríaPro

Documento del **núcleo compartido** y del responsable de integración. Explica cómo se divide el proyecto en 4 módulos, qué es compartido, cómo se integran las ramas y cómo se verifica el resultado.
Cada módulo tiene además su propio README, guía de integración y matriz de trazabilidad en `docs/modulos/`.

**Contenido**
1. Arquitectura general · 2. Estructura de carpetas y propiedad · 3. Diagrama de dependencias · 4. Matriz de las 19 HU · 5. Responsabilidades · 6. Contratos compartidos · 7. Modelos y DTO · 8. Rutas del API · 9. Diccionario de datos · 10. Orden de ejecución SQL · 11. Configuración de SQL Server · 12. Autenticación y autorización · 13. Estrategia Git · 14. Guía de integración paso a paso · 15. Pruebas generales

---

## 1. Arquitectura general

```
 Navegador ──HTTPS──▶ EventoTickets.Web (Razor Pages, solución 2)
                        │  cookie HttpOnly "BoleteriaPro.Sesion" con el JWT
                        │  Servicios/ModuloN → IClienteApi (HttpClient + Bearer)      ← nunca abre conexiones SQL
                        ▼
                      EventoTickets.API (controladores, solución 1)
                        │  Filtro RespuestaApi · Middleware de errores · [Authorize(Roles)]
                        ├─▶ Flujo (orquesta) ──▶ Reglas (validaciones de negocio)
                        ├─▶ Servicios (SHA-256, archivos, QR, JWT, expiración de reservas)
                        └─▶ DA (Dapper) ──▶ SQL Server: procedimientos, vistas, funciones, triggers
```

| Proyecto | Responsabilidad | No hace |
|---|---|---|
| Abstracciones | Contratos: modelos con anotaciones, interfaces, excepciones, constantes, configuración | Implementaciones |
| DA | Ejecutar procedimientos con Dapper y traducir `THROW 50xxx` a excepciones de dominio | Reglas de negocio |
| Reglas | Hash de contraseñas, cotización, transiciones de estado, validación de archivos | Acceso a datos o HTTP |
| Flujo | Orquestar Reglas + DA + Servicios; verificar propiedad de los recursos | Conocer HTTP |
| Servicios | SHA-256, almacenamiento de comprobantes fuera de `wwwroot`, QR PNG, JWT, servicio de expiración | Reglas de negocio |
| API | Rutas, verbos, origen de parámetros, roles, códigos HTTP, Swagger | Lógica de negocio |
| Web | Pantallas; envía todo al API y muestra sus mensajes por campo | Acceso a datos, reglas |

Las garantías de concurrencia (no sobrevender, no reservar dos veces el mismo cupo, no permitir doble ingreso) viven **en la base de datos**: `UPDATE … WITH (UPDLOCK, ROWLOCK)` condicionado, transacciones con `XACT_ABORT` y `CHECK (CantidadComprometida + CantidadVendida <= CantidadInicial)`. La capa Reglas valida antes solo para dar mensajes claros.

## 2. Estructura de carpetas y propiedad

Un **único repositorio** y una **única aplicación integrada**. La propiedad se asigna por **carpeta**, no por copias del proyecto:

```
API/EventoTickets.<Capa>/Nucleo/            → núcleo (responsable de integración)
API/EventoTickets.<Capa>/ModuloN_Nombre/    → módulo N        (Abstracciones, DA, Reglas, Flujo, Servicios, Pruebas)
API/EventoTickets.API/Controllers/ModuloN_*/ → módulo N
API/EventoTickets.API/Registro/ModuloNRegistro.cs → módulo N (inyección de dependencias del módulo)
API/EventoTickets.API/Registro/NucleoRegistro.cs, Program.cs, appsettings*.json, *.csproj, *.sln → núcleo
Web/EventoTickets.Web.Abstracciones/{Nucleo|ModuloN_*}/ → núcleo / módulo N
Web/EventoTickets.Web/Servicios/{Nucleo|ModuloN_*}/     → núcleo / módulo N
Web/EventoTickets.Web/Registro/ModuloNRegistro.cs       → módulo N
Web/EventoTickets.Web/Pages/…                           → según la ruta (tabla siguiente)
database/base/, 00_EJECUTAR_TODO.sql, orden_ejecucion.txt, instaladores, _completo.sql → M2 (base de datos general)
database/moduloN_*/                                     → módulo N
database/validacion/                                    → núcleo
docs/modulos/MN_*/ → módulo N  ·  docs/modelo-datos/ → M2  ·  resto de docs/ → núcleo
```

| Páginas Razor | Dueño |
|---|---|
| `Cuenta/*`, `Admin/Usuarios/*`, `Admin/Categorias/*`, `Admin/Locaciones/*`, parciales `_FormUsuario`, `_FormCategoria`, `_FormLocacion`, `_SelectorRol`, `_MatrizPermisos` | M1 |
| `Index` (cartelera), `Eventos/*`, `Admin/Eventos/*`, `Admin/TiposEntrada/*`, `Admin/Reportes/*`, `Admin/Index` (panel), parciales `_TarjetaEvento`, `_FormEvento`, `_FormTipoEntrada` | M2 |
| `Compras/Resumen`, `Compras/Checkout`, `MisPedidos/*`, `Admin/Pedidos/*`, parcial `_ResumenPedido` | M3 |
| `Compras/RegistrarPago`, `Compras/Confirmacion`, `MisPagos/*`, `MisEntradas/*`, `Admin/Pagos/*`, `Admin/Entradas/*`, `Admin/Accesos/*` | M4 |
| `_Layout`, `_AdminLayout`, `_AuthLayout`, `_Head`, `_Logo`, `_Alertas`, `_EstadoBadge`, `_EstilosTailwind`, `_ValidationScriptsPartial`, `_ViewImports`, `_ViewStart`, `Error`, `Infraestructura/*`, `wwwroot/*`, `Program.cs` | Núcleo |

Los **espacios de nombres no cambian por módulo** (`EventoTickets.Abstracciones.Modelos`, `EventoTickets.Flujo`, …): mover archivos entre carpetas no rompe referencias y los `using` son los mismos para todos.

**Archivos compartidos con un único responsable (núcleo):** `Program.cs` (API y Web), `RespuestaApi`, `RespuestaApiFiltro`, `ManejoExcepcionesMiddleware`, `Roles`, `Estados`, `Excepciones`, `Configuraciones`, `IRepositorioDapper`/`RepositorioDapper`, `TraductorErroresSql`, `IServicioHash`/`ServicioHash`, `ClienteApi`, `ResultadoApi`, `PaginaBase`, `Formato`, layouts, `.csproj`, `.sln`, `appsettings*.json`. Un módulo que necesite cambiarlos abre un PR con la etiqueta `nucleo` que revisa el responsable de integración.

El manifiesto exacto de qué archivo pertenece a cada paquete está en `MANIFIESTO.txt` dentro de cada ZIP.

## 3. Diagrama de dependencias

```
              ┌──────────── NÚCLEO (contratos, Program.cs, filtros, ClienteApi, layouts) ────────────┐
              │                                                                                       │
   API:   M1 Usuarios          M2 Eventos ◀────────── M3 Pedidos ◀──────────── M4 Pagos/QR
          (independiente)      (independiente)        IEventoDA, ITipoEntradaDA IPedidoDA, IEntradaDA
                                                                                  (+ IEventoDA vía M3)
   Web:   M1 ◀──────────────── M2 ◀────────────────── M3 ◀──────────────────── M4
               ICategoriaServicio,     IEventoServicio,           IPedidoServicio,
               ILocacionServicio       ITipoEntradaServicio       _ResumenPedido
               (formularios/filtros)   (pantalla Resumen)
   SQL:   base/ (M2) ──▶ M1 ──▶ M2 ──▶ M3 ──▶ M4 ──▶ M2_05_reportes ──▶ datos (M1, M2, M4)
```

- El grafo es **acíclico**: ningún módulo depende de uno posterior.
- **Orden de integración:** núcleo → M1 → M2 → M3 → M4.
- Dependencias eliminadas para lograrlo: la lista de entradas por evento pasó de `EventosController` a `EntradasController` (M4); los pagos de un pedido pasaron a `GET /api/pagos/pedido/{id}` (M4); el panel usa sus propios DTO (`DashboardPago`, `DashboardPedido`); el detalle del evento (M2) envía un **contrato de formulario** a `Compras/Resumen` (M3) sin usar clases de M3; `ArchivoDescargado` es un DTO del núcleo.
- **Verificado compilando subconjuntos** (ver §15): núcleo+M1, núcleo+M1+M2, núcleo+M1+M2+M3 y los 4 módulos compilan con 0 errores; en el API, núcleo+M2 compila sin M1.

## 4. Matriz de las 19 historias de usuario

| HU | Historia (resumen) | Módulo | Procedimientos | Endpoints | Vistas web | Pruebas automatizadas |
|---|---|---|---|---|---|---|
| HU-01 | Registro de comprador | M1 | `sp_Usuario_Crear` | `POST /api/autenticacion/registro` | `Cuenta/Registro` | xUnit M1 (registro), SQL M1, E2E |
| HU-02 | Inicio y cierre de sesión | M1 | `sp_Usuario_ObtenerCredenciales`, `sp_Usuario_ObtenerPorId` | `POST /api/autenticacion/login`, `GET /api/autenticacion/perfil` | `Cuenta/Login`, `Cuenta/Logout` | xUnit M1 (login, hash, bloqueo), E2E |
| HU-03 | Gestión de usuarios, rol y estado | M1 | `sp_Usuario_Listar`, `_ObtenerPorId`, `_Crear`, `_Actualizar`, `_CambiarEstado`, `_CambiarPassword`, `_Eliminar` | `/api/usuarios` (GET, GET id, POST, PUT, PATCH estado, PUT password, DELETE) | `Admin/Usuarios/*` (6) | xUnit M1, SQL M1, E2E |
| HU-04 | Crear y publicar eventos | M2 | `sp_Evento_Crear`, `sp_Evento_CambiarEstado` | `POST /api/eventos`, `PATCH /api/eventos/{id}/estado` | `Admin/Eventos/Crear`, `Detalle` | xUnit M2, SQL M2, E2E |
| HU-05 | Editar y cambiar estado | M2 | `sp_Evento_Actualizar`, `_CambiarEstado`, `_Eliminar`, `_ObtenerPorId` | `PUT`, `PATCH`, `DELETE`, `GET /api/eventos/{id}` | `Admin/Eventos/Editar`, `Detalle`, `Eliminar`, `Index` | xUnit M2 (transiciones), SQL M2 |
| HU-06 | Cartelera con filtros | M2 | `sp_Evento_Listar` (`vw_EventosCatalogo`) | `GET /api/eventos`, `GET /api/eventos/{id}` | `Index`, `Eventos/Detalle` | xUnit M2 (cartelera), SQL M2, E2E |
| HU-07 | Catálogo de categorías | M1 | `sp_Categoria_*` (5) | `/api/categorias` (5) | `Admin/Categorias/*` (5) | SQL M1, E2E |
| HU-08 | Catálogo de locaciones | M1 | `sp_Locacion_*` (5) | `/api/locaciones` (5) | `Admin/Locaciones/*` (5) | SQL M1 |
| HU-09 | Configurar tipos de entrada | M2 | `sp_TipoEntrada_Crear`, `_ListarPorEvento` | `POST /api/tipos-entrada`, `GET /api/eventos/{id}/tipos-entrada` | `Admin/TiposEntrada/Index`, `Crear` | SQL M2 |
| HU-10 | Actualizar precio/cupo | M2 | `sp_TipoEntrada_Actualizar`, `_Eliminar`, `_ObtenerPorId` | `PUT`, `DELETE`, `GET /api/tipos-entrada/{id}` | `Admin/TiposEntrada/Editar`, `Detalle`, `Eliminar` | SQL M2 |
| HU-11 | Disponibilidad en tiempo real | M2 | `fn_DisponibilidadTipoEntrada`, `fn_DisponibilidadEvento`, `trg_TiposEntrada_SincronizarAgotado` | `GET /api/eventos/{id}/tipos-entrada` | `Eventos/Detalle` (selector) | SQL M2, xUnit M3 (Agotado no vende) |
| HU-12 | Generar pedido con desglose | M3 | `sp_Pedido_Crear` (+`trg_PedidoDetalles_ValidarEvento`) | `POST /api/pedidos/cotizacion`, `POST /api/pedidos` | `Compras/Resumen`, `Compras/Checkout` | xUnit M3, SQL M3, integración SQL (concurrencia), E2E |
| HU-13 | Historial y estado de pedidos | M3 | `sp_Pedido_ListarPorUsuario`, `_ObtenerPorId`, `_Listar`, `_ActualizarMetodoPago` | `GET /api/pedidos/mios`, `GET /api/pedidos/{id}`, `GET /api/pedidos`, `PATCH /api/pedidos/{id}/metodo-pago` | `MisPedidos/Index`, `Detalle`, `Editar`, `Admin/Pedidos/*` | xUnit M3, SQL M3, E2E |
| HU-14 | Cancelación y vencimiento | M3 | `sp_Pedido_Cancelar`, `sp_Pedido_LiberarVencidos` | `POST /api/pedidos/{id}/cancelacion`, `POST /api/pedidos/liberacion-vencidos` | `MisPedidos/Detalle`, `Admin/Pedidos/Index` | xUnit M3 (servicio de expiración), SQL M3, E2E |
| HU-15 | Registrar comprobante | M4 | `sp_Pago_Registrar` | `POST /api/pagos` (multipart) | `Compras/RegistrarPago` | xUnit M4, SQL M4, E2E |
| HU-16 | Auditoría de pagos | M4 | `sp_Pago_Listar`, `_ObtenerPorId`, `_ListarPorPedido`, `_ListarPorUsuario` | `GET /api/pagos`, `/{id}`, `/{id}/comprobante`, `/pedido/{id}`, `/mios` | `Admin/Pagos/Index`, `Detalle`, `MisPagos` | xUnit M4, E2E |
| HU-17 | Aprobar o rechazar pago | M4 | `sp_Pago_Aprobar`, `sp_Pago_Rechazar` | `POST /api/pagos/{id}/aprobacion`, `/rechazo` | `Admin/Pagos/Detalle` | xUnit M4, SQL M4, E2E |
| HU-18 | Entradas con QR | M4 | emisión en `sp_Pago_Aprobar`; `sp_Entrada_ListarPorUsuario`, `_ObtenerPorId`, `_Transferir`, `_ListarPorEvento` | `GET /api/entradas/mias`, `/{id}`, `/{id}/qr`, `/evento/{id}`, `PUT /{id}/titular` | `MisEntradas/*`, `Compras/Confirmacion`, `Admin/Entradas` | xUnit M4, SQL M4, E2E |
| HU-19 | Validación de acceso | M4 | `sp_Entrada_RegistrarIngreso` (+`trg_Entradas_ValidarConsistencia`) | `POST /api/entradas/ingresos` | `Admin/Accesos` | xUnit M4, SQL M4, integración SQL (concurrencia), E2E |

Adicional (requisito del enunciado, M2): panel e informes — `sp_Reporte_Dashboard`, `sp_Reporte_VentasPorEvento`, `GET /api/reportes/*`, `Admin/Index`, `Admin/Reportes`.
Estado de cada fila y nombre de cada prueba: en la matriz de trazabilidad del módulo correspondiente.

## 5. Responsabilidades

| Rol | Persona | Responsable de |
|---|---|---|
| Integrante 1 · M1 | Marco Cervantes | HU-01, 02, 03, 07, 08; CRUD de usuarios (con eliminación lógica), categorías y locaciones |
| Integrante 2 · M2 | Donnkan Cervantes Peraza | HU-04, 05, 06, 09, 10, 11; CRUD de eventos y tipos de entrada; **base de datos general** (creación, 9 tablas, PK/FK, restricciones, índices, funciones, datos iniciales, script maestro, modelo de datos) |
| Integrante 3 · M3 | Jonathan Umaña | HU-12, 13, 14; CRUD de pedidos (crear, consultar, actualizar método de pago, cancelación lógica); vencimiento de reservas |
| Integrante 4 · M4 | Brodie Hernandez | HU-15 a HU-19; CRUD de pagos (registrar, consultar, aprobar/rechazar = baja lógica) y de entradas (emitir, consultar, nominar, registrar ingreso) |
| Integración | Uno de los cuatro, designado por el equipo | Núcleo, `develop`, revisión de PR que tocan el núcleo, verificación conjunta (§15) |

**CRUD por integrante (requisito del enunciado):**

| Integrante | Create | Read | Update | Delete / baja lógica |
|---|---|---|---|---|
| M1 | `sp_Usuario_Crear`, `sp_Categoria_Crear`, `sp_Locacion_Crear` | `_Listar`, `_ObtenerPorId` | `_Actualizar`, `_CambiarEstado`, `_CambiarPassword` | `sp_Usuario_Eliminar` (Inactivo), `sp_Categoria_Eliminar`, `sp_Locacion_Eliminar` (físico o `Activo = 0`) |
| M2 | `sp_Evento_Crear`, `sp_TipoEntrada_Crear` | `sp_Evento_Listar`, `_ObtenerPorId`, `sp_TipoEntrada_ListarPorEvento` | `sp_Evento_Actualizar`, `_CambiarEstado`, `sp_TipoEntrada_Actualizar` | `sp_Evento_Eliminar` (sin pedidos; si no, Cancelado), `sp_TipoEntrada_Eliminar` (físico o `Activo = 0`) |
| M3 | `sp_Pedido_Crear` | `_ListarPorUsuario`, `_Listar`, `_ObtenerPorId` | `sp_Pedido_ActualizarMetodoPago` | `sp_Pedido_Cancelar`, `sp_Pedido_LiberarVencidos` (lógicas: Cancelado/Vencido; nunca se borra) |
| M4 | `sp_Pago_Registrar` (y emisión de entradas en `sp_Pago_Aprobar`) | `sp_Pago_Listar`, `_ObtenerPorId`, `_ListarPorPedido`, `sp_Entrada_*Listar*`, `_ObtenerPorId` | `sp_Pago_Aprobar`, `sp_Entrada_Transferir`, `sp_Entrada_RegistrarIngreso` | `sp_Pago_Rechazar` (Rechazado); entradas `Anulada` al cancelar el evento |

## 6. Contratos compartidos

### 6.1 Formato único de respuesta del API — `RespuestaApi<T>`
```json
{ "exito": true, "codigo": 200, "mensaje": null, "datos": { … }, "errores": null, "codigoError": null, "fechaUtc": "2026-11-04T20:00:00Z" }
```
- Los controladores devuelven `Ok(datos)` / `CreatedAtAction(...)`; el filtro global `RespuestaApiFiltro` lo envuelve. Los archivos (`File`) no se envuelven.
- Operaciones sin datos: `Ok(RespuestaApi.Mensaje("…"))`.
- **Errores** (mismo sobre, `exito: false`): producidos por `ManejoExcepcionesMiddleware`, por la validación automática de modelos (`errores` por campo) y por `UseStatusCodePages` (401/403/404 sin cuerpo).
- La Web lee el sobre en `ClienteApi` y entrega `ResultadoApi<T>` a las páginas; `PaginaBase.AgregarErrores` coloca `errores` en cada campo del formulario.

### 6.2 Errores y códigos
| Excepción de dominio | HTTP | Origen típico |
|---|---|---|
| `ReglaNegocioException` | 400 | Reglas o `THROW 50001–50399` |
| `AutenticacionException` | 401 | Credenciales o cuenta no habilitada |
| `AccesoDenegadoException` | 403 | `THROW 50403` o recurso de otro usuario |
| `RecursoNoEncontradoException` | 404 | `THROW 50404` |
| `ConflictoException` | 409 | `THROW 50409`, duplicados, falta de cupo |
| cualquier otra | 500 | Se registra en el log; el mensaje al cliente es genérico (no expone detalles) |

`TraductorErroresSql` (DA, núcleo) convierte `SqlException.Number` en estas excepciones. Rangos por módulo para nuevos `THROW` de reglas: M1 50001–50010, M2 50011–50019, M3 50020–50039, M4 50040–50059 (los existentes ya siguen esta convención); 50400/50403/50404/50409 son comunes.

### 6.3 Estados (constantes en `Nucleo/Constantes/Estados.cs`, idénticas en API y Web, respaldadas por `CHECK`)
| Entidad | Valores |
|---|---|
| Usuario | `Activo`, `Bloqueado`, `Inactivo` (= eliminado lógicamente) |
| Evento | `Borrador`, `Publicado`, `Agotado` (automático por trigger), `Cancelado`, `Finalizado` |
| Pedido | `PendientePago`, `EnValidacion`, `Pagado`, `Cancelado`, `Vencido` (cancelación automática por expiración, HU-14) |
| Pago | `PendienteValidacion`, `Aprobado`, `Rechazado` |
| Entrada | `Valida`, `Utilizada`, `Anulada` |
| Método de pago | `SINPE Movil`, `Transferencia Bancaria` |
| Documento | `CEDULA` (0-0000-0000), `DIMEX` (11–12 dígitos), `PASAPORTE` |

### 6.4 Roles y políticas
Roles: `Superadministrador`, `AdministradorEventos`, `AuditorPagos`, `ValidadorAcceso`, `Comprador`.
Combinaciones (constantes en `Roles`, usadas en `[Authorize(Roles = …)]` y en las políticas de la Web): `GestionEventos` (Super + AdminEventos), `Auditoria` (Super + Auditor), `ControlAcceso` (Super + Validador), `Supervision` (Super + AdminEventos + Auditor), `Personal` (todos menos Comprador).

### 6.5 Convenciones de nombres
- Procedimientos `sp_<Entidad>_<Accion>`; vistas `vw_`; funciones `fn_`; triggers `trg_<Tabla>_<Propósito>`; restricciones `PK_`, `FK_<Hijo>_<Padre>`, `UQ_`, `CK_`, `DF_`, índices `IX_`.
- C#: `<Entidad>Request` (entrada), `<Entidad>Response` (salida), `I<Entidad>DA`, `I<Entidad>Flujo`, `I<Entidad>Reglas`, `I<Entidad>Controller`, `I<Entidad>Servicio` (Web).
- Rutas REST en minúscula y plural: `/api/<recurso>`, acciones como sub-recurso (`/estado`, `/cancelacion`, `/aprobacion`).
- Cada módulo registra sus dependencias en su `ModuloNRegistro.cs`; `Program.cs` solo encadena `AgregarNucleo().AgregarModulo1Usuarios()…AgregarModulo4PagosQR()`.

### 6.6 Contrato de formulario M2 → M3
`Eventos/Detalle` (M2) hace `POST /Compras/Resumen` (M3) con: `Pedido.EventoId`, `Pedido.MetodoPago`, `Pedido.Lineas[i].TipoEntradaId`, `Pedido.Lineas[i].Cantidad`, más el token antifalsificación. Cambiar estos nombres requiere acuerdo de M2 y M3.

## 7. Modelos y DTO

| Módulo | Entrada (Request) | Salida (Response) |
|---|---|---|
| Núcleo | — | `RespuestaApi<T>`, `ResultadoEliminacion` (API); `ResultadoApi<T>`, `SinContenido`, `ArchivoDescargado` (Web) |
| M1 | `RegistroCompradorRequest`, `LoginRequest`, `UsuarioCrearRequest`, `UsuarioActualizarRequest`, `CambiarEstadoUsuarioRequest`, `CambiarPasswordRequest`, `CategoriaRequest`, `LocacionRequest` | `LoginResponse`, `UsuarioResponse`, `CategoriaResponse`, `LocacionResponse` |
| M2 | `EventoRequest` (`CodigoReferencia` opcional), `CambiarEstadoEventoRequest`, `TipoEntradaRequest`, `FiltroEventos` (`Busqueda`, `CategoriaId`, `LocacionId`, `Ciudad`, `FechaDesde`, `FechaHasta`, `SoloCartelera`, `SoloVigentes`), `FiltroReporteVentas` | `EventoResponse`, `TipoEntradaResponse` (con `Disponible`), `DashboardResponse`, `VentaEventoResponse` |
| M3 | `PedidoRequest` + `PedidoLineaRequest`, `ActualizarMetodoPagoRequest`, `FiltroPedidos` | `CotizacionResponse`, `PedidoResponse`, `PedidoCompletoResponse` (+`PedidoDetalleResponse`) |
| M4 | `PagoRegistroRequest` (multipart, `IFormFile Comprobante`), `PagoAprobarRequest`, `PagoRechazarRequest`, `TransferenciaEntradaRequest`, `ValidacionAccesoRequest` | `PagoResponse`, `EntradaResponse`; PNG del QR; archivo del comprobante |

Los modelos de la Web replican los del API con las mismas anotaciones de validación (la Web valida primero; el API vuelve a validar siempre).

## 8. Rutas del API

Generado a partir de los atributos de los controladores. Documentación interactiva: Swagger (`/swagger`) con botón *Authorize*.

| Módulo | Método | Ruta | Autorización (roles) | Acción del controlador |
|---|---|---|---|---|
| M1 | POST | `/api/autenticacion/login` | Público | AutenticacionController.Login |
| M1 | POST | `/api/autenticacion/registro` | Público | AutenticacionController.Registro |
| M1 | GET | `/api/autenticacion/perfil` | Autenticado | AutenticacionController.Perfil |
| M1 | GET | `/api/categorias` | Público | CategoriasController.Listar |
| M1 | GET | `/api/categorias/{id}` | Público | CategoriasController.Obtener |
| M1 | POST | `/api/categorias` | GestionEventos | CategoriasController.Crear |
| M1 | PUT | `/api/categorias/{id}` | GestionEventos | CategoriasController.Actualizar |
| M1 | DELETE | `/api/categorias/{id}` | GestionEventos | CategoriasController.Eliminar |
| M1 | GET | `/api/locaciones` | Público | LocacionesController.Listar |
| M1 | GET | `/api/locaciones/{id}` | Público | LocacionesController.Obtener |
| M1 | POST | `/api/locaciones` | GestionEventos | LocacionesController.Crear |
| M1 | PUT | `/api/locaciones/{id}` | GestionEventos | LocacionesController.Actualizar |
| M1 | DELETE | `/api/locaciones/{id}` | GestionEventos | LocacionesController.Eliminar |
| M1 | GET | `/api/usuarios` | Superadministrador | UsuariosController.Listar |
| M1 | GET | `/api/usuarios/{id}` | Superadministrador | UsuariosController.Obtener |
| M1 | POST | `/api/usuarios` | Superadministrador | UsuariosController.Crear |
| M1 | PUT | `/api/usuarios/{id}` | Superadministrador | UsuariosController.Actualizar |
| M1 | PATCH | `/api/usuarios/{id}/estado` | Superadministrador | UsuariosController.CambiarEstado |
| M1 | PUT | `/api/usuarios/{id}/password` | Superadministrador | UsuariosController.CambiarPassword |
| M1 | DELETE | `/api/usuarios/{id}` | Superadministrador | UsuariosController.Eliminar |
| M2 | GET | `/api/eventos` | Público | EventosController.Listar |
| M2 | GET | `/api/eventos/{id}` | Público | EventosController.Obtener |
| M2 | POST | `/api/eventos` | GestionEventos | EventosController.Crear |
| M2 | PUT | `/api/eventos/{id}` | GestionEventos | EventosController.Actualizar |
| M2 | PATCH | `/api/eventos/{id}/estado` | GestionEventos | EventosController.CambiarEstado |
| M2 | DELETE | `/api/eventos/{id}` | GestionEventos | EventosController.Eliminar |
| M2 | GET | `/api/eventos/{id}/tipos-entrada` | Público | EventosController.ListarTiposEntrada |
| M2 | GET | `/api/reportes/dashboard` | Personal | ReportesController.Dashboard |
| M2 | GET | `/api/reportes/ventas-eventos` | Supervision | ReportesController.VentasPorEvento |
| M2 | GET | `/api/tipos-entrada/{id}` | GestionEventos | TiposEntradaController.Obtener |
| M2 | POST | `/api/tipos-entrada` | GestionEventos | TiposEntradaController.Crear |
| M2 | PUT | `/api/tipos-entrada/{id}` | GestionEventos | TiposEntradaController.Actualizar |
| M2 | DELETE | `/api/tipos-entrada/{id}` | GestionEventos | TiposEntradaController.Eliminar |
| M3 | POST | `/api/pedidos/cotizacion` | Público | PedidosController.Cotizar |
| M3 | POST | `/api/pedidos` | Autenticado | PedidosController.Crear |
| M3 | GET | `/api/pedidos/mios` | Autenticado | PedidosController.ListarMios |
| M3 | GET | `/api/pedidos` | Supervision | PedidosController.Listar |
| M3 | GET | `/api/pedidos/{id}` | Autenticado | PedidosController.Obtener |
| M3 | PATCH | `/api/pedidos/{id}/metodo-pago` | Autenticado | PedidosController.ActualizarMetodoPago |
| M3 | POST | `/api/pedidos/{id}/cancelacion` | Autenticado | PedidosController.Cancelar |
| M3 | POST | `/api/pedidos/liberacion-vencidos` | Supervision | PedidosController.LiberarVencidos |
| M4 | GET | `/api/entradas/mias` | Autenticado | EntradasController.ListarMias |
| M4 | GET | `/api/entradas/evento/{eventoId}` | Personal | EntradasController.ListarPorEvento |
| M4 | GET | `/api/entradas/{id}` | Autenticado | EntradasController.Obtener |
| M4 | GET | `/api/entradas/{id}/qr` | Autenticado | EntradasController.ObtenerQr |
| M4 | PUT | `/api/entradas/{id}/titular` | Autenticado | EntradasController.Transferir |
| M4 | POST | `/api/entradas/ingresos` | ControlAcceso | EntradasController.RegistrarIngreso |
| M4 | POST | `/api/pagos` | Autenticado | PagosController.Registrar |
| M4 | GET | `/api/pagos` | Supervision | PagosController.Listar |
| M4 | GET | `/api/pagos/mios` | Autenticado | PagosController.ListarMios |
| M4 | GET | `/api/pagos/pedido/{pedidoId}` | Autenticado | PagosController.ListarPorPedido |
| M4 | GET | `/api/pagos/{id}` | Autenticado | PagosController.Obtener |
| M4 | GET | `/api/pagos/{id}/comprobante` | Autenticado | PagosController.DescargarComprobante |
| M4 | POST | `/api/pagos/{id}/aprobacion` | Auditoria | PagosController.Aprobar |
| M4 | POST | `/api/pagos/{id}/rechazo` | Auditoria | PagosController.Rechazar |

## 9. Diccionario de datos

9 tablas del diagrama ER (detalle y decisiones en `docs/modelo-datos/MODELO_DATOS.md`). Generado desde `base/02_tablas.sql`, `03_llaves_foraneas.sql` y `04_restricciones.sql`.

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

## 10. Orden de ejecución SQL

`database/orden_ejecucion.txt` es la fuente única del orden; la usan `00_EJECUTAR_TODO.sql` (`:r`), `instalar.sh`, `instalar.ps1` y el archivo consolidado.

| # | Script | Dueño | Por qué en este orden |
|---|---|---|---|
| 1–6 | `base/01_crear_base_datos` … `base/06_funciones` | M2 | Base, 9 tablas, 13 FK, restricciones, índices y funciones de disponibilidad |
| 7 | `modulo1_usuarios/M1_01_procedimientos` | M1 | Solo usa tablas |
| 8–10 | `modulo2_eventos/M2_01_vistas`, `M2_02_procedimientos`, `M2_03_triggers` | M2 | Vistas de catálogo antes que sus SP |
| 11–13 | `modulo3_pedidos/M3_01_vistas`, `M3_02_procedimientos`, `M3_03_triggers` | M3 | `vw_PedidosResumen` antes que `sp_Pedido_*` |
| 14–16 | `modulo4_pagos_qr/M4_01_vistas`, `M4_02_procedimientos`, `M4_03_triggers` | M4 | Usa `sp_Pedido_LiberarVencidos` (M3) |
| 17 | `modulo2_eventos/M2_05_reportes` | M2 | El panel lee vistas de M3 y M4 |
| 18–20 | `M1_02_datos_iniciales`, `M2_04_datos_iniciales`, `M4_04_datos_prueba` | M1, M2, M4 | Usuarios y catálogos → eventos y tipos → pedidos/pagos/entradas creados con los **mismos SP** del API |

Después: pruebas por módulo (`M1_03`, `M2_06`, `M3_04`, `M4_05`) y `validacion/99_validacion_general.sql`. Opcional: `M3_05_job_expiracion_opcional.sql` (SQL Server Agent).
Tras modificar un script, regenere `EventCR_BoleteriaPro_DB_completo.sql` con `database/consolidar.sh` o `consolidar.ps1`.

## 11. Configuración de SQL Server

- SQL Server 2019 o superior (Developer, Express, LocalDB o contenedor `mcr.microsoft.com/mssql/server:2022-latest`).
- `01_crear_base_datos.sql` crea `EventCR_BoleteriaPro_DB` con intercalación `Modern_Spanish_CI_AS` y `READ_COMMITTED_SNAPSHOT ON` (lecturas sin bloqueo; las escrituras críticas usan `UPDLOCK`).
- Autenticación: integrada de Windows (`Trusted_Connection=True`) o usuario SQL (Docker). Use `TrustServerCertificate=True` en desarrollo.
- Cadena de conexión: clave `ConnectionStrings:EventCR_BoleteriaPro_DB`, por variable de entorno `ConnectionStrings__EventCR_BoleteriaPro_DB` o *user-secrets*. **No** se guardan credenciales reales en `appsettings.json`.
- Usuario de aplicación recomendado en servidores compartidos: permisos `EXECUTE` sobre el esquema `dbo` (el API solo llama procedimientos).

## 12. Autenticación y autorización

| Aspecto | Implementación |
|---|---|
| Contraseñas | PBKDF2-SHA256, 100 000 iteraciones, sal aleatoria de 16 bytes, comparación en tiempo constante (`PasswordReglas`). Nunca en texto plano. |
| Token | JWT HS256 emitido por `POST /api/autenticacion/login` (`ServicioTokenJwt`), con `NameIdentifier`, `Name`, `Email`, `Role`, `jti`; vence en `Jwt:MinutosExpiracion` (120). |
| Clave | `Jwt:Clave` (≥ 32 caracteres) por variable de entorno `Jwt__Clave`; fuera de *Development* el API no inicia sin ella. |
| Web | Cookie `BoleteriaPro.Sesion` (HttpOnly, SameSite=Lax) que guarda el JWT; `ClienteApi` lo envía como `Authorization: Bearer`. El navegador nunca recibe el token ni archivos sin pasar por la Web. |
| Autorización | `[Authorize(Roles = …)]` en el API; carpetas protegidas por política en la Web (`/Admin/Usuarios` → Superadministrador, etc.). Los SP sensibles (`sp_Pago_Aprobar`, `sp_Entrada_RegistrarIngreso`) verifican además el rol en la base. |
| Datos personales y financieros | Un comprador solo ve **sus** pedidos, pagos, comprobantes y entradas (los Flujos comparan `UsuarioId` y lanzan 403). Comprobantes fuera de `wwwroot`, servidos solo por un endpoint autenticado y con protección contra rutas fuera de la carpeta. |
| Cuentas | `Bloqueado` e `Inactivo` no inician sesión; el mismo mensaje para correo inexistente o clave incorrecta. |

## 13. Estrategia Git

**Ramas**
```
main                      ← solo versiones entregables (Avance 1, Final). Protegida.
develop                   ← integración continua. Protegida: solo por PR con 1 revisión.
feature/modulo-1-usuarios
feature/modulo-2-eventos
feature/modulo-3-pedidos
feature/modulo-4-pagos-qr
hotfix/<descripcion>      ← desde main, se fusiona a main y develop
```

**Flujo**
1. Al iniciar: el responsable de integración sube el **núcleo** a `develop` (paquete general).
2. Cada integrante crea su rama desde `develop` y copia su paquete (`archivos/`) encima.
3. Trabaja solo dentro de sus carpetas. Antes de cada PR: `git fetch && git rebase origin/develop`, compila ambas soluciones y ejecuta `dotnet test --filter "Modulo=MN"` y sus scripts SQL de prueba.
4. PR a `develop` con la plantilla: HU cubiertas, evidencia de compilación y pruebas, capturas. Se integran **en orden M1 → M2 → M3 → M4**.
5. Al cerrar un avance: PR `develop → main` y etiqueta (`v1.0-avance1`, `v2.0-final`).

**Convención de commits** (Conventional Commits en español):
```
<tipo>(<ámbito>): <descripción en imperativo>        tipos: feat, fix, refactor, test, docs, sql, chore
feat(m3): agrega edición del método de pago (HU-13)
sql(m2): agrega índice IX_Eventos_Fecha
fix(nucleo): corrige el envoltorio de errores 404
```
Un commit = un cambio coherente; mencionar la HU cuando aplique.

**Resolución de conflictos**
- Los conflictos solo deberían aparecer en archivos del núcleo (`Program.cs`, `.csproj`, `.sln`, constantes). Regla: **el responsable de integración decide**; el módulo explica su necesidad en el PR.
- Nunca se resuelve un conflicto borrando el cambio del otro: se combinan ambas líneas (p. ej. dos `.AgregarModuloN()`), se compila y se ejecutan las pruebas antes de confirmar.
- Contratos (§6): un cambio incompatible requiere acuerdo de los módulos afectados y se hace en un PR propio con la etiqueta `contrato`.
- SQL: un módulo no edita scripts de otro; si necesita un cambio en `base/`, lo solicita a M2.

## 14. Guía de integración paso a paso

1. Descomprimir `EVENTPASS_GUIA_INTEGRACION_GENERAL.zip` y los 4 paquetes de módulo en una misma carpeta.
2. Ejecutar `ensamblar.ps1` (Windows) o `ensamblar.sh` (macOS/Linux) desde el paquete general:
   `./ensamblar.sh <carpeta-con-los-zip-descomprimidos> <carpeta-destino>` — copia `archivos/` del núcleo y de M1…M4 (sin solapamiento: cada archivo pertenece a un solo paquete) y verifica que no falte ninguno del manifiesto.
3. `dotnet build API/EventoTickets.API.sln` y `dotnet build Web/EventoTickets.Web.sln` → 0 errores.
4. `cd database && ./instalar.sh … --pruebas` → todas las filas en `OK`.
5. `dotnet test API/EventoTickets.API.sln` → todas correctas (las de integración se omiten sin `EVENTCR_PRUEBAS_SQL`).
6. Levantar API y Web y hacer el recorrido del README §4.
7. Subir el resultado a `develop` siguiendo §13.

## 15. Pruebas generales

Resumen de lo ejecutado en el entorno de construcción (detalle y pendientes en `docs/PRUEBAS.md`):

| Verificación | Resultado |
|---|---|
| Compilación API (7 proyectos) y Web (2 proyectos) | ✅ 0 errores, 0 advertencias |
| Compilación por subconjuntos: núcleo+M1 · +M2 · +M3 · +M4 | ✅ 0 errores |
| Proyecto ensamblado desde los 5 ZIP con `ensamblar.sh` | ✅ 330 archivos, sin faltantes ni solapamientos, idéntico al repositorio; compila y pasa las mismas pruebas |
| xUnit (núcleo, M1–M4) | ✅ 85 correctas · 0 fallidas · ⏳ 3 omitidas (SQL Server real / QRCoder real) |
| Scripts SQL (sintaxis T-SQL) | ✅ 27/27 analizables · ⏳ ejecución pendiente en SQL Server |
| Cruces SP ↔ Dapper (50) y rutas Web ↔ API (54/55) | ✅ sin diferencias |
| Interfaz: 82 rutas, 29 flujos, 17 pantallas a 390 px | ✅ sin excepciones (contra API simulado) |

**Pruebas conjuntas obligatorias antes de cada entrega** (en `develop`, con SQL Server real): instalar la base con `--pruebas`, `dotnet test` completo con `EVENTCR_PRUEBAS_SQL`, recorrido del README §4 con los 5 roles y la lista de `docs/PRUEBAS.md` §4.

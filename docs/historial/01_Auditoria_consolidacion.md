# 01 · Informe de auditoría de recursos (Fase 1)

> **Documento histórico** de la primera fase (consolidación de los recursos). Los nombres vigentes son: base `EventCR_BoleteriaPro_DB`, estado de usuario `Bloqueado`, pedido `EnValidacion`, pago `PendienteValidacion`, entrada `Valida`. La estructura modular actual está en `docs/00_GUIA_INTEGRACION_GENERAL.md`.

Fecha: noviembre 2026 · Alcance: los 6 recursos entregados para consolidar el proyecto final de SC-701.

## 1. Resumen ejecutivo

| # | Recurso | Qué es realmente | Valor para el proyecto |
|---|---|---|---|
| A | `Proyecto.md` | Enunciado del profesor (SC-701) | **Fuente de requisitos** — prioridad 1 |
| F | `diagramaER.pdf` | Modelo de 9 entidades (dbdiagram.io) | **Fuente de la BD** — prioridad 2 |
| C | `stitch_eventpass_management_platform.zip` | 17 pantallas HTML + Tailwind + `DESIGN.md` | **Identidad visual** — prioridad 3 |
| B | `EventoTickets-main.zip` | Esqueleto vacío de la primera entrega | Solo nombres y estructura de capas |
| D/E | `boleter_apro.zip` | **Es el prototipo de Google AI Studio** (React + Vite) | Flujos, tipos y reglas de interfaz (referencia) |

**Hallazgo principal:** *BoleteriaPro* y el *prototipo de Google AI Studio* son el **mismo recurso**. `metadata.json` declara "BoleteríaPro", el README es el de "Run and deploy your AI Studio app", y `public/assets/aistudio/` contiene las 17 pantallas de Stitch **byte a byte idénticas** (verificado por hash MD5). No se recibió otro código de AI Studio.

## 2. Requisitos extraídos de `Proyecto.md`

**Tecnologías obligatorias:** .NET 8+, C#, Web API basada en controladores, Dapper o EF, Swagger, Razor Pages o React, SOLID y Clean Architecture.

**Arquitectura obligatoria:** Abstracciones, DA, Flujo, API (requeridos); Servicios (solo si se consumen servicios externos); Reglas (opcional); interfaz web en **solución aparte** con su propio proyecto Abstracciones.

**Requisitos no funcionales (API):** interfaz para toda clase no estática; atributos de origen de parámetros (`FromBody`, `FromRoute`, `FromQuery`…); verbos HTTP correctos; validaciones por anotaciones; rutas en cada endpoint; respuestas JSON por action methods; códigos HTTP correctos; métodos asíncronos; inyección de dependencias (sin `new` de implementaciones); configuración en `appsettings`.

**Requisitos no funcionales (Web):** solo presenta y solicita cambios al API por HTTP; no transforma ni aplica lógica de negocio; validaciones por anotaciones (tipo, longitud, formato, requerido, reglas); diseño responsivo acorde al prototipo; **Tag Helpers**; vistas separadas por acción; métodos asíncronos.

**Base de datos:** relacional, 3FN, **procedimientos almacenados**, tablas relacionadas.

**Entrega:** código en GitHub Classroom, debe **compilar sin errores**; 100 % de las historias de usuario terminadas (API + interfaz que invoca el API).

**Temas a aplicar (semanas 2-11):** request/response/headers, parámetros y restricciones de ruta, archivos estáticos, IActionResult, validaciones personalizadas, layouts, ViewData, vistas parciales (con ViewData), anidadas, FromServices, ciclos de vida DI, excepciones personalizadas, Tag Helpers, IConfiguration, listas con búsqueda y orden, cadenas de conexión, CRUD y procedimientos almacenados.

> **Nota:** el documento IEEE del anteproyecto (historias de usuario y casos de uso) **no fue incluido**. El mapeo de funcionalidades se hizo a partir del ER y de las pantallas de Stitch; el equipo debe contrastarlo con sus historias (ver `05_Funcionalidades.md`).

## 3. Auditoría por recurso

### B · Primera entrega (`EventoTickets-main.zip`)

| Elemento | Estado encontrado |
|---|---|
| `EventoTickets.slnx` | Referencia `EventoTickets/EventoTickets.csproj`, pero el archivo real es `EventoTickets.API.csproj` → la solución **no carga el proyecto**. Además `.slnx` requiere SDK 9.0.200+ (el curso exige .NET 8). |
| `EventoTickets.API` | Plantilla vacía de .NET 8 (Swashbuckle 6.6.2), sin controladores; `.http` apunta a `/weatherforecast`. Puerto 5238. |
| `EventoTickets.Abstracciones`, `.DA`, `.Flujo` | Proyectos **.NET Framework 4.8.1** estilo antiguo (solo Windows) con `Class1.cs` vacíos → **incompatibles** con un API .NET 8 y fuera de la tecnología exigida. |
| Modelos, controladores, BD, interfaz | No existen. |

**Conclusión:** no hay código funcional que reutilizar. Se conservan los **nombres** (`EventoTickets.*`), la división en capas, Swashbuckle 6.6.2 y el puerto 5238; los proyectos se recrean en .NET 8 (justificación técnica: incompatibilidad de framework).

### C · Stitch (`stitch_eventpass_management_platform.zip`)

17 pantallas con Tailwind (CDN) y un `DESIGN.md` con tokens: paleta *surface/secondary #0051d5*, tipografías **Plus Jakarta Sans** y **JetBrains Mono**, radios, espaciado e iconos **Material Symbols**. Marca: **BoleteríaPro**.

| Pantalla Stitch | Ruta en la Web |
|---|---|
| Catálogo de eventos | `/` (`Pages/Index`) |
| Detalle del evento y selección de entradas | `/Eventos/Detalle/{id}` |
| Inicio de sesión · Registro de comprador | `/Cuenta/Login` · `/Cuenta/Registro` |
| Checkout y pasarela de pago | `/Compras/Checkout/{pedidoId}` (adaptado a pago manual) |
| Registro de pago manual | `/Compras/RegistrarPago/{pedidoId}` |
| Confirmación de pago exitoso | `/Compras/Confirmacion/{pedidoId}` (también muestra "en verificación" y "vencido") |
| Mis pedidos | `/MisPedidos` (+ "Mis Pagos" del menú: `/MisPagos`) |
| Mis entradas digitales | `/MisEntradas` |
| Transferencia de entrada digital | `/MisEntradas/Transferir/{id}` |
| Panel principal (administración) | `/Admin` |
| Gestión de usuarios y roles · Crear/editar usuario RBAC | `/Admin/Usuarios` · `Crear` · `Editar/{id}` |
| Crear y editar evento | `/Admin/Eventos/Crear` · `Editar/{id}` |
| Gestión de localidades y tipos de entrada | `/Admin/TiposEntrada/{eventoId}` + Crear/Editar/Eliminar |
| Detalle y validación de pago | `/Admin/Pagos/Detalle/{id}` |
| Mapa de silletería interactiva | **Descartada** (ver §5) |

Observaciones: dos capturas (`gestión de usuarios y roles`, `transferencia de entrada`) son collages de otras pantallas, pero su `code.html` es válido y se usó como referencia. Los logos e imágenes apuntan a `lh3.googleusercontent.com`; el logo se reemplazó por un SVG local.

### D/E · BoleteriaPro = prototipo de Google AI Studio (`boleter_apro.zip`)

| Aspecto | Hallazgo |
|---|---|
| Tecnología | React 19 + Vite 8 + Tailwind 4 + TypeScript; dependencias `@google/genai` y `express` **declaradas pero no usadas**. |
| Arquitectura | SPA de un solo componente `App.tsx` con navegación por estado (`ScreenId`), sin router ni backend. |
| Datos | `mockData.ts` en memoria; nada persiste. Moneda COP. |
| Valor real | 17 pantallas transcritas a React con sus **flujos** (catálogo → detalle → checkout → confirmación → mis entradas → transferencia; panel → validación de pago) y **tipos** (`EventItem`, `TicketTier`, `OrderItem`, `DigitalTicket`, `AdminUser`, `ManualPayment`) útiles para definir modelos y estados. |

## 4. Decisiones de arquitectura derivadas

1. **Interfaz: Razor Pages** (no React). El enunciado exige *Tag Helpers*, *vistas separadas por acción* y *validaciones por anotaciones en los modelos* de la Web, conceptos propios de Razor y de los temas de las semanas 5-10. El HTML de Stitch se porta directamente a `.cshtml`. El código React del prototipo se usa como **referencia de flujos y tipos**, no se copia.
2. **Acceso a datos: Dapper + procedimientos almacenados** (el enunciado exige SP; Dapper los invoca sin ORM adicional).
3. **Motor: SQL Server.** El profesor no fija motor; se eligió SQL Server por los temas del curso (cadenas de conexión, procedimientos almacenados) y por ser el entorno habitual del equipo. Si el profesor indica otro motor, solo cambian `database/` y la capa DA.
4. **Proyecto Servicios: no se crea.** Es requerido "solo en caso de consumir servicios externos" y el sistema no consume ninguno (los pagos son manuales, ver §5).
5. **Autenticación:** el API emite **JWT**; la Web guarda el token en una cookie cifrada y lo reenvía como `Bearer`.

## 5. Matriz de decisión

| Recurso | Elemento | Decisión | Justificación |
|---|---|---|---|
| Primera entrega | Nombres `EventoTickets.*` y separación en capas | **Conservar** | Continuidad con la primera entrega y el repositorio de Classroom. |
| Primera entrega | Proyectos Abstracciones/DA/Flujo (.NET Framework 4.8.1) | **Refactorizar (recrear en .NET 8)** | Incompatibles con un API .NET 8; el curso exige .NET 8. |
| Primera entrega | `EventoTickets.slnx` | **Descartar** → `.sln` | Ruta de proyecto errónea; `.slnx` necesita SDK 9. |
| Primera entrega | `Class1.cs`, `weatherforecast` en `.http` | **Descartar** | Código de plantilla sin función. |
| Primera entrega | Swashbuckle 6.6.2, puerto 5238 | **Conservar** | Siguen siendo válidos. |
| Stitch | Tokens de diseño (colores, fuentes, radios, espaciado) | **Integrar** | `wwwroot/js/tailwind.config.js` es copia del `tailwind.config` de Stitch. |
| Stitch | Barra superior, pie, barra lateral admin, tarjetas, tablas, badges, formularios | **Integrar** | Layouts `_Layout`, `_AdminLayout`, `_AuthLayout` y componentes `@apply`. |
| Stitch | 16 pantallas listadas en §3 | **Adaptar** | Datos reales del API, validaciones y acciones reales. |
| Stitch | Mapa de silletería interactiva | **Descartar** | El ER no tiene entidad Asiento; inventarla viola "no inventar entidades". `AsientoAsignado` se asigna automáticamente al emitir. |
| Stitch | Formulario de tarjeta, PSE, billeteras, 3D Secure | **Descartar** | Requeriría pasarela externa simulada; el ER modela pagos manuales verificados por un auditor. |
| Stitch | Matriz granular de permisos (toggles) | **Adaptar (solo lectura)** | El ER guarda un único `Rol` por usuario; no hay tabla de permisos. La matriz se deriva del rol. |
| Stitch | Notificaciones, Apple/Google Wallet, WhatsApp, códigos promocionales, factura electrónica | **Descartar** | Sin soporte en el ER ni en los requisitos; no se dejan botones sin función. |
| AI Studio | Flujo de compra y estados de pedido/pago/entrada | **Recuperar** | Base de los estados `PendientePago → EnVerificacion → Pagado`, `Pendiente → Aprobado/Rechazado`, `Activa/Utilizada/Anulada`. |
| AI Studio | Tipos TypeScript | **Adaptar** | Inspiraron los `*Response` de C#, ajustados a las columnas del ER. |
| AI Studio | Roles (Super Admin, Taquilla, Auditor, Organizador) | **Adaptar** | Se usaron los 5 perfiles RBAC de Stitch, que coinciden con las FK del ER (`AuditorUsuarioId`, `ValidadorPuertaUsuarioId`). |
| AI Studio | `mockData.ts`, navegación por estado, selector de pantallas de desarrollo | **Descartar** | Datos simulados y navegación incompatibles con un sistema real con BD. |
| AI Studio | `@google/genai`, `express`, `.env` de Gemini | **Descartar** | No se usan en el código. |
| BoleteriaPro | Assets `aistudio/*.html` | **Descartar** | Duplicados exactos de Stitch. |
| Diagrama ER | 9 entidades, atributos, tipos, NN, únicos y 13 relaciones | **Implementar tal cual** | Ver `03_BaseDeDatos.md`. |

## 6. Problemas técnicos encontrados y cómo se resolvieron

| Problema | Resolución |
|---|---|
| Primera entrega no compila como solución .NET 8 | Proyectos recreados en .NET 8 con referencias correctas. |
| ER guarda `EventoId` de forma redundante en `Pedidos` y `Entradas` | 2 triggers garantizan que pedido, tipo de entrada y entrada pertenezcan al mismo evento. |
| Compras simultáneas podían sobrevender | `UPDATE` condicional con `UPDLOCK` + `CHECK (Comprometida + Vendida <= Inicial)`. |
| Comprobantes de pago reutilizables | SHA-256 del archivo (`HashComprobante`) y par banco/referencia no pueden repetirse en pagos no rechazados. |
| Reservas que nunca se pagan bloquean inventario | `FechaVencimientoReserva` + `sp_Pedido_LiberarVencidos` (se ejecuta al crear pedidos y registrar pagos). |
| Moneda COP en el prototipo | Colones (CRC) e IVA 13 % configurables en `appsettings` (`Ventas`). |

# Pruebas · EventCR / BoleteríaPro

## 1. Entorno de verificación (y sus límites)

Las pruebas se ejecutaron en un entorno Linux con **.NET SDK 8.0**, **sin acceso a NuGet y sin SQL Server**. Por eso:

- Se compiló contra paquetes locales con la **misma firma pública** de Dapper 2.1.35, Microsoft.Data.SqlClient 5.2.2, JwtBearer 8.0.10, System.IdentityModel.Tokens.Jwt 7.1.2, Swashbuckle 6.6.2, QRCoder 1.6.0 y xUnit 2.9.2. En su máquina `dotnet restore` descarga los reales.
- Las pruebas xUnit se ejecutaron con un ejecutor por reflexión equivalente a `dotnet test` (mismos atributos `Fact`, `Theory`, `InlineData`, `Trait`, `Skip`). Las que dependen de una librería real (QRCoder) o de SQL Server **se omitieron** y se reportan como pendientes, no como aprobadas.
- Los scripts SQL se validaron **sintácticamente** (analizador T-SQL `sqlfluff`) y por **cruce automático** con el código C#, pero **no se ejecutaron en SQL Server**.
- La interfaz se ejecutó de verdad (Kestrel) contra un **API simulado** que respeta el contrato `RespuestaApi<T>`, recorriéndola con Chromium (Playwright).

## 2. Resultados ejecutados

| Prueba | Resultado |
|---|---|
| `dotnet build API/EventoTickets.API.sln` (7 proyectos, incluye Pruebas) | ✅ 0 errores, 0 advertencias |
| `dotnet build Web/EventoTickets.Web.sln` (2 proyectos, 73 vistas Razor, 52 páginas) | ✅ 0 errores, 0 advertencias |
| Compilación por **subconjuntos de módulos** (núcleo+M1 · núcleo+M1+M2 · núcleo+M1+M2+M3 · los 4) | ✅ 0 errores en API y Web (API núcleo+M2 también compila sin M1) |
| Proyecto **ensamblado desde los 5 ZIP** con `ensamblar.sh` | ✅ 330 archivos, sin faltantes ni solapamientos, idéntico al repositorio; 0 errores en API y Web; 85/85 xUnit; 82 rutas, 29 flujos y 17 pantallas móviles sin fallas |
| xUnit: núcleo 11 · M1 19 · M2 20 · M3 15 · M4 20 | ✅ **85 correctas, 0 fallidas** |
| xUnit omitidas | ⏳ 3: 2 de concurrencia contra SQL Server (M3, M4) y 1 de QR con QRCoder real (M4) |
| Análisis T-SQL de 27 scripts (todos menos `00_EJECUTAR_TODO.sql`, que usa directivas `:r` de SQLCMD) | ✅ 0 secciones no analizables |
| Cruce **procedimientos ↔ llamadas Dapper** | ✅ 50 SP definidos, 50 usados, nombres de parámetros coinciden |
| Cruce **rutas Web ↔ rutas API** | ✅ las 54 rutas que llama la Web existen en el API (55; `GET /api/autenticacion/perfil` solo se usa desde Swagger) |
| Recorrido de **82 rutas** con 5 perfiles (público, comprador, superadministrador, validador, auditor) | ✅ sin excepciones; 404 y redirecciones de autorización esperadas |
| **29 flujos** de formularios (POST, multipart, descargas) | ✅ 29/29, repetido 3 veces |
| Responsive a 390 px (17 pantallas) | ✅ sin desbordamiento horizontal |

### 2.1 Pruebas unitarias por módulo (nombres en las matrices de trazabilidad)
- **Núcleo:** filtro `RespuestaApi` (envuelve, respeta 201, no envuelve dos veces, no toca archivos); middleware (400/401/403/404/409 con el sobre de error; 500 sin exponer detalles internos); SHA-256 conocido.
- **M1:** hash PBKDF2 (semilla, clave incorrecta, sal aleatoria, hash malformado), cuentas bloqueadas/inactivas, autogestión del administrador, login con mensaje único, registro con hash y rol Comprador, validaciones del formulario de registro.
- **M2:** transiciones de estado permitidas/prohibidas (incluye Agotado), fecha pasada, código opcional/formato, cartelera pública forzada a Publicado/Agotado y vigentes, Borrador oculto, alta siempre en Borrador, no editar cancelados.
- **M3:** fórmula de cotización igual a `sp_Pedido_Crear` (2 × ₡180 000 = ₡447 480), agrupación de líneas, máximo por pedido, falta de cupo (409), tipos de otro evento, solo eventos publicados, parámetros enviados al SP, propiedad del pedido, servicio de expiración (se ejecuta solo, se desactiva con 0, sobrevive a fallos de la base).
- **M4:** comprobante obligatorio, formatos y tamaño, monto, hash y referencia normalizada, eliminación del archivo si la base rechaza el pago, propiedad de pagos/comprobantes/entradas, motivo de rechazo, lectura del QR `CODIGO|HASH`, almacenamiento fuera de `wwwroot`, rechazo de rutas fuera de la carpeta.

### 2.2 Defectos encontrados y corregidos en esta fase
| Defecto | Corrección |
|---|---|
| `ServicioArchivosComprobantes` aceptaba rutas hermanas con el mismo prefijo (`../ComprobantesFalsos/…`) | Comparación contra la raíz **con separador final**; prueba agregada |
| El núcleo Web dependía de un DTO de M4 (`ArchivoDescargado`) | DTO movido al núcleo (detectado al compilar subconjuntos) |
| `Eventos/Detalle` (M2) dependía de `PedidoRequest` (M3) | Se usa el contrato de formulario, sin clases de M3 |
| `ReporteDA` leía DTO de M3/M4 en el panel | DTO propios `DashboardPago`, `DashboardPedido` |
| Mensaje "suspender" con el estado renombrado a Bloqueado | Texto corregido |

## 3. Pruebas de base de datos (escritas, pendientes de ejecutar en SQL Server)

| Script | Casos |
|---|---|
| `modulo1_usuarios/M1_03_pruebas.sql` | 12: CRUD de usuario con bloqueo y eliminación lógica, correo/documento duplicados, hash, CHECK de estados, inexistente, CRUD de categoría y locación, desactivación con eventos, duplicado, capacidad ≥ aforo |
| `modulo2_eventos/M2_06_pruebas.sql` | 15: crear y publicar con código autogenerado, editar conserva el código, Agotado automático y regreso, `fn_DisponibilidadEvento`, editar tipo, Borrador fuera de cartelera, eliminar sin pedidos, fecha pasada, categoría inexistente, publicar sin tipos, evento con pedidos, aforo, tipo duplicado, CHECK anti-sobreventa, disponibilidad calculada |
| `modulo3_pedidos/M3_04_pruebas.sql` | 14: crear compromete cupo y congela precios, actualizar método, cancelar libera, vencimiento libera **una sola vez**, sobreventa, límite, otro evento, usuario bloqueado, evento no publicado, pagado no se cancela ni edita, pedidos ajenos, sin `DELETE` físico, estados válidos |
| `modulo4_pagos_qr/M4_05_pruebas.sql` | 14: registrar → aprobar → entradas Válidas con SHA-256 → nominación → ingreso; rechazo con motivo; duplicado; monto; rol de auditor; pago ya validado; rol de validador; código inexistente; doble ingreso; entradas = unidades |
| `validacion/99_validacion_general.sql` | Inventario de objetos, conteo por tabla y 7 verificaciones de integridad |

Las pruebas de éxito corren dentro de una transacción que se revierte; las que esperan un error crean lo necesario en su propia transacción, que el `CATCH` revierte. **No dejan cambios.**

## 4. Lista de verificación en la máquina del equipo

- [ ] `dotnet build` de ambas soluciones con los paquetes reales (0 errores).
- [ ] `dotnet test API/EventoTickets.API.sln` → 86 correctas (incluye la de QR) y 2 omitidas sin `EVENTCR_PRUEBAS_SQL`.
- [ ] `database/instalar.* --pruebas` → todas las filas `OK`.
- [ ] Con `EVENTCR_PRUEBAS_SQL`: `dotnet test --filter "Categoria=Integracion"` → doble reserva y doble ingreso correctas.
- [ ] Swagger: login → *Authorize* → `GET /api/reportes/dashboard` = 200; sin token `POST /api/eventos` = 401; con comprador = 403.
- [ ] Recorrido del README §4 con datos reales.
- [ ] Bajar `Ventas:MinutosReserva` a 1 → el pedido pasa a Vencido sin abrir páginas y el cupo vuelve.

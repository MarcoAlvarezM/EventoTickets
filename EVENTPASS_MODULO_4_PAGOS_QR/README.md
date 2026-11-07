# Módulo 4 · Pagos, auditoría, entradas QR y control de acceso

**Responsable:** Brodie Hernandez · **Rama:** `feature/modulo-4-pagos-qr` · **Paquete:** `EVENTPASS_MODULO_4_PAGOS_QR.zip`

## Historias de usuario
- HU-15 Registrar comprobante (SINPE Móvil o transferencia): PendienteValidacion, SHA-256, pedido → EnValidacion
- HU-16 Auditoría de pagos (voucher, monto reportado vs total)
- HU-17 Aprobar o rechazar con motivo
- HU-18 Entradas digitales con QR (estado Válida), ver y descargar
- HU-19 Validación de acceso: Utilizada, sin doble ingreso aun con solicitudes simultáneas

## CRUD del integrante
| Entidad | Create | Read | Update | Delete / baja lógica |
|---|---|---|---|---|
| Pagos | sp_Pago_Registrar (multipart, SHA-256) · POST /api/pagos · Compras/RegistrarPago | sp_Pago_Listar / _ObtenerPorId / _ListarPorPedido / _ListarPorUsuario · GET · Admin/Pagos, Detalle, MisPagos | sp_Pago_Aprobar (emite entradas) · POST /aprobacion · Admin/Pagos/Detalle | sp_Pago_Rechazar (baja lógica: Rechazado, con motivo) · POST /rechazo · Admin/Pagos/Detalle |
| Entradas | emisión dentro de sp_Pago_Aprobar (una por unidad) | sp_Entrada_ListarPorUsuario / _ObtenerPorId / _ListarPorEvento · GET (+ /qr PNG) · MisEntradas, MisEntradas/Detalle, Admin/Entradas | sp_Entrada_Transferir (nominación, QR nuevo) · PUT /titular · MisEntradas/Transferir; sp_Entrada_RegistrarIngreso (Utilizada) · POST /ingresos · Admin/Accesos | Anulada al cancelar el evento (sp_Evento_CambiarEstado, M2); no se borran |

Cada vista de lista, creación, edición, detalle y eliminación/desactivación muestra validaciones por campo (anotaciones + errores del API), mensajes de éxito/error y confirmación antes de las acciones destructivas.

## Archivos del módulo
**API:** `Abstracciones/Modulo4_PagosQR`, `DA/Modulo4_PagosQR`, `Reglas/Modulo4_PagosQR`, `Flujo/Modulo4_PagosQR`, `Servicios/Modulo4_PagosQR (ServicioArchivosComprobantes, ServicioQr)`, `API/Controllers/Modulo4_PagosQR (Pagos, Entradas)`, `API/Registro/Modulo4Registro.cs`, `Pruebas/Modulo4_PagosQR`

**Web:** `Web.Abstracciones/Modulo4_PagosQR`, `Web/Servicios/Modulo4_PagosQR`, `Web/Registro/Modulo4Registro.cs`, `Pages/Compras/RegistrarPago`, `Pages/Compras/Confirmacion`, `Pages/MisPagos/*`, `Pages/MisEntradas/* (Index, Detalle con QR, Transferir)`, `Pages/Admin/Pagos/*`, `Pages/Admin/Entradas/*`, `Pages/Admin/Accesos/*`

**SQL (`database/`):** `modulo4_pagos_qr/M4_01_vistas.sql (vw_PagosDetalle, vw_EntradasDetalle)`, `modulo4_pagos_qr/M4_02_procedimientos.sql (12 SP)`, `modulo4_pagos_qr/M4_03_triggers.sql`, `modulo4_pagos_qr/M4_04_datos_prueba.sql (pedidos, pagos y entradas de ejemplo)`, `modulo4_pagos_qr/M4_05_pruebas.sql`

**Documentación:** `docs/modulos/M4_PagosQR/` (este README, `GUIA_INTEGRACION.md`, `MATRIZ_TRAZABILIDAD.md`).
La lista exacta de archivos está en `MANIFIESTO.txt` del paquete.

## Dependencias
- **Usa:** Núcleo, M3 (`IPedidoDA`, `IPedidoServicio`, `_ResumenPedido`, `sp_Pedido_LiberarVencidos`) y, a través de M3, M2.
- **Provee:** Emisión de entradas para el resto del sistema; `IServicioArchivosComprobantes` e `IServicioQr`.

## Pruebas del módulo
```bash
dotnet test API/EventoTickets.API.sln --filter "Modulo=M4"                                   # unitarias
EVENTCR_PRUEBAS_SQL="<cadena>" dotnet test API/EventoTickets.API.sln --filter "Modulo=M4&Categoria=Integracion"
sqlcmd -S <servidor> -E -C -i database/modulo4_pagos_qr/M4_05_pruebas.sql       # pruebas SQL (todas deben decir OK)
```
Resultados obtenidos y pendientes: `MATRIZ_TRAZABILIDAD.md` y `docs/PRUEBAS.md`.

## Verificación manual
1. Reportar un pago con comprobante → pago PendienteValidacion, pedido EnValidacion.
2. Subir el mismo comprobante en otro pedido → 409.
3. Aprobar → se emiten tantas entradas *Válida* como unidades; el QR se descarga como PNG.
4. Leer dos veces el mismo QR (o ejecutar la prueba de integración) → solo un ingreso.

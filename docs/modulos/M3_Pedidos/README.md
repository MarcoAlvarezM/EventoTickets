# Módulo 3 · Pedidos, historial, cancelación y vencimiento de reservas

**Responsable:** Jonathan Umaña · **Rama:** `feature/modulo-3-pedidos` · **Paquete:** `EVENTPASS_MODULO_3_PEDIDOS_RESERVAS.zip`

## Historias de usuario
- HU-12 Generar pedido con desglose (Subtotal, Servicio, IVA) y reserva atómica
- HU-13 Historial y estado de pedidos (PendientePago, EnValidacion, Pagado, Cancelado)
- HU-14 Cancelación manual y vencimiento automático de la reserva (libera el cupo una sola vez)

## CRUD del integrante
| Entidad | Create | Read | Update | Delete / baja lógica |
|---|---|---|---|---|
| Pedidos | sp_Pedido_Crear (transacción atómica, UPDLOCK) · POST /api/pedidos · Compras/Resumen → Checkout | sp_Pedido_ListarPorUsuario / _Listar / _ObtenerPorId · GET · MisPedidos/Index, Detalle, Admin/Pedidos | sp_Pedido_ActualizarMetodoPago (solo PendientePago) · PATCH /api/pedidos/{id}/metodo-pago · MisPedidos/Editar | sp_Pedido_Cancelar (lógica: Cancelado) · POST /api/pedidos/{id}/cancelacion · MisPedidos/Detalle (con confirmación); sp_Pedido_LiberarVencidos (Vencido). **Los pedidos nunca se eliminan físicamente.** |

Cada vista de lista, creación, edición, detalle y eliminación/desactivación muestra validaciones por campo (anotaciones + errores del API), mensajes de éxito/error y confirmación antes de las acciones destructivas.

## Archivos del módulo
**API:** `Abstracciones/Modulo3_Pedidos`, `DA/Modulo3_Pedidos`, `Reglas/Modulo3_Pedidos`, `Flujo/Modulo3_Pedidos`, `Servicios/Modulo3_Pedidos (ExpiracionReservasServicio)`, `API/Controllers/Modulo3_Pedidos`, `API/Registro/Modulo3Registro.cs`, `Pruebas/Modulo3_Pedidos`

**Web:** `Web.Abstracciones/Modulo3_Pedidos`, `Web/Servicios/Modulo3_Pedidos`, `Web/Registro/Modulo3Registro.cs`, `Pages/Compras/Resumen`, `Pages/Compras/Checkout`, `Pages/MisPedidos/*`, `Pages/Admin/Pedidos/*`, `Shared: _ResumenPedido`

**SQL (`database/`):** `modulo3_pedidos/M3_01_vistas.sql (vw_PedidosResumen)`, `modulo3_pedidos/M3_02_procedimientos.sql (7 SP)`, `modulo3_pedidos/M3_03_triggers.sql`, `modulo3_pedidos/M3_04_pruebas.sql`, `modulo3_pedidos/M3_05_job_expiracion_opcional.sql`

**Documentación:** `docs/modulos/M3_Pedidos/` (este README, `GUIA_INTEGRACION.md`, `MATRIZ_TRAZABILIDAD.md`).
La lista exacta de archivos está en `MANIFIESTO.txt` del paquete.

## Dependencias
- **Usa:** Núcleo y M2 (`IEventoDA`, `ITipoEntradaDA`; en la Web `IEventoServicio`, `ITipoEntradaServicio` y el contrato de formulario de `Eventos/Detalle`).
- **Provee:** `IPedidoDA` (API) e `IPedidoServicio`, parcial `_ResumenPedido` (Web) que usa M4; `sp_Pedido_LiberarVencidos`, que también ejecuta `sp_Pago_Registrar`.

## Pruebas del módulo
```bash
dotnet test API/EventoTickets.API.sln --filter "Modulo=M3"                                   # unitarias
EVENTCR_PRUEBAS_SQL="<cadena>" dotnet test API/EventoTickets.API.sln --filter "Modulo=M3&Categoria=Integracion"
sqlcmd -S <servidor> -E -C -i database/modulo3_pedidos/M3_04_pruebas.sql       # pruebas SQL (todas deben decir OK)
```
Resultados obtenidos y pendientes: `MATRIZ_TRAZABILIDAD.md` y `docs/PRUEBAS.md`.

## Verificación manual
1. Dos navegadores compran a la vez el último cupo → uno obtiene el pedido, el otro recibe 409 (o ejecutar la prueba de integración con `EVENTCR_PRUEBAS_SQL`).
2. Bajar `Ventas:MinutosReserva` a 1 y esperar → el pedido pasa a **Vencido** sin abrir ninguna página y el cupo vuelve.
3. Cancelar un pedido → Cancelado, el cupo vuelve; el registro permanece en la base.

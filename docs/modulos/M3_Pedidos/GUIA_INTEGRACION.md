# Guía de integración · Módulo 3 (Pedidos, historial, cancelación y vencimiento de reservas)

1. **Prerrequisito:** `develop` ya contiene el núcleo (paquete general) y los módulos anteriores en el orden núcleo → M1 → M2 → M3 → M4.
2. Crear la rama: `git checkout develop && git pull && git checkout -b feature/modulo-3-pedidos`.
3. Copiar el contenido de `archivos/` del paquete en la raíz del repositorio (no sobrescribe archivos de otros módulos: cada archivo pertenece a un solo paquete).
4. Registro de dependencias: el módulo ya trae `API/EventoTickets.API/Registro/Modulo3Registro.cs` y `Web/EventoTickets.Web/Registro/Modulo3Registro.cs`; `Program.cs` (núcleo) ya los invoca. **No editar `Program.cs`.**
5. Base de datos: los scripts del módulo ya figuran en `database/orden_ejecucion.txt`; instalar con `database/instalar.*` y ejecutar las pruebas SQL del módulo.
6. Compilar ambas soluciones y ejecutar `dotnet test --filter "Modulo=M3"`.
7. Abrir PR a `develop` con: HU cubiertas, salida de compilación y de pruebas, capturas de las vistas.

## Contratos que consume
Núcleo y M2 (`IEventoDA`, `ITipoEntradaDA`; en la Web `IEventoServicio`, `ITipoEntradaServicio` y el contrato de formulario de `Eventos/Detalle`).

## Contratos que expone (no cambiar sin acuerdo)
`IPedidoDA` (API) e `IPedidoServicio`, parcial `_ResumenPedido` (Web) que usa M4; `sp_Pedido_LiberarVencidos`, que también ejecuta `sp_Pago_Registrar`.
Además usa sin modificar los contratos del núcleo: `RespuestaApi<T>`, excepciones de dominio, `Roles`, `Estados`, `ClienteApi`/`ResultadoApi<T>`, `PaginaBase`.

## Lista de verificación antes del PR
- [ ] Solo se modificaron archivos de las carpetas del módulo.
- [ ] `dotnet build` de API y Web con 0 errores y 0 advertencias.
- [ ] Pruebas xUnit del módulo correctas; pruebas SQL del módulo en `OK`.
- [ ] Ninguna página accede a SQL; todo pasa por el API.
- [ ] No hay contraseñas, cadenas de conexión ni claves en el código ni en `appsettings.json`.
- [ ] Mensajes de error y éxito visibles en cada formulario; confirmación en acciones destructivas.

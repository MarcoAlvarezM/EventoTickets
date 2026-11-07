# Guía de integración · Módulo 4 (Pagos, auditoría, entradas QR y control de acceso)

1. **Prerrequisito:** `develop` ya contiene el núcleo (paquete general) y los módulos anteriores en el orden núcleo → M1 → M2 → M3 → M4.
2. Crear la rama: `git checkout develop && git pull && git checkout -b feature/modulo-4-pagos-qr`.
3. Copiar el contenido de `archivos/` del paquete en la raíz del repositorio (no sobrescribe archivos de otros módulos: cada archivo pertenece a un solo paquete).
4. Registro de dependencias: el módulo ya trae `API/EventoTickets.API/Registro/Modulo4Registro.cs` y `Web/EventoTickets.Web/Registro/Modulo4Registro.cs`; `Program.cs` (núcleo) ya los invoca. **No editar `Program.cs`.**
5. Base de datos: los scripts del módulo ya figuran en `database/orden_ejecucion.txt`; instalar con `database/instalar.*` y ejecutar las pruebas SQL del módulo.
6. Compilar ambas soluciones y ejecutar `dotnet test --filter "Modulo=M4"`.
7. Abrir PR a `develop` con: HU cubiertas, salida de compilación y de pruebas, capturas de las vistas.

## Contratos que consume
Núcleo, M3 (`IPedidoDA`, `IPedidoServicio`, `_ResumenPedido`, `sp_Pedido_LiberarVencidos`) y, a través de M3, M2.

## Contratos que expone (no cambiar sin acuerdo)
Emisión de entradas para el resto del sistema; `IServicioArchivosComprobantes` e `IServicioQr`.
Además usa sin modificar los contratos del núcleo: `RespuestaApi<T>`, excepciones de dominio, `Roles`, `Estados`, `ClienteApi`/`ResultadoApi<T>`, `PaginaBase`.

## Lista de verificación antes del PR
- [ ] Solo se modificaron archivos de las carpetas del módulo.
- [ ] `dotnet build` de API y Web con 0 errores y 0 advertencias.
- [ ] Pruebas xUnit del módulo correctas; pruebas SQL del módulo en `OK`.
- [ ] Ninguna página accede a SQL; todo pasa por el API.
- [ ] No hay contraseñas, cadenas de conexión ni claves en el código ni en `appsettings.json`.
- [ ] Mensajes de error y éxito visibles en cada formulario; confirmación en acciones destructivas.

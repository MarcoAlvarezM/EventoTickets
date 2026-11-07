# Guía de integración · Módulo 2 (Eventos, cartelera, tipos de entrada, disponibilidad y base de datos general)

1. **Prerrequisito:** `develop` ya contiene el núcleo (paquete general) y los módulos anteriores en el orden núcleo → M1 → M2 → M3 → M4.
2. Crear la rama: `git checkout develop && git pull && git checkout -b feature/modulo-2-eventos`.
3. Copiar el contenido de `archivos/` del paquete en la raíz del repositorio (no sobrescribe archivos de otros módulos: cada archivo pertenece a un solo paquete).
4. Registro de dependencias: el módulo ya trae `API/EventoTickets.API/Registro/Modulo2Registro.cs` y `Web/EventoTickets.Web/Registro/Modulo2Registro.cs`; `Program.cs` (núcleo) ya los invoca. **No editar `Program.cs`.**
5. Base de datos: los scripts del módulo ya figuran en `database/orden_ejecucion.txt`; instalar con `database/instalar.*` y ejecutar las pruebas SQL del módulo.
6. Compilar ambas soluciones y ejecutar `dotnet test --filter "Modulo=M2"`.
7. Abrir PR a `develop` con: HU cubiertas, salida de compilación y de pruebas, capturas de las vistas.

## Contratos que consume
Núcleo. En la Web usa `ICategoriaServicio`/`ILocacionServicio` de M1 para formularios y filtros. En el API es independiente de M1 (las validaciones de catálogo están en los SP).

## Contratos que expone (no cambiar sin acuerdo)
`IEventoDA`, `ITipoEntradaDA` (API) y `IEventoServicio`, `ITipoEntradaServicio` (Web) que usa M3; el **contrato de formulario** hacia `Compras/Resumen` (guía §6.6); toda la estructura de la base.
Además usa sin modificar los contratos del núcleo: `RespuestaApi<T>`, excepciones de dominio, `Roles`, `Estados`, `ClienteApi`/`ResultadoApi<T>`, `PaginaBase`.

## Lista de verificación antes del PR
- [ ] Solo se modificaron archivos de las carpetas del módulo.
- [ ] `dotnet build` de API y Web con 0 errores y 0 advertencias.
- [ ] Pruebas xUnit del módulo correctas; pruebas SQL del módulo en `OK`.
- [ ] Ninguna página accede a SQL; todo pasa por el API.
- [ ] No hay contraseñas, cadenas de conexión ni claves en el código ni en `appsettings.json`.
- [ ] Mensajes de error y éxito visibles en cada formulario; confirmación en acciones destructivas.

# Módulo 2 · Eventos, cartelera, tipos de entrada, disponibilidad y base de datos general

**Responsable:** Donnkan Cervantes Peraza · **Rama:** `feature/modulo-2-eventos` · **Paquete:** `EVENTPASS_MODULO_2_EVENTOS.zip`

## Historias de usuario
- HU-04 Crear y publicar eventos (fecha futura, categoría y locación activas, CodigoReferencia único autogenerado)
- HU-05 Editar y cambiar estado (Borrador, Publicado, Agotado, Cancelado, Finalizado)
- HU-06 Cartelera (solo Publicado/Agotado; filtros por categoría, ciudad, fecha y nombre)
- HU-09 Configurar tipos de entrada
- HU-10 Actualizar precio/cupo
- HU-11 Disponibilidad en tiempo real
- Base de datos general: creación, 9 tablas, relaciones, PK/FK, restricciones, índices, funciones, datos iniciales, script maestro y modelo de datos
- Panel e informes (requisito del enunciado)

## CRUD del integrante
| Entidad | Create | Read | Update | Delete / baja lógica |
|---|---|---|---|---|
| Eventos | sp_Evento_Crear (siempre Borrador; código EVT-AAAA-00000 si se omite) · POST · Admin/Eventos/Crear | sp_Evento_Listar / _ObtenerPorId · GET · Index, Detalle, cartelera | sp_Evento_Actualizar / _CambiarEstado · PUT, PATCH · Editar, Detalle (publicar, finalizar, cancelar) | sp_Evento_Eliminar (solo sin pedidos; si no, se cancela) · DELETE · Eliminar |
| Tipos de entrada | sp_TipoEntrada_Crear · POST · Crear | sp_TipoEntrada_ListarPorEvento / _ObtenerPorId · GET · Index, Detalle | sp_TipoEntrada_Actualizar · PUT · Editar | sp_TipoEntrada_Eliminar (física; Activo = 0 si tiene ventas) · DELETE · Eliminar |

Cada vista de lista, creación, edición, detalle y eliminación/desactivación muestra validaciones por campo (anotaciones + errores del API), mensajes de éxito/error y confirmación antes de las acciones destructivas.

## Archivos del módulo
**API:** `Abstracciones/Modulo2_Eventos`, `DA/Modulo2_Eventos`, `Reglas/Modulo2_Eventos`, `Flujo/Modulo2_Eventos`, `API/Controllers/Modulo2_Eventos (Eventos, TiposEntrada, Reportes)`, `API/Registro/Modulo2Registro.cs`, `Pruebas/Modulo2_Eventos`

**Web:** `Web.Abstracciones/Modulo2_Eventos`, `Web/Servicios/Modulo2_Eventos`, `Web/Registro/Modulo2Registro.cs`, `Pages/Index (cartelera)`, `Pages/Eventos/*`, `Pages/Admin/Eventos/*`, `Pages/Admin/TiposEntrada/*`, `Pages/Admin/Reportes/*`, `Pages/Admin/Index (panel)`, `Shared: _TarjetaEvento, _FormEvento, _FormTipoEntrada`, `docs/modelo-datos/`

**SQL (`database/`):** `base/01…06 (estructura general)`, `modulo2_eventos/M2_01_vistas.sql`, `modulo2_eventos/M2_02_procedimientos.sql (11 SP)`, `modulo2_eventos/M2_03_triggers.sql (Agotado automático)`, `modulo2_eventos/M2_04_datos_iniciales.sql`, `modulo2_eventos/M2_05_reportes.sql`, `modulo2_eventos/M2_06_pruebas.sql`, `00_EJECUTAR_TODO.sql, orden_ejecucion.txt, EventCR_BoleteriaPro_DB_completo.sql, instalar.sh/.ps1, consolidar.sh/.ps1`

**Documentación:** `docs/modulos/M2_Eventos/` (este README, `GUIA_INTEGRACION.md`, `MATRIZ_TRAZABILIDAD.md`).
La lista exacta de archivos está en `MANIFIESTO.txt` del paquete.

## Dependencias
- **Usa:** Núcleo. En la Web usa `ICategoriaServicio`/`ILocacionServicio` de M1 para formularios y filtros. En el API es independiente de M1 (las validaciones de catálogo están en los SP).
- **Provee:** `IEventoDA`, `ITipoEntradaDA` (API) y `IEventoServicio`, `ITipoEntradaServicio` (Web) que usa M3; el **contrato de formulario** hacia `Compras/Resumen` (guía §6.6); toda la estructura de la base.

## Pruebas del módulo
```bash
dotnet test API/EventoTickets.API.sln --filter "Modulo=M2"                                   # unitarias
EVENTCR_PRUEBAS_SQL="<cadena>" dotnet test API/EventoTickets.API.sln --filter "Modulo=M2&Categoria=Integracion"
sqlcmd -S <servidor> -E -C -i database/modulo2_eventos/M2_06_pruebas.sql       # pruebas SQL (todas deben decir OK)
```
Resultados obtenidos y pendientes: `MATRIZ_TRAZABILIDAD.md` y `docs/PRUEBAS.md`.

## Verificación manual
1. Crear un evento sin código → queda Borrador con `EVT-<año>-<id>`.
2. Publicarlo sin tipos de entrada → error; agregar un tipo y publicar → aparece en la cartelera.
3. Vender todo el cupo → el evento pasa a **Agotado** solo; liberar una reserva → vuelve a Publicado.
4. `00_EJECUTAR_TODO.sql` instala desde cero sin errores.

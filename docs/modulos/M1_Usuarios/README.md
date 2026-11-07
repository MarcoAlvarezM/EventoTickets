# Módulo 1 · Usuarios, autenticación, categorías y locaciones

**Responsable:** Marco Alvarez · **Rama:** `feature/modulo-1-usuarios` · **Paquete:** `EVENTPASS_MODULO_1_USUARIOS.zip`

## Historias de usuario
- HU-01 Registro de comprador
- HU-02 Inicio y cierre de sesión
- HU-03 Gestión de usuarios (rol, Activo/Bloqueado, eliminación lógica)
- HU-07 Catálogo de categorías
- HU-08 Catálogo de locaciones

## CRUD del integrante
| Entidad | Create | Read | Update | Delete / baja lógica |
|---|---|---|---|---|
| Usuarios | sp_Usuario_Crear · POST /api/usuarios · Admin/Usuarios/Crear (y registro público HU-01) | sp_Usuario_Listar / _ObtenerPorId · GET · Index, Detalle | sp_Usuario_Actualizar / _CambiarEstado / _CambiarPassword · PUT, PATCH · Editar, Bloquear/Desbloquear, Password | sp_Usuario_Eliminar (Estado = Inactivo) · DELETE · Eliminar |
| Categorías | sp_Categoria_Crear · POST · Crear | sp_Categoria_Listar / _ObtenerPorId · GET · Index, Detalle | sp_Categoria_Actualizar · PUT · Editar | sp_Categoria_Eliminar (física; Activo = 0 si tiene eventos) · DELETE · Eliminar |
| Locaciones | sp_Locacion_Crear · POST · Crear | sp_Locacion_Listar / _ObtenerPorId · GET · Index, Detalle | sp_Locacion_Actualizar · PUT · Editar | sp_Locacion_Eliminar (física; Activo = 0 si tiene eventos) · DELETE · Eliminar |

Cada vista de lista, creación, edición, detalle y eliminación/desactivación muestra validaciones por campo (anotaciones + errores del API), mensajes de éxito/error y confirmación antes de las acciones destructivas.

## Archivos del módulo
**API:** `Abstracciones/Modulo1_Usuarios`, `DA/Modulo1_Usuarios`, `Reglas/Modulo1_Usuarios`, `Flujo/Modulo1_Usuarios`, `Servicios/Modulo1_Usuarios (ServicioTokenJwt)`, `API/Controllers/Modulo1_Usuarios (Autenticacion, Usuarios, Categorias, Locaciones)`, `API/Registro/Modulo1Registro.cs`, `Pruebas/Modulo1_Usuarios`

**Web:** `Web.Abstracciones/Modulo1_Usuarios`, `Web/Servicios/Modulo1_Usuarios`, `Web/Registro/Modulo1Registro.cs`, `Pages/Cuenta/*`, `Pages/Admin/Usuarios/*`, `Pages/Admin/Categorias/*`, `Pages/Admin/Locaciones/*`, `Shared: _FormUsuario, _FormCategoria, _FormLocacion, _SelectorRol, _MatrizPermisos`

**SQL (`database/`):** `modulo1_usuarios/M1_01_procedimientos.sql (18 SP)`, `modulo1_usuarios/M1_02_datos_iniciales.sql (usuarios por rol, categorías, locaciones)`, `modulo1_usuarios/M1_03_pruebas.sql`

**Documentación:** `docs/modulos/M1_Usuarios/` (este README, `GUIA_INTEGRACION.md`, `MATRIZ_TRAZABILIDAD.md`).
La lista exacta de archivos está en `MANIFIESTO.txt` del paquete.

## Dependencias
- **Usa:** Solo del núcleo.
- **Provee:** `ICategoriaServicio` y `ILocacionServicio` (Web) que M2 usa en formularios y filtros; usuarios semilla que M2/M3/M4 usan en sus datos y pruebas.

## Pruebas del módulo
```bash
dotnet test API/EventoTickets.API.sln --filter "Modulo=M1"                                   # unitarias
EVENTCR_PRUEBAS_SQL="<cadena>" dotnet test API/EventoTickets.API.sln --filter "Modulo=M1&Categoria=Integracion"
sqlcmd -S <servidor> -E -C -i database/modulo1_usuarios/M1_03_pruebas.sql       # pruebas SQL (todas deben decir OK)
```
Resultados obtenidos y pendientes: `MATRIZ_TRAZABILIDAD.md` y `docs/PRUEBAS.md`.

## Verificación manual
1. Registrar un comprador nuevo → queda Comprador/Activo y el hash empieza con `PBKDF2-SHA256$`.
2. Iniciar sesión con `roberto.silva@correo.com` (Bloqueado) → mensaje de cuenta bloqueada.
3. Eliminar un usuario → queda `Inactivo` y no puede iniciar sesión; su historial se conserva.
4. Eliminar una categoría con eventos → se desactiva (no se borra).

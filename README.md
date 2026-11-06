# EventCR · BoleteríaPro

Plataforma web de **gestión de eventos y venta de entradas** con validación manual de pagos (SINPE Móvil / transferencia), boletos digitales con código QR y control de acceso en puerta.
Proyecto final de *Programación Avanzada en Web (SC-701)*, Universidad Fidélitas. Base de datos: **`EventCR_BoleteriaPro_DB`**.

| Capa | Tecnología |
|---|---|
| API REST | .NET 8 · C# · Web API con controladores · Swagger · JWT (HS256) |
| Acceso a datos | Dapper + **procedimientos almacenados** (no se usa Entity Framework) |
| Interfaz web | ASP.NET Core Razor Pages (.NET 8) · Tag Helpers · Tailwind (diseño Stitch) — solución aparte que **solo consume el API** |
| Servicios | SHA-256 de comprobantes, gestión de archivos, generación de QR (QRCoder), expiración automática de reservas |
| Base de datos | SQL Server 2019+ · 9 tablas (ER) · 50 procedimientos · 6 vistas · 3 funciones · 3 triggers |
| Pruebas | xUnit (85 pruebas + 3 de integración/paquetes reales) · scripts SQL de pruebas por módulo |

El proyecto está **dividido en 4 módulos** con dueño, sobre un **núcleo compartido** con un único responsable de integración.
Guía completa: [`docs/00_GUIA_INTEGRACION_GENERAL.md`](docs/00_GUIA_INTEGRACION_GENERAL.md).

| Módulo | Responsable | Historias de usuario |
|---|---|---|
| M1 · Usuarios, autenticación, categorías y locaciones | ~M | HU-01, HU-02, HU-03, HU-07, HU-08 |
| M2 · Eventos, cartelera, tipos de entrada, disponibilidad **y base de datos general** | Donnkan Cervantes Peraza | HU-04, HU-05, HU-06, HU-09, HU-10, HU-11 |
| M3 · Pedidos, historial, cancelación y vencimiento | Jonathan Umaña | HU-12, HU-13, HU-14 |
| M4 · Pagos, auditoría, entradas QR y control de acceso | ~Brodie | HU-15, HU-16, HU-17, HU-18, HU-19 |

---

## 1. Estructura del repositorio

```
EventCR/
├── API/EventoTickets.API.sln          Solución 1 (REST API)
│   ├── EventoTickets.Abstracciones/   contratos: Nucleo/ + Modulo1_Usuarios/ … Modulo4_PagosQR/ (Modelos, Interfaces)
│   ├── EventoTickets.DA/              Dapper → SP          (Nucleo/ + ModuloN/)
│   ├── EventoTickets.Reglas/          reglas de negocio    (ModuloN/)
│   ├── EventoTickets.Flujo/           orquestación         (ModuloN/)
│   ├── EventoTickets.Servicios/       hash, JWT, archivos, QR, expiración (Nucleo/ + ModuloN/)
│   ├── EventoTickets.API/             Controllers/ModuloN, Nucleo (filtro, middleware, seguridad), Registro/ (DI por módulo)
│   └── EventoTickets.Pruebas/         xUnit: Nucleo/ + ModuloN/
├── Web/EventoTickets.Web.sln          Solución 2 (Razor Pages; nunca accede a SQL)
│   ├── EventoTickets.Web.Abstracciones/  Nucleo/ + ModuloN/ (Modelos, Interfaces)
│   └── EventoTickets.Web/             Pages/ (por ruta), Servicios/ModuloN, Registro/, Infraestructura/, wwwroot/
├── database/                          00_EJECUTAR_TODO.sql, base/ (M2), moduloN_*/, validacion/, instaladores
└── docs/                              guía general, modelo de datos, pruebas, módulos (README, guía, matriz)
```

## 2. Requisitos previos

- **.NET 8 SDK** (`dotnet --version` ≥ 8.0.100)
- **SQL Server 2019+** (Developer/Express, LocalDB o Docker) y `sqlcmd`, SSMS o Azure Data Studio

> macOS / Linux con Docker:
> ```bash
> docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=<clave-segura>" -p 1433:1433 --name sqlserver -d mcr.microsoft.com/mssql/server:2022-latest
> ```

## 3. Instalación

### 3.1 Base de datos
```bash
cd database
# Windows (autenticación integrada)          macOS / Linux (Docker)
.\instalar.ps1 -Servidor localhost            ./instalar.sh -S localhost,1433 -U sa -P '<clave>'
.\instalar.ps1 -Servidor localhost -Pruebas   ./instalar.sh -S localhost,1433 -U sa -P '<clave>' --pruebas
```
Alternativas: `00_EJECUTAR_TODO.sql` en **modo SQLCMD** (SSMS: *Consulta → Modo SQLCMD*) o `EventCR_BoleteriaPro_DB_completo.sql` (un solo archivo, F5).
El orden exacto está en `database/orden_ejecucion.txt` y se explica en la guía general §10.

### 3.2 Configuración sensible (variables de entorno, nunca en el repositorio)
Ver [`.env.example`](.env.example). ASP.NET Core lee las variables de entorno con `__` como separador:
```bash
ConnectionStrings__EventCR_BoleteriaPro_DB="Server=localhost,1433;Database=EventCR_BoleteriaPro_DB;User Id=sa;Password=<clave>;TrustServerCertificate=True;"
Jwt__Clave="<clave aleatoria de 32 caracteres o más>"
```
O con *user-secrets*: `dotnet user-secrets set "Jwt:Clave" "<clave>" --project API/EventoTickets.API`.
En *Development* existe una clave JWT de desarrollo; fuera de Development el API no inicia sin `Jwt__Clave`.

### 3.3 Ejecutar
```bash
dotnet run --project API/EventoTickets.API --launch-profile http   # http://localhost:5238/swagger
dotnet run --project Web/EventoTickets.Web --launch-profile http   # http://localhost:5240
```

### 3.4 Compilar y probar
```bash
dotnet build API/EventoTickets.API.sln
dotnet build Web/EventoTickets.Web.sln
dotnet test  API/EventoTickets.API.sln                              # todas las pruebas
dotnet test  API/EventoTickets.API.sln --filter "Modulo=M3"          # solo un módulo
EVENTCR_PRUEBAS_SQL="<cadena>" dotnet test API/EventoTickets.API.sln --filter "Categoria=Integracion"
```

## 4. Usuarios de prueba

Contraseña de todos: `Boleteria2026!` (almacenada como hash PBKDF2-SHA256; nunca en texto plano).

| Correo | Rol |
|---|---|
| admin@boleteriapro.cr | Superadministrador |
| eventos@boleteriapro.cr | AdministradorEventos |
| auditor@boleteriapro.cr | AuditorPagos |
| acceso@boleteriapro.cr | ValidadorAcceso |
| mariana.gomez@correo.com · carlos.mendez@correo.com · valeria.ruiz@correo.com | Comprador |
| roberto.silva@correo.com | Comprador **Bloqueado** (para probar HU-02) |

### Recorrido de demostración
1. Comprador: cartelera → evento → elegir entradas → **Ver resumen del pedido** → **Confirmar y reservar** → registrar pago con comprobante.
2. Auditor: *Pagos y Validación* → revisar comprobante → **Aprobar** (se emiten las entradas en estado *Válida*).
3. Comprador: *Mis Entradas* → **Ver boleto** → descargar QR / imprimir; nominar a otro asistente (QR nuevo).
4. Validador: *Control de Acceso* → leer/pegar el código → *Acceso autorizado*; repetir → *ya fue utilizada*.

## 5. Documentación

| Documento | Contenido |
|---|---|
| [00_GUIA_INTEGRACION_GENERAL.md](docs/00_GUIA_INTEGRACION_GENERAL.md) | Arquitectura, carpetas, dependencias, matriz de 19 HU, contratos, rutas, configuración, Git, integración |
| [modelo-datos/MODELO_DATOS.md](docs/modelo-datos/MODELO_DATOS.md) | Modelo de datos y diccionario (responsable: M2) |
| [PRUEBAS.md](docs/PRUEBAS.md) | Pruebas ejecutadas, resultados reales y pendientes |
| [modulos/](docs/modulos/) | Por módulo: README, guía de integración y matriz de trazabilidad |
| [historial/](docs/historial/) | Auditoría de la consolidación inicial (primera fase) |
| [diseno-stitch/DESIGN.md](docs/diseno-stitch/DESIGN.md) | Sistema de diseño de Stitch |

## 6. Solución de problemas

| Síntoma | Causa / solución |
|---|---|
| La Web dice *"El servicio no está disponible…"* | El API no está corriendo o `Api:UrlBase` apunta a otro puerto. |
| `Configure Jwt:Clave…` al iniciar el API | Falta `Jwt__Clave` fuera de *Development*. |
| `A network-related or instance-specific error` | Cadena de conexión incorrecta (en Docker: `Server=localhost,1433` + usuario SQL). |
| `00_EJECUTAR_TODO.sql` marca error en `:r` | No está activo el **modo SQLCMD**; use el instalador o el archivo `_completo.sql`. |
| Las reservas no vencen | `Ventas:SegundosRevisionExpiracion` = 0 desactiva el servicio; use 60 (por defecto) o el job opcional `M3_05`. |

# EVENTPASS · Guía de integración general (núcleo compartido)

Este paquete contiene el **núcleo** del proyecto EventCR / BoleteríaPro, que mantiene el responsable de integración, y las herramientas para unir los 4 módulos en **una sola aplicación**.

| Contenido | Para qué |
|---|---|
| `00_GUIA_INTEGRACION_GENERAL.md` | Arquitectura, carpetas y propiedad, dependencias, matriz de las 19 HU, contratos, DTO, rutas, diccionario de datos, orden SQL, SQL Server, autenticación, Git, integración y pruebas |
| `archivos/` | Núcleo con rutas del repositorio: soluciones y `.csproj`, `Program.cs` (API y Web), contratos (`RespuestaApi`, excepciones, roles, estados, configuración), filtro y middleware, `ClienteApi`, layouts, `wwwroot`, pruebas del núcleo, `database/validacion/`, `README.md`, `.gitignore`, `.env.example` y `docs/` generales |
| `MANIFIESTO.txt` | Archivos de este paquete |
| `MANIFIESTO_COMPLETO.txt` | Todos los archivos del proyecto y su dueño (NUCLEO, M1…M4), separados por tabulador |
| `ensamblar.sh` / `ensamblar.ps1` | Unen el núcleo y los 4 paquetes y verifican que no falte ni se repita ningún archivo |

## Ensamblar
```bash
# 1. Descomprimir los 5 ZIP en una misma carpeta, p. ej. ~/eventpass/
# 2. macOS / Linux
~/eventpass/EVENTPASS_GUIA_INTEGRACION_GENERAL/ensamblar.sh ~/eventpass ~/EventCR
# 2. Windows (PowerShell)
~\eventpass\EVENTPASS_GUIA_INTEGRACION_GENERAL\ensamblar.ps1 -Origen ~\eventpass -Destino ~\EventCR
# 3. Compilar y probar
cd ~/EventCR
dotnet build API/EventoTickets.API.sln && dotnet build Web/EventoTickets.Web.sln
dotnet test API/EventoTickets.API.sln
```
Luego instale la base (`database/instalar.*`) y siga el README del proyecto.

> El núcleo **por sí solo no compila**: `Program.cs` registra los 4 módulos. Para trabajar en un módulo aislado, siga el orden de integración núcleo → M1 → M2 → M3 → M4 (guía §3 y §14).

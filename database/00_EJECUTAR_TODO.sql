/* =============================================================================
   00 · Script maestro de instalación — EventCR_BoleteriaPro_DB
   Responsable: Módulo 2 (estructura general de la base de datos)

   Requiere MODO SQLCMD:
     - SSMS: menú Consulta > Modo SQLCMD, luego F5 (abra este archivo desde la carpeta database/).
     - Azure Data Studio / sqlcmd:  sqlcmd -S localhost -E -C -i 00_EJECUTAR_TODO.sql
   Sin modo SQLCMD use EventCR_BoleteriaPro_DB_completo.sql (mismo contenido en un archivo).

   ¡Atención! 01 y 02 eliminan y recrean la base y sus tablas (instalación limpia).
   Orden: estructura (base) → objetos de cada módulo (M1 → M2 → M3 → M4) → reportes →
          datos iniciales → datos de prueba. Las pruebas por módulo se ejecutan aparte.
   ============================================================================= */
:on error exit
PRINT N'>> base/01_crear_base_datos.sql';
:r base/01_crear_base_datos.sql
PRINT N'>> base/02_tablas.sql';
:r base/02_tablas.sql
PRINT N'>> base/03_llaves_foraneas.sql';
:r base/03_llaves_foraneas.sql
PRINT N'>> base/04_restricciones.sql';
:r base/04_restricciones.sql
PRINT N'>> base/05_indices.sql';
:r base/05_indices.sql
PRINT N'>> base/06_funciones.sql';
:r base/06_funciones.sql
PRINT N'>> modulo1_usuarios/M1_01_procedimientos.sql';
:r modulo1_usuarios/M1_01_procedimientos.sql
PRINT N'>> modulo2_eventos/M2_01_vistas.sql';
:r modulo2_eventos/M2_01_vistas.sql
PRINT N'>> modulo2_eventos/M2_02_procedimientos.sql';
:r modulo2_eventos/M2_02_procedimientos.sql
PRINT N'>> modulo2_eventos/M2_03_triggers.sql';
:r modulo2_eventos/M2_03_triggers.sql
PRINT N'>> modulo3_pedidos/M3_01_vistas.sql';
:r modulo3_pedidos/M3_01_vistas.sql
PRINT N'>> modulo3_pedidos/M3_02_procedimientos.sql';
:r modulo3_pedidos/M3_02_procedimientos.sql
PRINT N'>> modulo3_pedidos/M3_03_triggers.sql';
:r modulo3_pedidos/M3_03_triggers.sql
PRINT N'>> modulo4_pagos_qr/M4_01_vistas.sql';
:r modulo4_pagos_qr/M4_01_vistas.sql
PRINT N'>> modulo4_pagos_qr/M4_02_procedimientos.sql';
:r modulo4_pagos_qr/M4_02_procedimientos.sql
PRINT N'>> modulo4_pagos_qr/M4_03_triggers.sql';
:r modulo4_pagos_qr/M4_03_triggers.sql
PRINT N'>> modulo2_eventos/M2_05_reportes.sql';
:r modulo2_eventos/M2_05_reportes.sql
PRINT N'>> modulo1_usuarios/M1_02_datos_iniciales.sql';
:r modulo1_usuarios/M1_02_datos_iniciales.sql
PRINT N'>> modulo2_eventos/M2_04_datos_iniciales.sql';
:r modulo2_eventos/M2_04_datos_iniciales.sql
PRINT N'>> modulo4_pagos_qr/M4_04_datos_prueba.sql';
:r modulo4_pagos_qr/M4_04_datos_prueba.sql
PRINT N'EventCR_BoleteriaPro_DB instalada. Ejecute las pruebas: moduloN/*_pruebas.sql y validacion/99_validacion_general.sql';

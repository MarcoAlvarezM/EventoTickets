/* =============================================================================
   Estructura general de la base de datos · Responsable: Módulo 2 (Donnkan Cervantes Peraza)
   EventoTickets / BoleteríaPro — 01. Creación de la base de datos
   Motor: Microsoft SQL Server 2019+ (compatible con SQL Server 2022 / Express)
   Ejecutar conectado a la instancia (base master).
   ============================================================================= */
SET NOCOUNT ON;
GO

IF DB_ID(N'EventCR_BoleteriaPro_DB') IS NULL
BEGIN
    CREATE DATABASE EventCR_BoleteriaPro_DB COLLATE Modern_Spanish_CI_AS;
END
GO

ALTER DATABASE EventCR_BoleteriaPro_DB SET READ_COMMITTED_SNAPSHOT ON WITH ROLLBACK IMMEDIATE;
GO

USE EventCR_BoleteriaPro_DB;
GO

PRINT N'Base de datos EventCR_BoleteriaPro_DB lista.';
GO

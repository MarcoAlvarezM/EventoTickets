/* =============================================================================
   M3 · (OPCIONAL) Trabajo de SQL Server Agent para vencer reservas — HU-14
   El mecanismo principal de vencimiento es el servicio en segundo plano del API
   (ExpiracionReservasServicio, cada Ventas:SegundosRevisionExpiracion segundos), que
   no depende de que alguien visite una página. Además sp_Pedido_Crear y sp_Pago_Registrar
   vencen las reservas expiradas antes de operar.
   Este script es una alternativa para servidores con SQL Server Agent (no disponible en
   SQL Server Express). Ejecuta sp_Pedido_LiberarVencidos cada minuto.
   ============================================================================= */
USE msdb;
GO
IF EXISTS (SELECT 1 FROM msdb.dbo.sysjobs WHERE name = N'EventCR - Vencer reservas')
    EXEC msdb.dbo.sp_delete_job @job_name = N'EventCR - Vencer reservas';
GO
EXEC msdb.dbo.sp_add_job @job_name = N'EventCR - Vencer reservas', @enabled = 1,
     @description = N'HU-14: marca como Vencido cada pedido PendientePago con la reserva expirada y devuelve el cupo.';
EXEC msdb.dbo.sp_add_jobstep @job_name = N'EventCR - Vencer reservas', @step_name = N'sp_Pedido_LiberarVencidos',
     @subsystem = N'TSQL', @database_name = N'EventCR_BoleteriaPro_DB',
     @command = N'EXEC dbo.sp_Pedido_LiberarVencidos @Silencioso = 1;';
EXEC msdb.dbo.sp_add_schedule @schedule_name = N'EventCR - Cada minuto', @freq_type = 4, @freq_interval = 1,
     @freq_subday_type = 4, @freq_subday_interval = 1;
EXEC msdb.dbo.sp_attach_schedule @job_name = N'EventCR - Vencer reservas', @schedule_name = N'EventCR - Cada minuto';
EXEC msdb.dbo.sp_add_jobserver @job_name = N'EventCR - Vencer reservas';
GO

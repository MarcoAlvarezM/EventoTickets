/* =============================================================================
   M4 · Triggers
   Módulo: Módulo 4 · Pagos, auditoría, entradas QR y control de acceso (HU-15 a HU-19)
   Objetos: trg_Entradas_ValidarConsistencia
   Depende de: base/01..06, modulo3 (vw_PedidosResumen, sp_Pedido_LiberarVencidos)
   ============================================================================= */
USE EventCR_BoleteriaPro_DB;
GO

/* Una entrada debe coincidir con el evento de su pedido y de su tipo de entrada */
CREATE OR ALTER TRIGGER dbo.trg_Entradas_ValidarConsistencia
ON dbo.Entradas
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (
        SELECT 1
        FROM inserted i
        INNER JOIN dbo.Pedidos p      ON p.PedidoId = i.PedidoId
        INNER JOIN dbo.TiposEntrada t ON t.TipoEntradaId = i.TipoEntradaId
        WHERE i.EventoId <> p.EventoId OR i.EventoId <> t.EventoId)
    BEGIN
        ROLLBACK TRANSACTION;
        THROW 50051, N'La entrada no es consistente con el evento de su pedido o de su tipo de entrada.', 1;
    END
END
GO

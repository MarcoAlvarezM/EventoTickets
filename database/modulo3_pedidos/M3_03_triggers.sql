/* =============================================================================
   M3 · Triggers
   Módulo: Módulo 3 · Pedidos, historial, cancelación y vencimiento de reservas (HU-12, 13, 14)
   Objetos: trg_PedidoDetalles_ValidarEvento
   Depende de: base/01..06, modulo2 (vistas de eventos)
   ============================================================================= */
USE EventCR_BoleteriaPro_DB;
GO

/* Las líneas de un pedido solo pueden usar tipos de entrada del evento del pedido */
CREATE OR ALTER TRIGGER dbo.trg_PedidoDetalles_ValidarEvento
ON dbo.PedidoDetalles
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (
        SELECT 1
        FROM inserted i
        INNER JOIN dbo.Pedidos p      ON p.PedidoId = i.PedidoId
        INNER JOIN dbo.TiposEntrada t ON t.TipoEntradaId = i.TipoEntradaId
        WHERE t.EventoId <> p.EventoId)
    BEGIN
        ROLLBACK TRANSACTION;
        THROW 50050, N'El tipo de entrada no pertenece al evento del pedido.', 1;
    END
END
GO

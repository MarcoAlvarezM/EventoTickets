/* =============================================================================
   M2 · Triggers
   Módulo: Módulo 2 · Eventos, cartelera, tipos de entrada y disponibilidad (HU-04, 05, 06, 09, 10, 11)
   Objetos: trg_TiposEntrada_SincronizarAgotado
   Depende de: base/01..06
   ============================================================================= */
USE EventCR_BoleteriaPro_DB;
GO

/* Sincroniza el estado Agotado del evento con su inventario (HU-06 / HU-11):
   Publicado -> Agotado cuando no queda cupo; Agotado -> Publicado cuando se libera cupo
   (expiración o cancelación de reservas). Los demás estados no se tocan. */
CREATE OR ALTER TRIGGER dbo.trg_TiposEntrada_SincronizarAgotado
ON dbo.TiposEntrada
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Afectados TABLE (EventoId INT PRIMARY KEY);
    INSERT INTO @Afectados (EventoId)
    SELECT EventoId FROM inserted UNION SELECT EventoId FROM deleted;

    UPDATE e SET Estado = 'Agotado'
    FROM dbo.Eventos e INNER JOIN @Afectados a ON a.EventoId = e.EventoId
    WHERE e.Estado = 'Publicado' AND dbo.fn_DisponibilidadEvento(e.EventoId) = 0;

    UPDATE e SET Estado = 'Publicado'
    FROM dbo.Eventos e INNER JOIN @Afectados a ON a.EventoId = e.EventoId
    WHERE e.Estado = 'Agotado' AND dbo.fn_DisponibilidadEvento(e.EventoId) > 0;
END
GO

using EventoTickets.Abstracciones.Modelos;

namespace EventoTickets.Abstracciones.Interfaces.Flujo;


public interface IPagoFlujo
{
    Task<PagoResponse> RegistrarAsync(int usuarioId, PagoRegistroRequest solicitud);
    Task<IEnumerable<PagoResponse>> ListarAsync(string? estado, string? busqueda);
    Task<IEnumerable<PagoResponse>> ListarPorUsuarioAsync(int usuarioId);
    Task<IEnumerable<PagoResponse>> ListarPorPedidoAsync(int pedidoId, int usuarioId, bool esPersonal);
    Task<PagoResponse> ObtenerAsync(int pagoId, int usuarioId, bool esPersonal);
    Task<ArchivoComprobante> ObtenerComprobanteAsync(int pagoId, int usuarioId, bool esPersonal);
    Task<PagoResponse> AprobarAsync(int pagoId, int auditorUsuarioId, PagoAprobarRequest solicitud);
    Task<PagoResponse> RechazarAsync(int pagoId, int auditorUsuarioId, PagoRechazarRequest solicitud);
}

public interface IEntradaFlujo
{
    Task<IEnumerable<EntradaResponse>> ListarPorUsuarioAsync(int usuarioId);
    Task<IEnumerable<EntradaResponse>> ListarPorEventoAsync(int eventoId, string? estado);
    Task<EntradaResponse> ObtenerAsync(int entradaId, int usuarioId, bool esPersonal);
    Task<EntradaResponse> TransferirAsync(int entradaId, int usuarioId, TransferenciaEntradaRequest solicitud);
    Task<EntradaResponse> RegistrarIngresoAsync(ValidacionAccesoRequest solicitud, int validadorUsuarioId);
}

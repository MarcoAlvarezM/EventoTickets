using EventoTickets.Web.Abstracciones.Modelos;

namespace EventoTickets.Web.Abstracciones.Interfaces;


public interface IPagoServicio
{
    Task<ResultadoApi<PagoResponse>> RegistrarAsync(PagoRegistroRequest solicitud);
    Task<ResultadoApi<List<PagoResponse>>> ListarAsync(string? estado, string? busqueda);
    Task<ResultadoApi<List<PagoResponse>>> ListarMiosAsync();
    Task<ResultadoApi<List<PagoResponse>>> ListarPorPedidoAsync(int pedidoId);
    Task<ResultadoApi<PagoResponse>> ObtenerAsync(int id);
    Task<ResultadoApi<ArchivoDescargado>> DescargarComprobanteAsync(int id);
    Task<ResultadoApi<PagoResponse>> AprobarAsync(int id, PagoAprobarRequest solicitud);
    Task<ResultadoApi<PagoResponse>> RechazarAsync(int id, PagoRechazarRequest solicitud);
}

public interface IEntradaServicio
{
    Task<ResultadoApi<List<EntradaResponse>>> ListarMiasAsync();
    Task<ResultadoApi<List<EntradaResponse>>> ListarPorEventoAsync(int eventoId, string? estado);
    Task<ResultadoApi<EntradaResponse>> ObtenerAsync(int id);
    Task<ResultadoApi<ArchivoDescargado>> DescargarQrAsync(int id);
    Task<ResultadoApi<EntradaResponse>> TransferirAsync(int id, TransferenciaEntradaRequest solicitud);
    Task<ResultadoApi<EntradaResponse>> RegistrarIngresoAsync(ValidacionAccesoRequest solicitud);
}

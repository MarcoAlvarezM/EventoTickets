using EventoTickets.Web.Abstracciones.Modelos;

namespace EventoTickets.Web.Abstracciones.Interfaces;


public interface IPedidoServicio
{
    Task<ResultadoApi<CotizacionResponse>> CotizarAsync(PedidoRequest solicitud);
    Task<ResultadoApi<PedidoCompletoResponse>> CrearAsync(PedidoRequest solicitud);
    Task<ResultadoApi<List<PedidoResponse>>> ListarMiosAsync(string? estado);
    Task<ResultadoApi<List<PedidoResponse>>> ListarAsync(FiltroPedidos filtro);
    Task<ResultadoApi<PedidoCompletoResponse>> ObtenerAsync(int id);
    Task<ResultadoApi<PedidoCompletoResponse>> ActualizarMetodoPagoAsync(int id, ActualizarMetodoPagoRequest solicitud);
    Task<ResultadoApi<int>> LiberarVencidosAsync();
    Task<ResultadoApi<SinContenido>> CancelarAsync(int id);
}

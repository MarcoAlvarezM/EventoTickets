using EventoTickets.Abstracciones.Modelos;

namespace EventoTickets.Abstracciones.Interfaces.Flujo;


public interface IPedidoFlujo
{
    Task<CotizacionResponse> CotizarAsync(PedidoRequest solicitud);
    Task<PedidoCompletoResponse> CrearAsync(int usuarioId, PedidoRequest solicitud);
    Task<PedidoCompletoResponse> ObtenerAsync(int pedidoId, int usuarioId, bool esPersonal);
    Task<IEnumerable<PedidoResponse>> ListarPorUsuarioAsync(int usuarioId, string? estado);
    Task<IEnumerable<PedidoResponse>> ListarAsync(FiltroPedidos filtro);
    Task<PedidoCompletoResponse> ActualizarMetodoPagoAsync(int pedidoId, int usuarioId, bool esPersonal, ActualizarMetodoPagoRequest solicitud);
    Task CancelarAsync(int pedidoId, int usuarioId, bool esPersonal);
    Task<int> LiberarVencidosAsync();
}

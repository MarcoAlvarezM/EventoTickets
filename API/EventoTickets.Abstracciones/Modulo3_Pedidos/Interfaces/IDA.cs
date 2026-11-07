using System.Data;
using EventoTickets.Abstracciones.Modelos;
using Microsoft.AspNetCore.Http;

namespace EventoTickets.Abstracciones.Interfaces.DA;


public interface IPedidoDA
{
    Task<int> CrearAsync(int usuarioId, int eventoId, string metodoPago, IEnumerable<PedidoLineaRequest> lineas,
                         decimal porcentajeCargo, decimal porcentajeIva, int minutosReserva, int maxEntradas);
    Task<PedidoCompletoResponse?> ObtenerAsync(int pedidoId);
    Task<IEnumerable<PedidoResponse>> ListarPorUsuarioAsync(int usuarioId, string? estado);
    Task<IEnumerable<PedidoResponse>> ListarAsync(FiltroPedidos filtro);
    Task ActualizarMetodoPagoAsync(int pedidoId, int? usuarioId, string metodoPago);
    Task CancelarAsync(int pedidoId, int? usuarioId);
    Task<int> LiberarVencidosAsync();
}

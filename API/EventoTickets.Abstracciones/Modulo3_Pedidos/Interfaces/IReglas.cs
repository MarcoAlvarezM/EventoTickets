using EventoTickets.Abstracciones.Modelos;
using Microsoft.AspNetCore.Http;

namespace EventoTickets.Abstracciones.Interfaces.Reglas;


public interface IPedidoReglas
{
    /// <summary>Agrupa líneas repetidas, elimina cantidades en cero y valida el máximo por pedido.</summary>
    List<PedidoLineaRequest> NormalizarLineas(IEnumerable<PedidoLineaRequest> lineas);

    /// <summary>Calcula subtotal, cargo por servicio, IVA y total con los precios vigentes.</summary>
    CotizacionResponse Cotizar(int eventoId, IEnumerable<PedidoLineaRequest> lineas, IEnumerable<TipoEntradaResponse> tiposEvento);
}

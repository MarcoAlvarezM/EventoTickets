using EventoTickets.Abstracciones.Configuracion;
using EventoTickets.Abstracciones.Excepciones;
using EventoTickets.Abstracciones.Interfaces.Reglas;
using EventoTickets.Abstracciones.Modelos;
using Microsoft.Extensions.Options;

namespace EventoTickets.Reglas;


/// <summary>
/// Cálculo del pedido. Fórmula (idéntica a sp_Pedido_Crear, que es la fuente de verdad al confirmar):
///   CargoServicio = Subtotal × %Cargo ;  IVA = (Subtotal + Cargo) × %IVA ;  Total = Subtotal + Cargo + IVA
/// </summary>
public class PedidoReglas : IPedidoReglas
{
    private readonly ConfiguracionVentas _ventas;

    public PedidoReglas(IOptions<ConfiguracionVentas> ventas) => _ventas = ventas.Value;

    public List<PedidoLineaRequest> NormalizarLineas(IEnumerable<PedidoLineaRequest> lineas)
    {
        var normalizadas = lineas
            .Where(l => l.Cantidad > 0)
            .GroupBy(l => l.TipoEntradaId)
            .Select(g => new PedidoLineaRequest { TipoEntradaId = g.Key, Cantidad = g.Sum(x => x.Cantidad) })
            .ToList();

        if (normalizadas.Count == 0)
            throw new ReglaNegocioException("Debe seleccionar al menos una entrada.");
        var total = normalizadas.Sum(l => l.Cantidad);
        if (total > _ventas.MaxEntradasPorPedido)
            throw new ReglaNegocioException($"Puede comprar como máximo {_ventas.MaxEntradasPorPedido} entradas por pedido.");
        return normalizadas;
    }

    public CotizacionResponse Cotizar(int eventoId, IEnumerable<PedidoLineaRequest> lineas, IEnumerable<TipoEntradaResponse> tiposEvento)
    {
        var tipos = tiposEvento.ToDictionary(t => t.TipoEntradaId);
        var cotizacion = new CotizacionResponse
        {
            EventoId = eventoId,
            PorcentajeCargoServicio = _ventas.PorcentajeCargoServicio,
            PorcentajeIVA = _ventas.PorcentajeIVA,
            MinutosReserva = _ventas.MinutosReserva,
            Moneda = _ventas.Moneda
        };

        foreach (var linea in lineas)
        {
            if (!tipos.TryGetValue(linea.TipoEntradaId, out var tipo) || !tipo.Activo)
                throw new ReglaNegocioException("Alguno de los tipos de entrada no pertenece al evento o no está activo.");
            if (linea.Cantidad > tipo.Disponible)
                throw new ConflictoException($"Solo quedan {tipo.Disponible} entradas disponibles de «{tipo.Nombre}».");

            cotizacion.Lineas.Add(new CotizacionLinea
            {
                TipoEntradaId = tipo.TipoEntradaId,
                Nombre = tipo.Nombre,
                ColorHex = tipo.ColorHex,
                Cantidad = linea.Cantidad,
                PrecioUnitario = tipo.Precio,
                SubtotalLinea = tipo.Precio * linea.Cantidad
            });
        }

        cotizacion.CantidadEntradas = cotizacion.Lineas.Sum(l => l.Cantidad);
        cotizacion.Subtotal = cotizacion.Lineas.Sum(l => l.SubtotalLinea);
        cotizacion.CargoServicio = Math.Round(cotizacion.Subtotal * _ventas.PorcentajeCargoServicio / 100m, 2, MidpointRounding.AwayFromZero);
        cotizacion.ImpuestoIVA = Math.Round((cotizacion.Subtotal + cotizacion.CargoServicio) * _ventas.PorcentajeIVA / 100m, 2, MidpointRounding.AwayFromZero);
        cotizacion.ImporteTotal = cotizacion.Subtotal + cotizacion.CargoServicio + cotizacion.ImpuestoIVA;
        return cotizacion;
    }
}

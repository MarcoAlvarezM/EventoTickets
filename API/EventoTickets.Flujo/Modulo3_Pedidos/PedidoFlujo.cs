using EventoTickets.Abstracciones.Configuracion;
using EventoTickets.Abstracciones.Constantes;
using EventoTickets.Abstracciones.Excepciones;
using EventoTickets.Abstracciones.Interfaces.DA;
using EventoTickets.Abstracciones.Interfaces.Flujo;
using EventoTickets.Abstracciones.Interfaces.Reglas;
using EventoTickets.Abstracciones.Modelos;
using Microsoft.Extensions.Options;

namespace EventoTickets.Flujo;


public class PedidoFlujo : IPedidoFlujo
{
    private readonly IPedidoDA _pedidoDA;
    private readonly IEventoDA _eventoDA;
    private readonly ITipoEntradaDA _tipoEntradaDA;
    private readonly IPedidoReglas _pedidoReglas;
    private readonly ConfiguracionVentas _ventas;

    public PedidoFlujo(IPedidoDA pedidoDA, IEventoDA eventoDA, ITipoEntradaDA tipoEntradaDA,
                       IPedidoReglas pedidoReglas, IOptions<ConfiguracionVentas> ventas)
    {
        _pedidoDA = pedidoDA;
        _eventoDA = eventoDA;
        _tipoEntradaDA = tipoEntradaDA;
        _pedidoReglas = pedidoReglas;
        _ventas = ventas.Value;
    }

    public async Task<CotizacionResponse> CotizarAsync(PedidoRequest solicitud)
    {
        var evento = await _eventoDA.ObtenerAsync(solicitud.EventoId) ?? throw new RecursoNoEncontradoException("El evento no existe.");
        if (evento.Estado != EstadosEvento.Publicado || evento.FechaEvento.Date < DateTime.Today)
            throw new ReglaNegocioException("El evento no está disponible para la venta.");

        var lineas = _pedidoReglas.NormalizarLineas(solicitud.Lineas);
        var tipos = await _tipoEntradaDA.ListarPorEventoAsync(solicitud.EventoId, true);
        return _pedidoReglas.Cotizar(solicitud.EventoId, lineas, tipos);
    }

    public async Task<PedidoCompletoResponse> CrearAsync(int usuarioId, PedidoRequest solicitud)
    {
        // La cotización valida reglas y disponibilidad antes de ir a la base;
        // el procedimiento vuelve a validar con bloqueo para evitar sobreventa concurrente.
        await CotizarAsync(solicitud);
        var lineas = _pedidoReglas.NormalizarLineas(solicitud.Lineas);

        var pedidoId = await _pedidoDA.CrearAsync(usuarioId, solicitud.EventoId, solicitud.MetodoPago, lineas,
            _ventas.PorcentajeCargoServicio, _ventas.PorcentajeIVA, _ventas.MinutosReserva, _ventas.MaxEntradasPorPedido);
        return await _pedidoDA.ObtenerAsync(pedidoId) ?? throw new RecursoNoEncontradoException("No se pudo recuperar el pedido creado.");
    }

    public async Task<PedidoCompletoResponse> ObtenerAsync(int pedidoId, int usuarioId, bool esPersonal)
    {
        var pedido = await _pedidoDA.ObtenerAsync(pedidoId) ?? throw new RecursoNoEncontradoException("El pedido no existe.");
        if (!esPersonal && pedido.UsuarioId != usuarioId)
            throw new AccesoDenegadoException("El pedido no pertenece al usuario.");
        return pedido;
    }

    public Task<IEnumerable<PedidoResponse>> ListarPorUsuarioAsync(int usuarioId, string? estado) =>
        _pedidoDA.ListarPorUsuarioAsync(usuarioId, estado);

    public Task<IEnumerable<PedidoResponse>> ListarAsync(FiltroPedidos filtro) => _pedidoDA.ListarAsync(filtro);

    public async Task<PedidoCompletoResponse> ActualizarMetodoPagoAsync(int pedidoId, int usuarioId, bool esPersonal, ActualizarMetodoPagoRequest solicitud)
    {
        await _pedidoDA.ActualizarMetodoPagoAsync(pedidoId, esPersonal ? null : usuarioId, solicitud.MetodoPago);
        return await ObtenerAsync(pedidoId, usuarioId, esPersonal);
    }

    public Task CancelarAsync(int pedidoId, int usuarioId, bool esPersonal) =>
        _pedidoDA.CancelarAsync(pedidoId, esPersonal ? null : usuarioId);

    public Task<int> LiberarVencidosAsync() => _pedidoDA.LiberarVencidosAsync();
}

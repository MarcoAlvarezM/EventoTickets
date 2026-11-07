using EventoTickets.Web.Abstracciones.Interfaces;
using EventoTickets.Web.Abstracciones.Modelos;

namespace EventoTickets.Web.Servicios;


public class PedidoServicio : IPedidoServicio
{
    private readonly IClienteApi _api;
    public PedidoServicio(IClienteApi api) => _api = api;

    public Task<ResultadoApi<CotizacionResponse>> CotizarAsync(PedidoRequest solicitud) => _api.PostAsync<CotizacionResponse>("api/pedidos/cotizacion", solicitud);
    public Task<ResultadoApi<PedidoCompletoResponse>> CrearAsync(PedidoRequest solicitud) => _api.PostAsync<PedidoCompletoResponse>("api/pedidos", solicitud);
    public Task<ResultadoApi<List<PedidoResponse>>> ListarMiosAsync(string? estado) =>
        _api.GetAsync<List<PedidoResponse>>(Consulta.Crear("api/pedidos/mios", ("estado", estado)));
    public Task<ResultadoApi<List<PedidoResponse>>> ListarAsync(FiltroPedidos filtro) =>
        _api.GetAsync<List<PedidoResponse>>(Consulta.Crear("api/pedidos", ("estado", filtro.Estado), ("eventoId", filtro.EventoId), ("busqueda", filtro.Busqueda)));
    public Task<ResultadoApi<PedidoCompletoResponse>> ObtenerAsync(int id) => _api.GetAsync<PedidoCompletoResponse>($"api/pedidos/{id}");
    public Task<ResultadoApi<PedidoCompletoResponse>> ActualizarMetodoPagoAsync(int id, ActualizarMetodoPagoRequest solicitud) =>
        _api.PatchAsync<PedidoCompletoResponse>($"api/pedidos/{id}/metodo-pago", solicitud);
    public async Task<ResultadoApi<int>> LiberarVencidosAsync()
    {
        var r = await _api.PostAsync<System.Text.Json.JsonElement>("api/pedidos/liberacion-vencidos", null);
        return new ResultadoApi<int> { Exito = r.Exito, CodigoEstado = r.CodigoEstado, Mensaje = r.Mensaje,
            Datos = r.Exito && r.Datos.ValueKind == System.Text.Json.JsonValueKind.Object && r.Datos.TryGetProperty("pedidosLiberados", out var n) ? n.GetInt32() : 0 };
    }
    public Task<ResultadoApi<SinContenido>> CancelarAsync(int id) => _api.PostAsync<SinContenido>($"api/pedidos/{id}/cancelacion", null);
}

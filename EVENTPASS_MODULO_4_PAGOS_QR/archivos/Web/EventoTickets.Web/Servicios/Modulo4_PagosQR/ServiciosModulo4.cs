using EventoTickets.Web.Abstracciones.Interfaces;
using EventoTickets.Web.Abstracciones.Modelos;

namespace EventoTickets.Web.Servicios;


public class PagoServicio : IPagoServicio
{
    private readonly IClienteApi _api;
    public PagoServicio(IClienteApi api) => _api = api;

    public async Task<ResultadoApi<PagoResponse>> RegistrarAsync(PagoRegistroRequest solicitud)
    {
        using var contenido = new MultipartFormDataContent
        {
            { new StringContent(solicitud.PedidoId.ToString()), nameof(solicitud.PedidoId) },
            { new StringContent(solicitud.BancoOrigen), nameof(solicitud.BancoOrigen) },
            { new StringContent(solicitud.ReferenciaComprobante), nameof(solicitud.ReferenciaComprobante) },
            { new StringContent(solicitud.MontoReportado.ToString(System.Globalization.CultureInfo.InvariantCulture)), nameof(solicitud.MontoReportado) }
        };
        if (!string.IsNullOrWhiteSpace(solicitud.NotaComprador))
            contenido.Add(new StringContent(solicitud.NotaComprador), nameof(solicitud.NotaComprador));
        if (solicitud.Comprobante is not null)
        {
            var archivo = new StreamContent(solicitud.Comprobante.OpenReadStream());
            archivo.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(
                string.IsNullOrWhiteSpace(solicitud.Comprobante.ContentType) ? "application/octet-stream" : solicitud.Comprobante.ContentType);
            contenido.Add(archivo, nameof(solicitud.Comprobante), solicitud.Comprobante.FileName);
        }
        return await _api.PostMultipartAsync<PagoResponse>("api/pagos", contenido);
    }

    public Task<ResultadoApi<List<PagoResponse>>> ListarAsync(string? estado, string? busqueda) =>
        _api.GetAsync<List<PagoResponse>>(Consulta.Crear("api/pagos", ("estado", estado), ("busqueda", busqueda)));
    public Task<ResultadoApi<List<PagoResponse>>> ListarMiosAsync() => _api.GetAsync<List<PagoResponse>>("api/pagos/mios");
    public Task<ResultadoApi<List<PagoResponse>>> ListarPorPedidoAsync(int pedidoId) => _api.GetAsync<List<PagoResponse>>($"api/pagos/pedido/{pedidoId}");
    public Task<ResultadoApi<PagoResponse>> ObtenerAsync(int id) => _api.GetAsync<PagoResponse>($"api/pagos/{id}");
    public Task<ResultadoApi<ArchivoDescargado>> DescargarComprobanteAsync(int id) => _api.DescargarAsync($"api/pagos/{id}/comprobante");
    public Task<ResultadoApi<PagoResponse>> AprobarAsync(int id, PagoAprobarRequest solicitud) => _api.PostAsync<PagoResponse>($"api/pagos/{id}/aprobacion", solicitud);
    public Task<ResultadoApi<PagoResponse>> RechazarAsync(int id, PagoRechazarRequest solicitud) => _api.PostAsync<PagoResponse>($"api/pagos/{id}/rechazo", solicitud);
}

public class EntradaServicio : IEntradaServicio
{
    private readonly IClienteApi _api;
    public EntradaServicio(IClienteApi api) => _api = api;

    public Task<ResultadoApi<List<EntradaResponse>>> ListarMiasAsync() => _api.GetAsync<List<EntradaResponse>>("api/entradas/mias");
    public Task<ResultadoApi<List<EntradaResponse>>> ListarPorEventoAsync(int eventoId, string? estado) =>
        _api.GetAsync<List<EntradaResponse>>(Consulta.Crear($"api/entradas/evento/{eventoId}", ("estado", estado)));
    public Task<ResultadoApi<EntradaResponse>> ObtenerAsync(int id) => _api.GetAsync<EntradaResponse>($"api/entradas/{id}");
    public Task<ResultadoApi<ArchivoDescargado>> DescargarQrAsync(int id) => _api.DescargarAsync($"api/entradas/{id}/qr");
    public Task<ResultadoApi<EntradaResponse>> TransferirAsync(int id, TransferenciaEntradaRequest solicitud) => _api.PutAsync<EntradaResponse>($"api/entradas/{id}/titular", solicitud);
    public Task<ResultadoApi<EntradaResponse>> RegistrarIngresoAsync(ValidacionAccesoRequest solicitud) => _api.PostAsync<EntradaResponse>("api/entradas/ingresos", solicitud);
}

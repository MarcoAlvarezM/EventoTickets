using EventoTickets.Abstracciones.Configuracion;
using EventoTickets.Abstracciones.Constantes;
using EventoTickets.Abstracciones.Excepciones;
using EventoTickets.Abstracciones.Interfaces.DA;
using EventoTickets.Abstracciones.Interfaces.Flujo;
using EventoTickets.Abstracciones.Interfaces.Reglas;
using EventoTickets.Abstracciones.Interfaces.Servicios;
using EventoTickets.Abstracciones.Modelos;
using Microsoft.Extensions.Options;

namespace EventoTickets.Flujo;


public class PagoFlujo : IPagoFlujo
{
    private readonly IPagoDA _pagoDA;
    private readonly IPedidoDA _pedidoDA;
    private readonly IServicioArchivosComprobantes _almacenamiento;
    private readonly IPagoReglas _pagoReglas;
    private readonly ConfiguracionVentas _ventas;

    public PagoFlujo(IPagoDA pagoDA, IPedidoDA pedidoDA, IServicioArchivosComprobantes almacenamiento,
                     IPagoReglas pagoReglas, IOptions<ConfiguracionVentas> ventas)
    {
        _pagoDA = pagoDA;
        _pedidoDA = pedidoDA;
        _almacenamiento = almacenamiento;
        _pagoReglas = pagoReglas;
        _ventas = ventas.Value;
    }

    public async Task<PagoResponse> RegistrarAsync(int usuarioId, PagoRegistroRequest solicitud)
    {
        var pedido = await _pedidoDA.ObtenerAsync(solicitud.PedidoId) ?? throw new RecursoNoEncontradoException("El pedido no existe.");
        if (pedido.UsuarioId != usuarioId)
            throw new AccesoDenegadoException("El pedido no pertenece al usuario.");
        if (pedido.Estado != EstadosPedido.PendientePago)
            throw new ReglaNegocioException("El pedido no admite reportes de pago en su estado actual.");

        _pagoReglas.ValidarMonto(solicitud.MontoReportado, pedido.ImporteTotal);
        _pagoReglas.ValidarArchivo(solicitud.Comprobante);

        var archivo = await _almacenamiento.GuardarAsync(solicitud.Comprobante!);
        try
        {
            var pagoId = await _pagoDA.RegistrarAsync(new PagoRegistroDatos
            {
                PedidoId = solicitud.PedidoId,
                UsuarioId = usuarioId,
                BancoOrigen = solicitud.BancoOrigen.Trim(),
                ReferenciaComprobante = solicitud.ReferenciaComprobante.Trim().ToUpperInvariant(),
                MontoReportado = solicitud.MontoReportado,
                ComprobanteUrl = archivo.RutaRelativa,
                HashComprobante = archivo.Hash,
                NotaComprador = string.IsNullOrWhiteSpace(solicitud.NotaComprador) ? null : solicitud.NotaComprador.Trim()
            });
            return await _pagoDA.ObtenerAsync(pagoId) ?? throw new RecursoNoEncontradoException("No se pudo recuperar el pago.");
        }
        catch
        {
            // Si la base rechaza el pago (duplicado, vencido...), no se conserva el archivo
            await _almacenamiento.EliminarAsync(archivo.RutaRelativa);
            throw;
        }
    }

    public Task<IEnumerable<PagoResponse>> ListarAsync(string? estado, string? busqueda) => _pagoDA.ListarAsync(estado, busqueda);

    public Task<IEnumerable<PagoResponse>> ListarPorUsuarioAsync(int usuarioId) => _pagoDA.ListarPorUsuarioAsync(usuarioId);

    public async Task<IEnumerable<PagoResponse>> ListarPorPedidoAsync(int pedidoId, int usuarioId, bool esPersonal)
    {
        var pedido = await _pedidoDA.ObtenerAsync(pedidoId) ?? throw new RecursoNoEncontradoException("El pedido no existe.");
        if (!esPersonal && pedido.UsuarioId != usuarioId)
            throw new AccesoDenegadoException("El pedido no pertenece al usuario.");
        return await _pagoDA.ListarPorPedidoAsync(pedidoId);
    }

    public async Task<PagoResponse> ObtenerAsync(int pagoId, int usuarioId, bool esPersonal)
    {
        var pago = await _pagoDA.ObtenerAsync(pagoId) ?? throw new RecursoNoEncontradoException("El pago no existe.");
        if (!esPersonal && pago.CompradorUsuarioId != usuarioId)
            throw new AccesoDenegadoException("El pago no pertenece al usuario.");
        return pago;
    }

    public async Task<ArchivoComprobante> ObtenerComprobanteAsync(int pagoId, int usuarioId, bool esPersonal)
    {
        var pago = await ObtenerAsync(pagoId, usuarioId, esPersonal);
        if (string.IsNullOrWhiteSpace(pago.ComprobanteUrl))
            throw new RecursoNoEncontradoException("El pago no tiene comprobante adjunto.");
        return await _almacenamiento.ObtenerAsync(pago.ComprobanteUrl)
               ?? throw new RecursoNoEncontradoException("El archivo del comprobante no se encuentra en el almacenamiento.");
    }

    public async Task<PagoResponse> AprobarAsync(int pagoId, int auditorUsuarioId, PagoAprobarRequest solicitud)
    {
        await _pagoDA.AprobarAsync(pagoId, auditorUsuarioId, string.IsNullOrWhiteSpace(solicitud.CodigoAprobacion) ? null : solicitud.CodigoAprobacion.Trim());
        return await _pagoDA.ObtenerAsync(pagoId) ?? throw new RecursoNoEncontradoException("El pago no existe.");
    }

    public async Task<PagoResponse> RechazarAsync(int pagoId, int auditorUsuarioId, PagoRechazarRequest solicitud)
    {
        await _pagoDA.RechazarAsync(pagoId, auditorUsuarioId, solicitud.MotivoRechazo.Trim(), _ventas.MinutosGraciaRechazo);
        return await _pagoDA.ObtenerAsync(pagoId) ?? throw new RecursoNoEncontradoException("El pago no existe.");
    }
}

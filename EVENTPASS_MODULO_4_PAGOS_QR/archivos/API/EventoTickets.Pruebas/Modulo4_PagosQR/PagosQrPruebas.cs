using EventoTickets.Abstracciones.Configuracion;
using EventoTickets.Abstracciones.Constantes;
using EventoTickets.Abstracciones.Excepciones;
using EventoTickets.Abstracciones.Interfaces.DA;
using EventoTickets.Abstracciones.Interfaces.Servicios;
using EventoTickets.Abstracciones.Modelos;
using EventoTickets.Flujo;
using EventoTickets.Pruebas.Nucleo;
using EventoTickets.Reglas;
using EventoTickets.Servicios;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace EventoTickets.Pruebas.Modulo4_PagosQR;

/// <summary>HU-15 registro de pago · HU-16 auditoría · HU-17 aprobar/rechazar · HU-18 QR · HU-19 acceso.</summary>
[Trait("Modulo", "M4")]
public class PagosQrPruebas
{
    private static readonly ConfiguracionAlmacenamiento Almacenamiento = new() { TamanoMaximoMB = 1 };
    private readonly PagoReglas _reglas = new(Opciones.De(Almacenamiento));

    private static IFormFile Archivo(string nombre, int bytes = 64)
    {
        var contenido = new byte[bytes];
        new Random(bytes).NextBytes(contenido);
        return new FormFile(new MemoryStream(contenido), 0, contenido.Length, "Comprobante", nombre);
    }

    [Fact]
    public void Comprobante_es_obligatorio() => Assert.Throws<ReglaNegocioException>(() => _reglas.ValidarArchivo(null));

    [Theory]
    [InlineData("comprobante.exe")]
    [InlineData("comprobante.html")]
    [InlineData("comprobante")]
    public void Solo_se_aceptan_imagenes_o_pdf(string nombre) => Assert.Throws<ReglaNegocioException>(() => _reglas.ValidarArchivo(Archivo(nombre)));

    [Theory]
    [InlineData("pago.JPG")]
    [InlineData("pago.png")]
    [InlineData("pago.pdf")]
    public void Formatos_permitidos(string nombre) => _reglas.ValidarArchivo(Archivo(nombre));

    [Fact]
    public void Tamano_maximo_del_comprobante() =>
        Assert.Throws<ReglaNegocioException>(() => _reglas.ValidarArchivo(Archivo("pago.png", 1024 * 1024 + 1)));

    [Fact]
    public void Monto_inferior_al_total_se_rechaza() => Assert.Throws<ReglaNegocioException>(() => _reglas.ValidarMonto(99, 100));

    private static (PagoFlujo Flujo, Fake<IPagoDA> Pagos, Fake<IPedidoDA> Pedidos, Fake<IServicioArchivosComprobantes> Archivos) CrearFlujo()
    {
        var (pagos, cp) = Fake<IPagoDA>.Crear();
        var (pedidos, cpe) = Fake<IPedidoDA>.Crear();
        var (archivos, ca) = Fake<IServicioArchivosComprobantes>.Crear();
        cpe.Cuando("ObtenerAsync", _ => new PedidoCompletoResponse { PedidoId = 1, UsuarioId = 5, Estado = EstadosPedido.PendientePago, ImporteTotal = 1000 });
        ca.Cuando("GuardarAsync", _ => new ComprobanteGuardado { RutaRelativa = "2026/11/x.png", Hash = new string('a', 64) });
        return (new PagoFlujo(pagos, pedidos, archivos, new PagoReglas(Opciones.De(Almacenamiento)), Opciones.De(new ConfiguracionVentas())), cp, cpe, ca);
    }

    private static PagoRegistroRequest Reporte(int pedidoId = 1, decimal monto = 1000) => new()
    {
        PedidoId = pedidoId, BancoOrigen = " BAC ", ReferenciaComprobante = "sinpe-123", MontoReportado = monto, Comprobante = Archivo("pago.png")
    };

    [Fact]
    public async Task Registrar_guarda_hash_y_referencia_normalizada()
    {
        var (flujo, pagos, _, _) = CrearFlujo();
        pagos.Cuando("RegistrarAsync", _ => 10).Cuando("ObtenerAsync", _ => new PagoResponse { PagoId = 10 });
        await flujo.RegistrarAsync(5, Reporte());
        var datos = (PagoRegistroDatos)pagos.Argumentos("RegistrarAsync")[0]!;
        Assert.Equal("SINPE-123", datos.ReferenciaComprobante);
        Assert.Equal("BAC", datos.BancoOrigen);
        Assert.Equal(64, datos.HashComprobante.Length);
    }

    [Fact]
    public async Task Si_la_base_rechaza_el_pago_se_elimina_el_archivo()
    {
        var (flujo, pagos, _, archivos) = CrearFlujo();
        pagos.Cuando("RegistrarAsync", _ => throw new ConflictoException("Comprobante duplicado"));
        await Assert.ThrowsAsync<ConflictoException>(() => flujo.RegistrarAsync(5, Reporte()));
        Assert.Equal(1, archivos.Veces("EliminarAsync"));
        Assert.Equal("2026/11/x.png", (string)archivos.Argumentos("EliminarAsync")[0]!);
    }

    [Fact]
    public async Task No_se_reporta_pago_de_un_pedido_ajeno_ni_se_guarda_el_archivo()
    {
        var (flujo, _, _, archivos) = CrearFlujo();
        await Assert.ThrowsAsync<AccesoDenegadoException>(() => flujo.RegistrarAsync(usuarioId: 6, Reporte()));
        Assert.Equal(0, archivos.Veces("GuardarAsync"));
    }

    [Fact]
    public async Task Comprador_no_ve_pagos_ni_comprobantes_ajenos()
    {
        var (flujo, pagos, _, _) = CrearFlujo();
        pagos.Cuando("ObtenerAsync", _ => new PagoResponse { PagoId = 3, CompradorUsuarioId = 6, ComprobanteUrl = "x.png" });
        await Assert.ThrowsAsync<AccesoDenegadoException>(() => flujo.ObtenerComprobanteAsync(3, usuarioId: 5, esPersonal: false));
        await Assert.ThrowsAsync<AccesoDenegadoException>(() => flujo.ListarPorPedidoAsync(1, usuarioId: 6, esPersonal: false));
    }

    [Fact]
    public async Task Rechazo_envia_el_motivo_y_los_minutos_de_gracia()
    {
        var (flujo, pagos, _, _) = CrearFlujo();
        pagos.Cuando("ObtenerAsync", _ => new PagoResponse { PagoId = 3 });
        await flujo.RechazarAsync(3, 3, new PagoRechazarRequest { MotivoRechazo = "  Monto distinto  " });
        var a = pagos.Argumentos("RechazarAsync");
        Assert.Equal("Monto distinto", (string)a[2]!);
        Assert.Equal(60, (int)a[3]!);
    }

    [Fact]
    public async Task Lectura_del_QR_separa_codigo_y_hash()
    {
        var (da, control) = Fake<IEntradaDA>.Crear();
        await new EntradaFlujo(da).RegistrarIngresoAsync(new ValidacionAccesoRequest { CodigoTicket = " TKT-001|abc123 " }, 4);
        var enviado = (ValidacionAccesoRequest)control.Argumentos("RegistrarIngresoAsync")[0]!;
        Assert.Equal("TKT-001", enviado.CodigoTicket);
        Assert.Equal("abc123", enviado.HashCriptografico);
        Assert.Equal(4, (int)control.Argumentos("RegistrarIngresoAsync")[1]!);
    }

    [Fact]
    public async Task Codigo_digitado_a_mano_no_requiere_hash()
    {
        var (da, control) = Fake<IEntradaDA>.Crear();
        await new EntradaFlujo(da).RegistrarIngresoAsync(new ValidacionAccesoRequest { CodigoTicket = "TKT-002" }, 4);
        Assert.Null(((ValidacionAccesoRequest)control.Argumentos("RegistrarIngresoAsync")[0]!).HashCriptografico);
    }

    [Fact]
    public async Task Comprador_no_ve_entradas_ajenas()
    {
        var (da, control) = Fake<IEntradaDA>.Crear();
        control.Cuando("ObtenerAsync", _ => new EntradaResponse { EntradaId = 1, UsuarioId = 6 });
        await Assert.ThrowsAsync<AccesoDenegadoException>(() => new EntradaFlujo(da).ObtenerAsync(1, usuarioId: 5, esPersonal: false));
    }

    private sealed class Entorno : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = "Testing";
        public string ApplicationName { get; set; } = "Pruebas";
        public string ContentRootPath { get; set; } = Path.Combine(Path.GetTempPath(), "eventcr-pruebas-" + Guid.NewGuid().ToString("N"));
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }

    [Fact]
    public async Task Comprobantes_se_guardan_fuera_de_wwwroot_con_hash_y_se_eliminan()
    {
        var entorno = new Entorno();
        var servicio = new ServicioArchivosComprobantes(Opciones.De(new ConfiguracionAlmacenamiento()), entorno, new ServicioHash());
        try
        {
            var archivo = Archivo("pago.png", 128);
            var guardado = await servicio.GuardarAsync(archivo);
            Assert.DoesNotContain("wwwroot", guardado.RutaRelativa);
            Assert.Matches("^[0-9a-f]{64}$", guardado.Hash);
            var leido = await servicio.ObtenerAsync(guardado.RutaRelativa);
            Assert.NotNull(leido);
            Assert.Equal("image/png", leido!.TipoContenido);
            leido.Contenido.Dispose();
            await servicio.EliminarAsync(guardado.RutaRelativa);
            Assert.Null(await servicio.ObtenerAsync(guardado.RutaRelativa));
        }
        finally
        {
            if (Directory.Exists(entorno.ContentRootPath)) Directory.Delete(entorno.ContentRootPath, true);
        }
    }

    [Theory]
    [InlineData("../../etc/passwd")]
    [InlineData("../ComprobantesFalsos/x.png")]
    public async Task Rutas_fuera_de_la_carpeta_de_comprobantes_se_rechazan(string ruta)
    {
        var servicio = new ServicioArchivosComprobantes(Opciones.De(new ConfiguracionAlmacenamiento { RutaComprobantes = "Comprobantes" }), new Entorno(), new ServicioHash());
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => servicio.ObtenerAsync(ruta));
    }

    /// <summary>Usa la librería QRCoder real: en un entorno sin paquetes NuGet reales no es significativa.</summary>
    [Fact]
    [Trait("Requiere", "PaquetesReales")]
    public void QR_se_genera_como_PNG_valido()
    {
        var png = new ServicioQr().GenerarPng("TKT-001|" + new string('a', 64));
        Assert.True(png.Length > 200, "El PNG es demasiado pequeño para contener un QR.");
        Assert.Equal(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }, png.Take(8).ToArray());
    }
}

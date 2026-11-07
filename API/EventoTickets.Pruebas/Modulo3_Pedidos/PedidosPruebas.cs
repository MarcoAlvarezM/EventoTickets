using EventoTickets.Abstracciones.Configuracion;
using EventoTickets.Abstracciones.Constantes;
using EventoTickets.Abstracciones.Excepciones;
using EventoTickets.Abstracciones.Interfaces.DA;
using EventoTickets.Abstracciones.Interfaces.Flujo;
using EventoTickets.Abstracciones.Modelos;
using EventoTickets.Flujo;
using EventoTickets.Pruebas.Nucleo;
using EventoTickets.Reglas;
using EventoTickets.Servicios;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace EventoTickets.Pruebas.Modulo3_Pedidos;

/// <summary>HU-12 crear pedido · HU-13 historial · HU-14 cancelación y vencimiento.</summary>
[Trait("Modulo", "M3")]
public class PedidosPruebas
{
    private static readonly ConfiguracionVentas Ventas = new() { PorcentajeCargoServicio = 10, PorcentajeIVA = 13, MaxEntradasPorPedido = 10, MinutosReserva = 30 };
    private readonly PedidoReglas _reglas = new(Opciones.De(Ventas));

    private static List<TipoEntradaResponse> Tipos() => new()
    {
        new() { TipoEntradaId = 1, EventoId = 1, Nombre = "VIP", Precio = 180000, Disponible = 5, Activo = true },
        new() { TipoEntradaId = 2, EventoId = 1, Nombre = "General", Precio = 85000, Disponible = 100, Activo = true },
        new() { TipoEntradaId = 3, EventoId = 1, Nombre = "Inactivo", Precio = 1000, Disponible = 100, Activo = false }
    };

    [Fact]
    public void Cotizacion_usa_la_misma_formula_que_sp_Pedido_Crear()
    {
        var c = _reglas.Cotizar(1, new[] { new PedidoLineaRequest { TipoEntradaId = 1, Cantidad = 2 } }, Tipos());
        Assert.Equal(360000m, c.Subtotal);
        Assert.Equal(36000m, c.CargoServicio);
        Assert.Equal(51480m, c.ImpuestoIVA);
        Assert.Equal(447480m, c.ImporteTotal);
    }

    [Fact]
    public void Lineas_repetidas_se_agrupan_y_las_vacias_se_descartan()
    {
        var l = _reglas.NormalizarLineas(new[]
        {
            new PedidoLineaRequest { TipoEntradaId = 2, Cantidad = 1 }, new PedidoLineaRequest { TipoEntradaId = 2, Cantidad = 2 },
            new PedidoLineaRequest { TipoEntradaId = 1, Cantidad = 0 }
        });
        Assert.Equal(3, Assert.Single(l).Cantidad);
    }

    [Fact]
    public void Pedido_vacio_se_rechaza() =>
        Assert.Throws<ReglaNegocioException>(() => _reglas.NormalizarLineas(Array.Empty<PedidoLineaRequest>()));

    [Fact]
    public void Maximo_de_entradas_por_pedido() =>
        Assert.Throws<ReglaNegocioException>(() => _reglas.NormalizarLineas(new[] { new PedidoLineaRequest { TipoEntradaId = 2, Cantidad = 11 } }));

    [Fact]
    public void Sin_cupo_suficiente_responde_conflicto() =>
        Assert.Throws<ConflictoException>(() => _reglas.Cotizar(1, new[] { new PedidoLineaRequest { TipoEntradaId = 1, Cantidad = 6 } }, Tipos()));

    [Fact]
    public void Tipo_inactivo_o_de_otro_evento_se_rechaza()
    {
        Assert.Throws<ReglaNegocioException>(() => _reglas.Cotizar(1, new[] { new PedidoLineaRequest { TipoEntradaId = 3, Cantidad = 1 } }, Tipos()));
        Assert.Throws<ReglaNegocioException>(() => _reglas.Cotizar(1, new[] { new PedidoLineaRequest { TipoEntradaId = 99, Cantidad = 1 } }, Tipos()));
    }

    private static (PedidoFlujo Flujo, Fake<IPedidoDA> Pedidos, Fake<IEventoDA> Eventos) CrearFlujo(string estadoEvento = EstadosEvento.Publicado)
    {
        var (pedidos, cp) = Fake<IPedidoDA>.Crear();
        var (eventos, ce) = Fake<IEventoDA>.Crear();
        var (tipos, ct) = Fake<ITipoEntradaDA>.Crear();
        ce.Cuando("ObtenerAsync", _ => new EventoResponse { EventoId = 1, Estado = estadoEvento, FechaEvento = DateTime.Today.AddDays(10) });
        ct.Cuando("ListarPorEventoAsync", _ => Tipos());
        var flujo = new PedidoFlujo(pedidos, eventos, tipos, new PedidoReglas(Opciones.De(Ventas)), Opciones.De(Ventas));
        return (flujo, cp, ce);
    }

    private static PedidoRequest Solicitud(int cantidad = 2) => new() { EventoId = 1, MetodoPago = MetodosPago.SinpeMovil, Lineas = { new PedidoLineaRequest { TipoEntradaId = 2, Cantidad = cantidad } } };

    [Theory]
    [InlineData(EstadosEvento.Agotado)]
    [InlineData(EstadosEvento.Borrador)]
    [InlineData(EstadosEvento.Cancelado)]
    public async Task Solo_se_vende_en_eventos_publicados(string estado)
    {
        var (flujo, pedidos, _) = CrearFlujo(estado);
        await Assert.ThrowsAsync<ReglaNegocioException>(() => flujo.CrearAsync(5, Solicitud()));
        Assert.Equal(0, pedidos.Veces("CrearAsync"));
    }

    [Fact]
    public async Task Crear_envia_la_configuracion_de_ventas_al_procedimiento()
    {
        var (flujo, pedidos, _) = CrearFlujo();
        pedidos.Cuando("CrearAsync", _ => 77).Cuando("ObtenerAsync", _ => new PedidoCompletoResponse { PedidoId = 77, UsuarioId = 5 });
        var pedido = await flujo.CrearAsync(5, Solicitud());
        var a = pedidos.Argumentos("CrearAsync");
        Assert.Equal(77, pedido.PedidoId);
        Assert.Equal(5, (int)a[0]!);
        Assert.Equal(10m, (decimal)a[4]!);
        Assert.Equal(13m, (decimal)a[5]!);
        Assert.Equal(30, (int)a[6]!);
        Assert.Equal(10, (int)a[7]!);
    }

    [Fact]
    public async Task Comprador_no_ve_pedidos_ajenos()
    {
        var (flujo, pedidos, _) = CrearFlujo();
        pedidos.Cuando("ObtenerAsync", _ => new PedidoCompletoResponse { PedidoId = 1, UsuarioId = 6 });
        await Assert.ThrowsAsync<AccesoDenegadoException>(() => flujo.ObtenerAsync(1, usuarioId: 5, esPersonal: false));
        Assert.Equal(1, (await flujo.ObtenerAsync(1, usuarioId: 2, esPersonal: true)).PedidoId);
    }

    [Fact]
    public async Task Cancelar_como_comprador_envia_el_propietario_y_como_personal_null()
    {
        var (flujo, pedidos, _) = CrearFlujo();
        await flujo.CancelarAsync(1, 5, esPersonal: false);
        Assert.Equal<object?>(5, pedidos.Argumentos("CancelarAsync")[1]);
        await flujo.CancelarAsync(1, 2, esPersonal: true);
        Assert.Null(pedidos.Argumentos("CancelarAsync")[1]);
    }

    [Fact]
    public async Task Servicio_de_expiracion_invoca_la_liberacion_sin_visitas_a_paginas()
    {
        var (flujo, control) = Fake<IPedidoFlujo>.Crear();
        var llamado = new TaskCompletionSource();
        control.Cuando("LiberarVencidosAsync", _ => { llamado.TrySetResult(); return 2; });
        var proveedor = new ServiceCollection().AddScoped(_ => flujo).BuildServiceProvider();
        var servicio = new ExpiracionReservasServicio(proveedor.GetRequiredService<IServiceScopeFactory>(),
            NullLogger<ExpiracionReservasServicio>.Instance, Opciones.De(new ConfiguracionVentas { SegundosRevisionExpiracion = 60 }));

        await servicio.StartAsync(CancellationToken.None);
        var termino = await Task.WhenAny(llamado.Task, Task.Delay(TimeSpan.FromSeconds(5)));
        await servicio.StopAsync(CancellationToken.None);
        Assert.True(termino == llamado.Task, "El servicio no ejecutó LiberarVencidosAsync al iniciar.");
    }

    [Fact]
    public async Task Servicio_de_expiracion_se_desactiva_con_cero()
    {
        var (flujo, control) = Fake<IPedidoFlujo>.Crear();
        var proveedor = new ServiceCollection().AddScoped(_ => flujo).BuildServiceProvider();
        var servicio = new ExpiracionReservasServicio(proveedor.GetRequiredService<IServiceScopeFactory>(),
            NullLogger<ExpiracionReservasServicio>.Instance, Opciones.De(new ConfiguracionVentas { SegundosRevisionExpiracion = 0 }));
        await servicio.StartAsync(CancellationToken.None);
        await Task.Delay(300);
        await servicio.StopAsync(CancellationToken.None);
        Assert.Equal(0, control.Veces("LiberarVencidosAsync"));
    }

    [Fact]
    public async Task Fallo_de_la_base_no_detiene_el_servicio_de_expiracion()
    {
        var (flujo, control) = Fake<IPedidoFlujo>.Crear();
        var llamado = new TaskCompletionSource();
        control.Cuando("LiberarVencidosAsync", _ => { llamado.TrySetResult(); throw new InvalidOperationException("BD caída"); });
        var proveedor = new ServiceCollection().AddScoped(_ => flujo).BuildServiceProvider();
        var servicio = new ExpiracionReservasServicio(proveedor.GetRequiredService<IServiceScopeFactory>(),
            NullLogger<ExpiracionReservasServicio>.Instance, Opciones.De(new ConfiguracionVentas { SegundosRevisionExpiracion = 60 }));
        await servicio.StartAsync(CancellationToken.None);
        await Task.WhenAny(llamado.Task, Task.Delay(TimeSpan.FromSeconds(5)));
        await Task.Delay(100);
        Assert.False(servicio.ExecuteTask?.IsFaulted ?? false);
        await servicio.StopAsync(CancellationToken.None);
    }
}

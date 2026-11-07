using System.ComponentModel.DataAnnotations;
using EventoTickets.Abstracciones.Constantes;
using EventoTickets.Abstracciones.Excepciones;
using EventoTickets.Abstracciones.Interfaces.DA;
using EventoTickets.Abstracciones.Modelos;
using EventoTickets.Flujo;
using EventoTickets.Pruebas.Nucleo;
using EventoTickets.Reglas;
using Xunit;

namespace EventoTickets.Pruebas.Modulo2_Eventos;

/// <summary>HU-04 crear/publicar · HU-05 editar y estados · HU-06 cartelera · HU-09/10 tipos · HU-11 disponibilidad.</summary>
[Trait("Modulo", "M2")]
public class EventosPruebas
{
    private readonly EventoReglas _reglas = new();

    [Theory]
    [InlineData(EstadosEvento.Borrador, EstadosEvento.Publicado)]
    [InlineData(EstadosEvento.Publicado, EstadosEvento.Borrador)]
    [InlineData(EstadosEvento.Publicado, EstadosEvento.Finalizado)]
    [InlineData(EstadosEvento.Publicado, EstadosEvento.Cancelado)]
    [InlineData(EstadosEvento.Agotado, EstadosEvento.Finalizado)]
    [InlineData(EstadosEvento.Agotado, EstadosEvento.Cancelado)]
    public void Transiciones_permitidas(string actual, string nuevo) => _reglas.ValidarTransicionEstado(actual, nuevo);

    [Theory]
    [InlineData(EstadosEvento.Cancelado, EstadosEvento.Publicado)]
    [InlineData(EstadosEvento.Finalizado, EstadosEvento.Publicado)]
    [InlineData(EstadosEvento.Borrador, EstadosEvento.Agotado)]
    [InlineData(EstadosEvento.Borrador, EstadosEvento.Finalizado)]
    [InlineData(EstadosEvento.Publicado, EstadosEvento.Publicado)]
    public void Transiciones_no_permitidas(string actual, string nuevo) =>
        Assert.Throws<ReglaNegocioException>(() => _reglas.ValidarTransicionEstado(actual, nuevo));

    [Fact]
    public void Fecha_pasada_no_se_programa() =>
        Assert.Throws<ReglaNegocioException>(() => _reglas.ValidarFechaProgramada(DateTime.Today.AddDays(-1)));

    private static EventoRequest EventoValido() => new()
    {
        CategoriaId = 1, LocacionId = 1, Nombre = "Concierto de prueba", DescripcionCompleta = "Descripción con más de veinte caracteres.",
        FechaEvento = DateTime.Today.AddDays(10), HoraEvento = new TimeSpan(19, 0, 0)
    };

    private static List<ValidationResult> Validar(object m)
    {
        var e = new List<ValidationResult>();
        Validator.TryValidateObject(m, new ValidationContext(m), e, true);
        return e;
    }

    [Fact]
    public void Codigo_de_referencia_es_opcional_se_autogenera_en_BD() => Assert.Empty(Validar(EventoValido()));

    [Fact]
    public void Codigo_de_referencia_con_formato_invalido_se_rechaza()
    {
        var m = EventoValido();
        m.CodigoReferencia = "evt 2026";
        Assert.Contains(nameof(EventoRequest.CodigoReferencia), Validar(m).SelectMany(e => e.MemberNames));
    }

    [Fact]
    public void Evento_sin_categoria_se_rechaza()
    {
        var m = EventoValido();
        m.CategoriaId = 0;
        Assert.Contains(nameof(EventoRequest.CategoriaId), Validar(m).SelectMany(e => e.MemberNames));
    }

    [Fact]
    public async Task Cartelera_publica_fuerza_Publicado_o_Agotado_y_fechas_vigentes()
    {
        var (da, control) = Fake<IEventoDA>.Crear();
        var filtro = new FiltroEventos { Estado = EstadosEvento.Borrador, Ciudad = "Heredia" };
        await new EventoFlujo(da, _reglas).ListarAsync(filtro, esPersonal: false);
        var enviado = (FiltroEventos)control.Argumentos("ListarAsync")[0]!;
        Assert.Null(enviado.Estado);
        Assert.True(enviado.SoloCartelera);
        Assert.True(enviado.SoloVigentes);
        Assert.Equal("Heredia", enviado.Ciudad);
    }

    [Fact]
    public async Task Personal_puede_filtrar_por_cualquier_estado()
    {
        var (da, control) = Fake<IEventoDA>.Crear();
        await new EventoFlujo(da, _reglas).ListarAsync(new FiltroEventos { Estado = EstadosEvento.Borrador }, esPersonal: true);
        var enviado = (FiltroEventos)control.Argumentos("ListarAsync")[0]!;
        Assert.Equal(EstadosEvento.Borrador, enviado.Estado);
        Assert.False(enviado.SoloCartelera);
    }

    [Fact]
    public async Task Borrador_no_es_visible_al_publico()
    {
        var (da, control) = Fake<IEventoDA>.Crear();
        control.Cuando("ObtenerAsync", _ => new EventoResponse { EventoId = 9, Estado = EstadosEvento.Borrador });
        await Assert.ThrowsAsync<RecursoNoEncontradoException>(() => new EventoFlujo(da, _reglas).ObtenerAsync(9, esPersonal: false));
    }

    [Fact]
    public async Task Crear_evento_siempre_inicia_en_Borrador()
    {
        var (da, control) = Fake<IEventoDA>.Crear();
        control.Cuando("CrearAsync", _ => 7).Cuando("ObtenerAsync", _ => new EventoResponse { EventoId = 7, Estado = EstadosEvento.Borrador });
        await new EventoFlujo(da, _reglas).CrearAsync(EventoValido());
        Assert.Equal(EstadosEvento.Borrador, (string)control.Argumentos("CrearAsync")[1]!);
    }

    [Fact]
    public async Task No_se_edita_un_evento_cancelado()
    {
        var (da, control) = Fake<IEventoDA>.Crear();
        control.Cuando("ObtenerAsync", _ => new EventoResponse { EventoId = 3, Estado = EstadosEvento.Cancelado, FechaEvento = DateTime.Today.AddDays(5) });
        await Assert.ThrowsAsync<ReglaNegocioException>(() => new EventoFlujo(da, _reglas).ActualizarAsync(3, EventoValido()));
        Assert.Equal(0, control.Veces("ActualizarAsync"));
    }
}

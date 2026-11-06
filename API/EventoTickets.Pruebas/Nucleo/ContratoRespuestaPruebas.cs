using System.Text.Json;
using EventoTickets.Abstracciones.Excepciones;
using EventoTickets.Abstracciones.Modelos;
using EventoTickets.API.Middleware;
using EventoTickets.API.Nucleo.Filtros;
using EventoTickets.Servicios;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace EventoTickets.Pruebas.Nucleo;

/// <summary>Contratos compartidos: formato único de respuesta (RespuestaApi), errores y hash.</summary>
[Trait("Modulo", "Nucleo")]
public class ContratoRespuestaPruebas
{
    private static async Task<object?> FiltrarAsync(IActionResult resultado)
    {
        var contexto = new ResultExecutingContext(
            new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor()),
            new List<IFilterMetadata>(), resultado, new object());
        await new RespuestaApiFiltro().OnResultExecutionAsync(contexto, () => Task.FromResult<ResultExecutedContext>(null!));
        return contexto.Result;
    }

    [Fact]
    public async Task Filtro_envuelve_Ok_en_RespuestaApi()
    {
        var datos = new { Nombre = "Evento" };
        var resultado = (ObjectResult)(await FiltrarAsync(new OkObjectResult(datos)))!;
        var sobre = Assert.IsType<RespuestaApi<object>>(resultado.Value);
        Assert.True(sobre.Exito);
        Assert.Equal(200, sobre.Codigo);
        Assert.Equal<object?>(datos, sobre.Datos);
    }

    [Fact]
    public async Task Filtro_respeta_codigo_201()
    {
        var resultado = (ObjectResult)(await FiltrarAsync(new ObjectResult(5) { StatusCode = 201 }))!;
        var sobre = Assert.IsType<RespuestaApi<object>>(resultado.Value);
        Assert.Equal(201, sobre.Codigo);
    }

    [Fact]
    public async Task Filtro_no_envuelve_dos_veces()
    {
        var original = RespuestaApi.Mensaje("Listo");
        var resultado = (ObjectResult)(await FiltrarAsync(new OkObjectResult(original)))!;
        Assert.Equal<object?>(original, resultado.Value);
    }

    [Fact]
    public async Task Filtro_no_toca_archivos()
    {
        var archivo = new FileContentResult(new byte[] { 1, 2 }, "image/png");
        Assert.Equal<object?>(archivo, await FiltrarAsync(archivo));
    }

    private static async Task<(int Codigo, JsonElement Cuerpo)> EjecutarMiddlewareAsync(Exception ex)
    {
        var contexto = new DefaultHttpContext();
        contexto.Response.Body = new MemoryStream();
        var middleware = new ManejoExcepcionesMiddleware(NullLogger<ManejoExcepcionesMiddleware>.Instance);
        await middleware.InvokeAsync(contexto, _ => throw ex);
        contexto.Response.Body.Position = 0;
        var cuerpo = await JsonDocument.ParseAsync(contexto.Response.Body);
        return (contexto.Response.StatusCode, cuerpo.RootElement.Clone());
    }

    [Theory]
    [InlineData(typeof(ReglaNegocioException), 400)]
    [InlineData(typeof(AutenticacionException), 401)]
    [InlineData(typeof(AccesoDenegadoException), 403)]
    [InlineData(typeof(RecursoNoEncontradoException), 404)]
    [InlineData(typeof(ConflictoException), 409)]
    public async Task Middleware_traduce_excepciones_de_dominio(Type tipo, int codigo)
    {
        var ex = (Exception)Activator.CreateInstance(tipo, tipo == typeof(ReglaNegocioException) ? new object?[] { "Mensaje de prueba", null } : new object?[] { "Mensaje de prueba" })!;
        var (estado, cuerpo) = await EjecutarMiddlewareAsync(ex);
        Assert.Equal(codigo, estado);
        Assert.False(cuerpo.GetProperty("exito").GetBoolean());
        Assert.Equal(codigo, cuerpo.GetProperty("codigo").GetInt32());
        Assert.Equal("Mensaje de prueba", cuerpo.GetProperty("mensaje").GetString());
    }

    [Fact]
    public async Task Middleware_no_expone_detalles_internos_en_500()
    {
        var (estado, cuerpo) = await EjecutarMiddlewareAsync(new InvalidOperationException("Server=prod;Password=secreta"));
        Assert.Equal(500, estado);
        Assert.DoesNotContain("secreta", cuerpo.GetRawText());
        Assert.Contains("error inesperado", cuerpo.GetProperty("mensaje").GetString());
    }

    [Fact]
    public void ServicioHash_calcula_SHA256_conocido()
    {
        Assert.Equal("ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad", new ServicioHash().CalcularSha256("abc"));
    }
}

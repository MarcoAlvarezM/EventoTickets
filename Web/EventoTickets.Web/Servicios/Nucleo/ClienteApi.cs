using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using EventoTickets.Web.Abstracciones.Interfaces;
using EventoTickets.Web.Abstracciones.Modelos;
using EventoTickets.Web.Infraestructura;

namespace EventoTickets.Web.Servicios;


/// <summary>
/// Única puerta de salida hacia el API REST. Adjunta el JWT guardado en la cookie de sesión
/// y convierte las respuestas (incluidos ProblemDetails) en <see cref="ResultadoApi{T}"/>.
/// </summary>
public class ClienteApi : IClienteApi
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private readonly HttpClient _http;
    private readonly IHttpContextAccessor _contexto;
    private readonly ILogger<ClienteApi> _logger;

    public ClienteApi(HttpClient http, IHttpContextAccessor contexto, ILogger<ClienteApi> logger)
    {
        _http = http;
        _contexto = contexto;
        _logger = logger;
    }

    public Task<ResultadoApi<T>> GetAsync<T>(string ruta) => EnviarAsync<T>(HttpMethod.Get, ruta, null);
    public Task<ResultadoApi<T>> PostAsync<T>(string ruta, object? cuerpo) => EnviarAsync<T>(HttpMethod.Post, ruta, Serializar(cuerpo));
    public Task<ResultadoApi<T>> PutAsync<T>(string ruta, object? cuerpo) => EnviarAsync<T>(HttpMethod.Put, ruta, Serializar(cuerpo));
    public Task<ResultadoApi<T>> PatchAsync<T>(string ruta, object? cuerpo) => EnviarAsync<T>(HttpMethod.Patch, ruta, Serializar(cuerpo));
    public Task<ResultadoApi<T>> DeleteAsync<T>(string ruta) => EnviarAsync<T>(HttpMethod.Delete, ruta, null);
    public Task<ResultadoApi<T>> PostMultipartAsync<T>(string ruta, MultipartFormDataContent contenido) =>
        EnviarAsync<T>(HttpMethod.Post, ruta, contenido);

    public async Task<ResultadoApi<ArchivoDescargado>> DescargarAsync(string ruta)
    {
        try
        {
            using var solicitud = CrearSolicitud(HttpMethod.Get, ruta, null);
            using var respuesta = await _http.SendAsync(solicitud);
            if (!respuesta.IsSuccessStatusCode)
                return await LeerErrorAsync<ArchivoDescargado>(respuesta);

            return new ResultadoApi<ArchivoDescargado>
            {
                Exito = true,
                CodigoEstado = (int)respuesta.StatusCode,
                Datos = new ArchivoDescargado
                {
                    Contenido = await respuesta.Content.ReadAsByteArrayAsync(),
                    TipoContenido = respuesta.Content.Headers.ContentType?.MediaType ?? "application/octet-stream",
                    NombreArchivo = respuesta.Content.Headers.ContentDisposition?.FileNameStar
                                    ?? respuesta.Content.Headers.ContentDisposition?.FileName?.Trim('"') ?? "comprobante"
                }
            };
        }
        catch (HttpRequestException ex)
        {
            return SinConexion<ArchivoDescargado>(ex);
        }
    }

    private async Task<ResultadoApi<T>> EnviarAsync<T>(HttpMethod metodo, string ruta, HttpContent? contenido)
    {
        try
        {
            using var solicitud = CrearSolicitud(metodo, ruta, contenido);
            using var respuesta = await _http.SendAsync(solicitud);

            if (!respuesta.IsSuccessStatusCode)
                return await LeerErrorAsync<T>(respuesta);

            var resultado = new ResultadoApi<T> { Exito = true, CodigoEstado = (int)respuesta.StatusCode };
            if (respuesta.StatusCode != HttpStatusCode.NoContent && respuesta.Content.Headers.ContentLength != 0)
            {
                // Contrato común: { exito, codigo, mensaje, datos, errores, codigoError }
                if (typeof(T) == typeof(SinContenido))
                {
                    var sobre = await respuesta.Content.ReadFromJsonAsync<SobreApi<JsonElement>>(Json);
                    resultado.Mensaje = sobre?.Mensaje;
                }
                else
                {
                    var sobre = await respuesta.Content.ReadFromJsonAsync<SobreApi<T>>(Json);
                    resultado.Datos = sobre is null ? default : sobre.Datos;
                    resultado.Mensaje = sobre?.Mensaje;
                }
            }
            return resultado;
        }
        catch (HttpRequestException ex)
        {
            return SinConexion<T>(ex);
        }
        catch (TaskCanceledException ex)
        {
            return SinConexion<T>(ex);
        }
    }

    private HttpRequestMessage CrearSolicitud(HttpMethod metodo, string ruta, HttpContent? contenido)
    {
        var solicitud = new HttpRequestMessage(metodo, ruta.TrimStart('/')) { Content = contenido };
        var token = _contexto.HttpContext?.User.FindFirst(ReclamosSesion.Token)?.Value;
        if (!string.IsNullOrEmpty(token))
            solicitud.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return solicitud;
    }

    private static HttpContent? Serializar(object? cuerpo) => cuerpo is null ? null : JsonContent.Create(cuerpo, options: Json);

    /// <summary>Forma del JSON común que devuelve el API (RespuestaApi&lt;T&gt;).</summary>
    private sealed class SobreApi<TDatos>
    {
        public bool Exito { get; set; }
        public string? Mensaje { get; set; }
        public TDatos? Datos { get; set; }
    }

    /// <summary>Lee la RespuestaApi de error (mensaje + errores por campo) devuelta por el API.</summary>
    private static async Task<ResultadoApi<T>> LeerErrorAsync<T>(HttpResponseMessage respuesta)
    {
        var resultado = new ResultadoApi<T> { Exito = false, CodigoEstado = (int)respuesta.StatusCode };
        try
        {
            var texto = await respuesta.Content.ReadAsStringAsync();
            if (!string.IsNullOrWhiteSpace(texto))
            {
                using var doc = JsonDocument.Parse(texto);
                var raiz = doc.RootElement;
                if (raiz.TryGetProperty("mensaje", out var mensaje) && mensaje.ValueKind == JsonValueKind.String)
                    resultado.Mensaje = mensaje.GetString();

                if (raiz.TryGetProperty("errores", out var errores) && errores.ValueKind == JsonValueKind.Object)
                {
                    foreach (var campo in errores.EnumerateObject())
                        resultado.Errores[campo.Name] = campo.Value.EnumerateArray().Select(e => e.GetString() ?? string.Empty).ToArray();
                }
            }
        }
        catch (JsonException)
        {
            // La respuesta no era JSON: se usa el mensaje genérico
        }

        resultado.Mensaje ??= respuesta.StatusCode switch
        {
            HttpStatusCode.Unauthorized => "Su sesión expiró. Inicie sesión nuevamente.",
            HttpStatusCode.Forbidden => "No tiene permisos para realizar esta acción.",
            HttpStatusCode.NotFound => "El recurso solicitado no existe.",
            _ => "No fue posible completar la operación."
        };
        return resultado;
    }

    private ResultadoApi<T> SinConexion<T>(Exception ex)
    {
        _logger.LogError(ex, "No se pudo contactar el API en {Url}", _http.BaseAddress);
        return new ResultadoApi<T>
        {
            Exito = false,
            CodigoEstado = 503,
            Mensaje = "El servicio no está disponible en este momento. Verifique que el API esté en ejecución."
        };
    }
}

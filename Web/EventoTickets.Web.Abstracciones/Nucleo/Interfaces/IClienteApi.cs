using EventoTickets.Web.Abstracciones.Modelos;

namespace EventoTickets.Web.Abstracciones.Interfaces;


/// <summary>Cliente HTTP único hacia el API REST; adjunta el token JWT de la sesión.</summary>
public interface IClienteApi
{
    Task<ResultadoApi<T>> GetAsync<T>(string ruta);
    Task<ResultadoApi<T>> PostAsync<T>(string ruta, object? cuerpo);
    Task<ResultadoApi<T>> PutAsync<T>(string ruta, object? cuerpo);
    Task<ResultadoApi<T>> PatchAsync<T>(string ruta, object? cuerpo);
    Task<ResultadoApi<T>> DeleteAsync<T>(string ruta);
    Task<ResultadoApi<T>> PostMultipartAsync<T>(string ruta, MultipartFormDataContent contenido);
    Task<ResultadoApi<ArchivoDescargado>> DescargarAsync(string ruta);
}

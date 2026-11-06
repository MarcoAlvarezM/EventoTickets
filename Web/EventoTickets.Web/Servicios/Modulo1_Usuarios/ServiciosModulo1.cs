using EventoTickets.Web.Abstracciones.Interfaces;
using EventoTickets.Web.Abstracciones.Modelos;

namespace EventoTickets.Web.Servicios;


public class AutenticacionServicio : IAutenticacionServicio
{
    private readonly IClienteApi _api;
    public AutenticacionServicio(IClienteApi api) => _api = api;

    public Task<ResultadoApi<LoginResponse>> LoginAsync(LoginRequest solicitud) => _api.PostAsync<LoginResponse>("api/autenticacion/login", solicitud);
    public Task<ResultadoApi<LoginResponse>> RegistrarAsync(RegistroCompradorRequest solicitud) => _api.PostAsync<LoginResponse>("api/autenticacion/registro", solicitud);
}

public class UsuarioServicio : IUsuarioServicio
{
    private readonly IClienteApi _api;
    public UsuarioServicio(IClienteApi api) => _api = api;

    public Task<ResultadoApi<List<UsuarioResponse>>> ListarAsync(string? busqueda, string? rol, string? estado) =>
        _api.GetAsync<List<UsuarioResponse>>(Consulta.Crear("api/usuarios", ("busqueda", busqueda), ("rol", rol), ("estado", estado)));
    public Task<ResultadoApi<UsuarioResponse>> ObtenerAsync(int id) => _api.GetAsync<UsuarioResponse>($"api/usuarios/{id}");
    public Task<ResultadoApi<UsuarioResponse>> CrearAsync(UsuarioCrearRequest solicitud) => _api.PostAsync<UsuarioResponse>("api/usuarios", solicitud);
    public Task<ResultadoApi<UsuarioResponse>> ActualizarAsync(int id, UsuarioActualizarRequest solicitud) => _api.PutAsync<UsuarioResponse>($"api/usuarios/{id}", solicitud);
    public Task<ResultadoApi<SinContenido>> CambiarEstadoAsync(int id, string estado) =>
        _api.PatchAsync<SinContenido>($"api/usuarios/{id}/estado", new CambiarEstadoUsuarioRequest { Estado = estado });
    public Task<ResultadoApi<SinContenido>> CambiarPasswordAsync(int id, CambiarPasswordRequest solicitud) =>
        _api.PutAsync<SinContenido>($"api/usuarios/{id}/password", solicitud);
    public Task<ResultadoApi<ResultadoEliminacion>> EliminarAsync(int id) => _api.DeleteAsync<ResultadoEliminacion>($"api/usuarios/{id}");
}

public class CategoriaServicio : ICategoriaServicio
{
    private readonly IClienteApi _api;
    public CategoriaServicio(IClienteApi api) => _api = api;

    public Task<ResultadoApi<List<CategoriaResponse>>> ListarAsync(bool soloActivas) =>
        _api.GetAsync<List<CategoriaResponse>>(Consulta.Crear("api/categorias", ("soloActivas", soloActivas)));
    public Task<ResultadoApi<CategoriaResponse>> ObtenerAsync(int id) => _api.GetAsync<CategoriaResponse>($"api/categorias/{id}");
    public Task<ResultadoApi<CategoriaResponse>> CrearAsync(CategoriaRequest solicitud) => _api.PostAsync<CategoriaResponse>("api/categorias", solicitud);
    public Task<ResultadoApi<CategoriaResponse>> ActualizarAsync(int id, CategoriaRequest solicitud) => _api.PutAsync<CategoriaResponse>($"api/categorias/{id}", solicitud);
    public Task<ResultadoApi<ResultadoEliminacion>> EliminarAsync(int id) => _api.DeleteAsync<ResultadoEliminacion>($"api/categorias/{id}");
}

public class LocacionServicio : ILocacionServicio
{
    private readonly IClienteApi _api;
    public LocacionServicio(IClienteApi api) => _api = api;

    public Task<ResultadoApi<List<LocacionResponse>>> ListarAsync(bool soloActivas, string? busqueda = null) =>
        _api.GetAsync<List<LocacionResponse>>(Consulta.Crear("api/locaciones", ("soloActivas", soloActivas), ("busqueda", busqueda)));
    public Task<ResultadoApi<LocacionResponse>> ObtenerAsync(int id) => _api.GetAsync<LocacionResponse>($"api/locaciones/{id}");
    public Task<ResultadoApi<LocacionResponse>> CrearAsync(LocacionRequest solicitud) => _api.PostAsync<LocacionResponse>("api/locaciones", solicitud);
    public Task<ResultadoApi<LocacionResponse>> ActualizarAsync(int id, LocacionRequest solicitud) => _api.PutAsync<LocacionResponse>($"api/locaciones/{id}", solicitud);
    public Task<ResultadoApi<ResultadoEliminacion>> EliminarAsync(int id) => _api.DeleteAsync<ResultadoEliminacion>($"api/locaciones/{id}");
}

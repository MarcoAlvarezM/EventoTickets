using EventoTickets.Abstracciones.Excepciones;
using EventoTickets.Abstracciones.Interfaces.DA;
using EventoTickets.Abstracciones.Interfaces.Flujo;
using EventoTickets.Abstracciones.Modelos;

namespace EventoTickets.Flujo;


public class CategoriaFlujo : ICategoriaFlujo
{
    private readonly ICategoriaDA _categoriaDA;

    public CategoriaFlujo(ICategoriaDA categoriaDA) => _categoriaDA = categoriaDA;

    public Task<IEnumerable<CategoriaResponse>> ListarAsync(bool soloActivas) => _categoriaDA.ListarAsync(soloActivas);

    public async Task<CategoriaResponse> ObtenerAsync(int categoriaId) =>
        await _categoriaDA.ObtenerAsync(categoriaId) ?? throw new RecursoNoEncontradoException("La categoría no existe.");

    public async Task<CategoriaResponse> CrearAsync(CategoriaRequest solicitud)
    {
        solicitud.Nombre = solicitud.Nombre.Trim();
        var id = await _categoriaDA.CrearAsync(solicitud);
        return await ObtenerAsync(id);
    }

    public async Task<CategoriaResponse> ActualizarAsync(int categoriaId, CategoriaRequest solicitud)
    {
        solicitud.Nombre = solicitud.Nombre.Trim();
        await _categoriaDA.ActualizarAsync(categoriaId, solicitud);
        return await ObtenerAsync(categoriaId);
    }

    public async Task<ResultadoEliminacion> EliminarAsync(int categoriaId)
    {
        var resultado = await _categoriaDA.EliminarAsync(categoriaId);
        return new ResultadoEliminacion
        {
            Resultado = resultado,
            Mensaje = resultado == "Desactivada"
                ? "La categoría tiene eventos asociados: se desactivó en lugar de eliminarse."
                : "Categoría eliminada."
        };
    }
}

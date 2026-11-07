using EventoTickets.Abstracciones.Excepciones;
using EventoTickets.Abstracciones.Interfaces.DA;
using EventoTickets.Abstracciones.Interfaces.Flujo;
using EventoTickets.Abstracciones.Modelos;

namespace EventoTickets.Flujo;


public class EntradaFlujo : IEntradaFlujo
{
    private readonly IEntradaDA _entradaDA;

    public EntradaFlujo(IEntradaDA entradaDA) => _entradaDA = entradaDA;

    public Task<IEnumerable<EntradaResponse>> ListarPorUsuarioAsync(int usuarioId) => _entradaDA.ListarPorUsuarioAsync(usuarioId);

    public Task<IEnumerable<EntradaResponse>> ListarPorEventoAsync(int eventoId, string? estado) =>
        _entradaDA.ListarPorEventoAsync(eventoId, estado);

    public async Task<EntradaResponse> ObtenerAsync(int entradaId, int usuarioId, bool esPersonal)
    {
        var entrada = await _entradaDA.ObtenerAsync(entradaId) ?? throw new RecursoNoEncontradoException("La entrada no existe.");
        if (!esPersonal && entrada.UsuarioId != usuarioId)
            throw new AccesoDenegadoException("La entrada no pertenece al usuario.");
        return entrada;
    }

    public Task<EntradaResponse> TransferirAsync(int entradaId, int usuarioId, TransferenciaEntradaRequest solicitud) =>
        _entradaDA.TransferirAsync(entradaId, usuarioId, solicitud);

    public Task<EntradaResponse> RegistrarIngresoAsync(ValidacionAccesoRequest solicitud, int validadorUsuarioId)
    {
        // El QR codifica "CODIGO|HASH"; se acepta también el código digitado a mano
        var partes = solicitud.CodigoTicket.Trim().Split('|', 2);
        if (partes.Length == 2)
        {
            solicitud.CodigoTicket = partes[0];
            solicitud.HashCriptografico ??= partes[1];
        }
        return _entradaDA.RegistrarIngresoAsync(solicitud, validadorUsuarioId);
    }
}

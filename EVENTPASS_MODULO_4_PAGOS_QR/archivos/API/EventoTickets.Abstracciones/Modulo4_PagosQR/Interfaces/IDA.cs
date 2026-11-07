using System.Data;
using EventoTickets.Abstracciones.Modelos;
using Microsoft.AspNetCore.Http;

namespace EventoTickets.Abstracciones.Interfaces.DA;


public interface IPagoDA
{
    Task<int> RegistrarAsync(PagoRegistroDatos datos);
    Task<IEnumerable<PagoResponse>> ListarAsync(string? estado, string? busqueda);
    Task<IEnumerable<PagoResponse>> ListarPorUsuarioAsync(int usuarioId);
    Task<IEnumerable<PagoResponse>> ListarPorPedidoAsync(int pedidoId);
    Task<PagoResponse?> ObtenerAsync(int pagoId);
    Task<int> AprobarAsync(int pagoId, int auditorUsuarioId, string? codigoAprobacion);
    Task RechazarAsync(int pagoId, int auditorUsuarioId, string motivo, int minutosGracia);
}

public interface IEntradaDA
{
    Task<IEnumerable<EntradaResponse>> ListarPorUsuarioAsync(int usuarioId);
    Task<IEnumerable<EntradaResponse>> ListarPorEventoAsync(int eventoId, string? estado);
    Task<EntradaResponse?> ObtenerAsync(int entradaId);
    Task<EntradaResponse> TransferirAsync(int entradaId, int usuarioId, TransferenciaEntradaRequest datos);
    Task<EntradaResponse> RegistrarIngresoAsync(ValidacionAccesoRequest datos, int validadorUsuarioId);
}

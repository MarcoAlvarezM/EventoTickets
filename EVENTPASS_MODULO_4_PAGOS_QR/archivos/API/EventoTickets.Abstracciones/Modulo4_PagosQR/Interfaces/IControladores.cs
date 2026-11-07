using EventoTickets.Abstracciones.Modelos;
using Microsoft.AspNetCore.Mvc;

namespace EventoTickets.Abstracciones.Interfaces.API;


public interface IPagoController
{
    Task<IActionResult> Registrar(PagoRegistroRequest solicitud);
    Task<IActionResult> Listar(string? estado, string? busqueda);
    Task<IActionResult> ListarMios();
    Task<IActionResult> ListarPorPedido(int pedidoId);
    Task<IActionResult> Obtener(int id);
    Task<IActionResult> DescargarComprobante(int id);
    Task<IActionResult> Aprobar(int id, PagoAprobarRequest solicitud);
    Task<IActionResult> Rechazar(int id, PagoRechazarRequest solicitud);
}

public interface IEntradaController
{
    Task<IActionResult> ListarMias();
    Task<IActionResult> ListarPorEvento(int eventoId, string? estado);
    Task<IActionResult> Obtener(int id);
    Task<IActionResult> ObtenerQr(int id);
    Task<IActionResult> Transferir(int id, TransferenciaEntradaRequest solicitud);
    Task<IActionResult> RegistrarIngreso(ValidacionAccesoRequest solicitud);
}

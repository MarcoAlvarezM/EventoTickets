using EventoTickets.Abstracciones.Modelos;
using Microsoft.AspNetCore.Mvc;

namespace EventoTickets.Abstracciones.Interfaces.API;


public interface IPedidoController
{
    Task<IActionResult> Cotizar(PedidoRequest solicitud);
    Task<IActionResult> Crear(PedidoRequest solicitud);
    Task<IActionResult> ListarMios(string? estado);
    Task<IActionResult> Listar(FiltroPedidos filtro);
    Task<IActionResult> Obtener(int id);
    Task<IActionResult> ActualizarMetodoPago(int id, ActualizarMetodoPagoRequest solicitud);
    Task<IActionResult> Cancelar(int id);
    Task<IActionResult> LiberarVencidos();
}

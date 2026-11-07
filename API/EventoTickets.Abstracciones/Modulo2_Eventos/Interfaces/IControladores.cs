using EventoTickets.Abstracciones.Modelos;
using Microsoft.AspNetCore.Mvc;

namespace EventoTickets.Abstracciones.Interfaces.API;


public interface IEventoController
{
    Task<IActionResult> Listar(FiltroEventos filtro);
    Task<IActionResult> Obtener(int id);
    Task<IActionResult> Crear(EventoRequest solicitud);
    Task<IActionResult> Actualizar(int id, EventoRequest solicitud);
    Task<IActionResult> CambiarEstado(int id, CambiarEstadoEventoRequest solicitud);
    Task<IActionResult> Eliminar(int id);
    Task<IActionResult> ListarTiposEntrada(int id, bool soloActivos);
}

public interface ITipoEntradaController
{
    Task<IActionResult> Obtener(int id);
    Task<IActionResult> Crear(TipoEntradaRequest solicitud);
    Task<IActionResult> Actualizar(int id, TipoEntradaBase solicitud);
    Task<IActionResult> Eliminar(int id);
}

public interface IReporteController
{
    Task<IActionResult> Dashboard();
    Task<IActionResult> VentasPorEvento(FiltroReporteVentas filtro);
}

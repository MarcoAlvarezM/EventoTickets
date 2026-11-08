using EventoTickets.Web.Abstracciones.Constantes;
using EventoTickets.Web.Abstracciones.Interfaces;
using EventoTickets.Web.Abstracciones.Modelos;
using EventoTickets.Web.Infraestructura;
using Microsoft.AspNetCore.Mvc;

namespace EventoTickets.Web.Pages.Admin.Pagos;

public class DetalleModel : PaginaBase
{
    private readonly IPagoServicio _pagos;
    private readonly IPedidoServicio _pedidos;

    public DetalleModel(IPagoServicio pagos, IPedidoServicio pedidos)
    {
        _pagos = pagos;
        _pedidos = pedidos;
    }

    [BindProperty(SupportsGet = true)] public int Id { get; set; }
    [BindProperty] public PagoAprobarRequest Aprobacion { get; set; } = new();
    [BindProperty] public PagoRechazarRequest Rechazo { get; set; } = new();

    public PagoResponse Pago { get; private set; } = new();
    public PedidoCompletoResponse Pedido { get; private set; } = new();
    public bool PuedeValidar => User.IsInRole(Roles.Superadministrador) || User.IsInRole(Roles.AuditorPagos);
    public bool EsImagen => Pago.ComprobanteUrl is { } u && (u.EndsWith(".png") || u.EndsWith(".jpg") || u.EndsWith(".jpeg"));

    public async Task<IActionResult> OnGetAsync() => await CargarAsync() ?? Page();

    public async Task<IActionResult> OnPostAprobarAsync()
    {
        if (!PuedeValidar) return Forbid();
        ModelState.Clear();
        if (!TryValidateModel(Aprobacion, nameof(Aprobacion))) return await CargarAsync() ?? Page();

        var resultado = await _pagos.AprobarAsync(Id, Aprobacion);
        if (resultado.Exito)
        {
            MensajeExito = $"Pago {resultado.Datos?.CodigoPago} aprobado. Se emitieron las entradas del pedido {resultado.Datos?.CodigoPedido}.";
            return RedirectToPage("Index", new { estado = EstadosPago.PendienteValidacion });
        }
        MensajeError = resultado.Mensaje;
        return RedirectToPage(new { id = Id });
    }

    public async Task<IActionResult> OnPostRechazarAsync()
    {
        if (!PuedeValidar) return Forbid();
        ModelState.Clear();
        if (!TryValidateModel(Rechazo, nameof(Rechazo))) return await CargarAsync() ?? Page();

        var resultado = await _pagos.RechazarAsync(Id, Rechazo);
        if (resultado.Exito)
        {
            MensajeExito = $"Pago {resultado.Datos?.CodigoPago} rechazado. El comprador puede reportar un nuevo comprobante.";
            return RedirectToPage("Index", new { estado = EstadosPago.PendienteValidacion });
        }
        if (resultado.Errores.Count > 0)
        {
            AgregarErrores(resultado, nameof(Rechazo));
            return await CargarAsync() ?? Page();
        }
        MensajeError = resultado.Mensaje;
        return RedirectToPage(new { id = Id });
    }

    /// <summary>Muestra el comprobante obtenido del API (requiere token del personal).</summary>
    public async Task<IActionResult> OnGetComprobanteAsync()
    {
        var archivo = await _pagos.DescargarComprobanteAsync(Id);
        if (!archivo.Exito || archivo.Datos is null) return NotFound();
        return File(archivo.Datos.Contenido, archivo.Datos.TipoContenido);
    }

    private async Task<IActionResult?> CargarAsync()
    {
        var pago = await _pagos.ObtenerAsync(Id);
        if (!pago.Exito || pago.Datos is null) return await ResultadoFallidoAsync(pago, "/Admin/Pagos/Index");
        Pago = pago.Datos;
        var pedido = await _pedidos.ObtenerAsync(Pago.PedidoId);
        Pedido = pedido.Datos ?? new();
        return null;
    }
}

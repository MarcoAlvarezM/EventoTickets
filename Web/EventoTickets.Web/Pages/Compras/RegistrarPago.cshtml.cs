using System.ComponentModel.DataAnnotations;
using EventoTickets.Web.Abstracciones.Configuracion;
using EventoTickets.Web.Abstracciones.Constantes;
using EventoTickets.Web.Abstracciones.Interfaces;
using EventoTickets.Web.Abstracciones.Modelos;
using EventoTickets.Web.Infraestructura;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace EventoTickets.Web.Pages.Compras;

public class RegistrarPagoModel : PaginaBase
{
    public static readonly string[] Bancos =
    {
        "Banco Nacional", "Banco de Costa Rica", "BAC Credomatic", "Banco Popular", "Davivienda",
        "Scotiabank", "Promerica", "Banco Lafise", "Mucap", "Coopeservidores", "Otra entidad"
    };

    private readonly IPedidoServicio _pedidos;
    private readonly IPagoServicio _pagos;

    public RegistrarPagoModel(IPedidoServicio pedidos, IPagoServicio pagos, IOptions<ConfiguracionCuentaRecaudadora> cuenta)
    {
        _pedidos = pedidos;
        _pagos = pagos;
        Cuenta = cuenta.Value;
    }

    [BindProperty(SupportsGet = true)] public int PedidoId { get; set; }
    [BindProperty] public PagoRegistroRequest Entrada { get; set; } = new();

    [BindProperty]
    [Range(typeof(bool), "true", "true", ErrorMessage = "Debe declarar que el comprobante es auténtico.")]
    public bool Declaracion { get; set; }

    public PedidoCompletoResponse Pedido { get; private set; } = new();
    public ConfiguracionCuentaRecaudadora Cuenta { get; }

    public async Task<IActionResult> OnGetAsync()
    {
        var carga = await CargarAsync();
        if (carga is not null) return carga;
        Entrada = new PagoRegistroRequest { PedidoId = PedidoId, MontoReportado = Pedido.ImporteTotal };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var carga = await CargarAsync();
        if (carga is not null) return carga;
        // El pedido viene de la ruta, no del formulario
        Entrada.PedidoId = PedidoId;
        ModelState.Remove($"{nameof(Entrada)}.{nameof(Entrada.PedidoId)}");
        if (!ModelState.IsValid) return Page();

        var resultado = await _pagos.RegistrarAsync(Entrada);
        if (!resultado.Exito)
        {
            if (resultado.NoAutenticado) return await ResultadoFallidoAsync(resultado);
            AgregarErrores(resultado, nameof(Entrada));
            return Page();
        }

        MensajeExito = $"Recibimos tu comprobante ({resultado.Datos?.CodigoPago}). Lo validaremos y emitiremos tus entradas.";
        return RedirectToPage("/Compras/Confirmacion", new { pedidoId = PedidoId });
    }

    private async Task<IActionResult?> CargarAsync()
    {
        var resultado = await _pedidos.ObtenerAsync(PedidoId);
        if (!resultado.Exito || resultado.Datos is null) return await ResultadoFallidoAsync(resultado, "/MisPedidos/Index");
        if (resultado.Datos.Estado != EstadosPedido.PendientePago)
        {
            MensajeError = "Este pedido ya no admite reportes de pago.";
            return RedirectToPage("/Compras/Confirmacion", new { pedidoId = PedidoId });
        }
        Pedido = resultado.Datos;
        return null;
    }
}

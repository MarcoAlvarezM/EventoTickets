using EventoTickets.Abstracciones.Configuracion;
using EventoTickets.Abstracciones.Interfaces.Flujo;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EventoTickets.Servicios;

/// <summary>
/// HU-14 · Expiración automática de reservas: cada N segundos (Ventas:SegundosRevisionExpiracion)
/// vence los pedidos PendientePago cuya FechaVencimientoReserva ya pasó y devuelve el cupo al inventario.
/// No depende de que el comprador visite la página. Alternativa en BD: job de SQL Server Agent (database/).
/// </summary>
public class ExpiracionReservasServicio : BackgroundService
{
    private readonly IServiceScopeFactory _scopes;
    private readonly ILogger<ExpiracionReservasServicio> _logger;
    private readonly TimeSpan _intervalo;

    public ExpiracionReservasServicio(IServiceScopeFactory scopes, ILogger<ExpiracionReservasServicio> logger, IOptions<ConfiguracionVentas> ventas)
    {
        _scopes = scopes;
        _logger = logger;
        _intervalo = ventas.Value.SegundosRevisionExpiracion <= 0
            ? TimeSpan.Zero
            : TimeSpan.FromSeconds(Math.Max(10, ventas.Value.SegundosRevisionExpiracion));
    }

    protected override async Task ExecuteAsync(CancellationToken detener)
    {
        if (_intervalo.TotalSeconds <= 0) return;
        using var temporizador = new PeriodicTimer(_intervalo);
        do
        {
            try
            {
                using var scope = _scopes.CreateScope();
                var liberados = await scope.ServiceProvider.GetRequiredService<IPedidoFlujo>().LiberarVencidosAsync();
                if (liberados > 0) _logger.LogInformation("Reservas vencidas liberadas: {Cantidad}", liberados);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogWarning(ex, "No se pudieron liberar las reservas vencidas; se reintentará.");
            }
        } while (await temporizador.WaitForNextTickAsync(detener));
    }
}

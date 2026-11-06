using System.Globalization;
using EventoTickets.Web.Abstracciones.Constantes;

namespace EventoTickets.Web.Infraestructura;

/// <summary>Formato de presentación (moneda, fechas y etiquetas de estado) usado por las vistas.</summary>
public static class Formato
{
    /// <summary>es-CR con punto decimal: coincide con lo que envían los &lt;input type="number"&gt;.</summary>
    public static readonly CultureInfo Cultura = CrearCultura();

    private static CultureInfo CrearCultura()
    {
        var cultura = (CultureInfo)CultureInfo.GetCultureInfo("es-CR").Clone();
        cultura.NumberFormat.NumberDecimalSeparator = ".";
        cultura.NumberFormat.NumberGroupSeparator = ",";
        cultura.NumberFormat.CurrencyDecimalSeparator = ".";
        cultura.NumberFormat.CurrencyGroupSeparator = ",";
        cultura.NumberFormat.CurrencySymbol = "₡";
        return cultura;
    }

    public static string Moneda(decimal? monto) => monto.HasValue ? $"₡{monto.Value.ToString("N0", Cultura)}" : "—";
    public static string MonedaExacta(decimal monto) => $"₡{monto.ToString("N2", Cultura)}";
    public static string Fecha(DateTime fecha) => fecha.ToString("dd MMM yyyy", Cultura);
    public static string FechaLarga(DateTime fecha) => Cultura.TextInfo.ToTitleCase(fecha.ToString("dddd d 'de' MMMM, yyyy", Cultura));
    public static string FechaHora(DateTime fecha) => fecha.ToString("dd/MM/yyyy HH:mm", Cultura);
    public static string FechaHoraOpcional(DateTime? fecha) => fecha.HasValue ? FechaHora(fecha.Value) : "—";
    public static string Hora(TimeSpan hora) => DateTime.Today.Add(hora).ToString("hh:mm tt", CultureInfo.InvariantCulture);

    public static string EtiquetaEstado(string? estado) => estado switch
    {
        EstadosPedido.PendientePago => "Pendiente de pago",
        EstadosPedido.EnValidacion => "En validación",
        EstadosPedido.Pagado => "Pagado y emitido",
        EstadosPedido.Vencido => "Reserva vencida",
        EstadosPago.PendienteValidacion => "Pendiente de validación",
        EstadosEntrada.Valida => "Válida",
        EstadosEvento.Agotado => "Agotado",
        EstadosUsuario.Inactivo => "Inactivo (eliminado)",
        EstadosPago.Aprobado => "Aprobado y conciliado",
        null or "" => "—",
        _ => estado
    };

    /// <summary>Clases Tailwind del badge según el estado (paleta Stitch).</summary>
    public static string ClaseEstado(string? estado) => estado switch
    {
        "Activo" or "Valida" or "Publicado" or "Pagado" or "Aprobado" => "bg-secondary/10 text-secondary",
        "PendientePago" or "PendienteValidacion" or "EnValidacion" or "Borrador" or "Agotado" => "bg-amber-100 text-amber-800",
        "Utilizada" or "Finalizado" => "bg-surface-container-high text-on-surface-variant",
        "Bloqueado" or "Inactivo" or "Cancelado" or "Rechazado" or "Vencido" or "Anulada" => "bg-error-container text-on-error-container",
        _ => "bg-surface-container text-on-surface-variant"
    };

    public static string EtiquetaRol(string rol) => rol switch
    {
        Roles.Superadministrador => "Superadministrador",
        Roles.AdministradorEventos => "Administrador de Eventos",
        Roles.AuditorPagos => "Auditor de Pagos",
        Roles.ValidadorAcceso => "Validador de Acceso",
        Roles.Comprador => "Comprador",
        _ => rol
    };

    public static string EtiquetaDocumento(string tipo) => tipo switch
    {
        TiposDocumento.Cedula => "Cédula",
        TiposDocumento.Dimex => "DIMEX",
        TiposDocumento.Pasaporte => "Pasaporte",
        _ => tipo
    };

    public static string Iniciales(string nombre) =>
        string.Concat(nombre.Split(' ', StringSplitOptions.RemoveEmptyEntries).Take(2).Select(p => char.ToUpperInvariant(p[0])));
}

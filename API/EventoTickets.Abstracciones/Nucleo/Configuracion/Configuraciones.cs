namespace EventoTickets.Abstracciones.Configuracion;


/// <summary>Sección "Ventas" de appsettings.json: parámetros de negocio del proceso de compra.</summary>
public sealed class ConfiguracionVentas
{
    public const string Seccion = "Ventas";
    public decimal PorcentajeCargoServicio { get; set; } = 10m;
    public decimal PorcentajeIVA { get; set; } = 13m;
    public int MinutosReserva { get; set; } = 30;
    public int MinutosGraciaRechazo { get; set; } = 60;
    public int MaxEntradasPorPedido { get; set; } = 10;
    /// <summary>Cada cuántos segundos el servicio en segundo plano vence reservas (HU-14). 0 = desactivado.</summary>
    public int SegundosRevisionExpiracion { get; set; } = 60;
    public string Moneda { get; set; } = "CRC";
}

/// <summary>Sección "Jwt" de appsettings.json.</summary>
public sealed class ConfiguracionJwt
{
    public const string Seccion = "Jwt";
    public string Emisor { get; set; } = string.Empty;
    public string Audiencia { get; set; } = string.Empty;
    public string Clave { get; set; } = string.Empty;
    public int MinutosExpiracion { get; set; } = 120;
}

/// <summary>Sección "Almacenamiento" de appsettings.json (comprobantes de pago).</summary>
public sealed class ConfiguracionAlmacenamiento
{
    public const string Seccion = "Almacenamiento";
    public string RutaComprobantes { get; set; } = "Almacenamiento/Comprobantes";
    public int TamanoMaximoMB { get; set; } = 10;
    public string[] ExtensionesPermitidas { get; set; } = { ".jpg", ".jpeg", ".png", ".pdf" };
}

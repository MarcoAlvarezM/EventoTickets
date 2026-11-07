namespace EventoTickets.Web.Abstracciones.Configuracion;


/// <summary>Sección "Api" de appsettings.json.</summary>
public sealed class ConfiguracionApi
{
    public const string Seccion = "Api";
    public string UrlBase { get; set; } = "http://localhost:5238/";
    public int TimeoutSegundos { get; set; } = 30;
}

/// <summary>Sección "CuentaRecaudadora": datos que se muestran al comprador para pagar (valores de demostración).</summary>
public sealed class ConfiguracionCuentaRecaudadora
{
    public const string Seccion = "CuentaRecaudadora";
    public string Banco { get; set; } = string.Empty;
    public string Titular { get; set; } = string.Empty;
    public string CedulaJuridica { get; set; } = string.Empty;
    public string CuentaIban { get; set; } = string.Empty;
    public string SinpeMovil { get; set; } = string.Empty;
}

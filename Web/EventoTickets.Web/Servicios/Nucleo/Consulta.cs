using EventoTickets.Web.Abstracciones.Interfaces;
using EventoTickets.Web.Abstracciones.Modelos;

namespace EventoTickets.Web.Servicios;


/// <summary>Arma la query string ignorando valores vacíos.</summary>
internal static class Consulta
{
    public static string Crear(string ruta, params (string Nombre, object? Valor)[] parametros)
    {
        var partes = parametros
            .Where(p => p.Valor is not null && !(p.Valor is string s && string.IsNullOrWhiteSpace(s)))
            .Select(p => $"{p.Nombre}={Uri.EscapeDataString(p.Valor switch
            {
                DateTime f => f.ToString("yyyy-MM-dd"),
                bool b => b ? "true" : "false",
                _ => Convert.ToString(p.Valor, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty
            })}");
        var qs = string.Join("&", partes);
        return qs.Length == 0 ? ruta : $"{ruta}?{qs}";
    }
}

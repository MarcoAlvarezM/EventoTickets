using System.Data;
using EventoTickets.Abstracciones.Interfaces.DA;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace EventoTickets.DA.Repositorios;


/// <summary>Crea conexiones SQL Server usando ConnectionStrings:EventCR_BoleteriaPro_DB del appsettings.</summary>
public class RepositorioDapper : IRepositorioDapper
{
    private readonly string _cadenaConexion;

    public RepositorioDapper(IConfiguration configuracion)
    {
        _cadenaConexion = configuracion.GetConnectionString("EventCR_BoleteriaPro_DB")
            ?? throw new InvalidOperationException("Falta la cadena de conexión 'EventCR_BoleteriaPro_DB' en appsettings.json.");
    }

    public IDbConnection ObtenerConexion() => new SqlConnection(_cadenaConexion);
}

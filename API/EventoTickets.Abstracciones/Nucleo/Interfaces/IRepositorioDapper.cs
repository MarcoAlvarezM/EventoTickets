using System.Data;
using EventoTickets.Abstracciones.Modelos;
using Microsoft.AspNetCore.Http;

namespace EventoTickets.Abstracciones.Interfaces.DA;


/// <summary>Fábrica de conexiones a la base de datos (cadena tomada de appsettings).</summary>
public interface IRepositorioDapper
{
    IDbConnection ObtenerConexion();
}

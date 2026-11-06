using System.ComponentModel.DataAnnotations;

namespace EventoTickets.Abstracciones.Modelos;


/// <summary>Resultado de una eliminación: física ("Eliminada") o lógica ("Desactivada").</summary>
public class ResultadoEliminacion
{
    public string Resultado { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
}

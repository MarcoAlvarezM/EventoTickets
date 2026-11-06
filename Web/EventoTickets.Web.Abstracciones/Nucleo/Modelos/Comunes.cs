using System.ComponentModel.DataAnnotations;

namespace EventoTickets.Web.Abstracciones.Modelos;


/// <summary>Resultado de una eliminación: física ("Eliminada") o lógica ("Desactivada").</summary>
public class ResultadoEliminacion
{
    public string Resultado { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
}

/// <summary>CONTRATO COMPARTIDO: archivo binario descargado del API (comprobantes, QR).</summary>
public class ArchivoDescargado
{
    public byte[] Contenido { get; set; } = Array.Empty<byte>();
    public string TipoContenido { get; set; } = "application/octet-stream";
    public string NombreArchivo { get; set; } = "comprobante";
}

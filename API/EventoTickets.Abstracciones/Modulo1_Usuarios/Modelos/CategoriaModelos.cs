using System.ComponentModel.DataAnnotations;

namespace EventoTickets.Abstracciones.Modelos;


public class CategoriaBase
{
    [Required(ErrorMessage = "El nombre es requerido.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre {2} y {1} caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(255, ErrorMessage = "La descripción admite máximo {1} caracteres.")]
    public string? Descripcion { get; set; }

    public bool Activo { get; set; } = true;
}

public class CategoriaRequest : CategoriaBase { }

public class CategoriaResponse : CategoriaBase
{
    public int CategoriaId { get; set; }
    public int CantidadEventos { get; set; }
}

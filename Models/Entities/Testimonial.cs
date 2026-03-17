namespace OptiControl.Models.Entities;

/// <summary>Testimonio para la sección "Lo que dicen nuestros clientes" de la web. Incluye valoración por estrellas (1-5).</summary>
public class Testimonial
{
    public int Id { get; set; }
    public string Quote { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public string? Location { get; set; }
    /// <summary>Valoración en estrellas, 1 a 5.</summary>
    public int Rating { get; set; } = 5;
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    /// <summary>False cuando lo envía un cliente desde el formulario público, hasta que el admin apruebe.</summary>
    public bool IsApproved { get; set; } = true;
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

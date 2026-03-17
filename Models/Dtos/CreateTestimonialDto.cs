using System.ComponentModel.DataAnnotations;

namespace OptiControl.Models.Dtos;

/// <summary>DTO para que un cliente envíe un testimonio desde el formulario público. Se guarda como pendiente de aprobación.</summary>
public class CreateTestimonialDto
{
    [Required, MaxLength(200)]
    public string AuthorName { get; set; } = string.Empty;

    [MaxLength(150)]
    public string? Location { get; set; }

    [Required, MaxLength(1000)]
    public string Quote { get; set; } = string.Empty;

    /// <summary>Valoración 1 a 5 estrellas. Se ajusta automáticamente si está fuera de rango.</summary>
    [Range(1, 5)]
    public int Rating { get; set; } = 5;
}

namespace OptiControl.Models.Dtos;

/// <summary>Datos de la agencia (nombre, contacto, moneda).</summary>
public class AgencyDataDto
{
    public int Id { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string Currency { get; set; } = "NIO";
    public DateTime? UpdatedAt { get; set; }
}

namespace OptiControl.Models.Dtos;

/// <summary>Factura vencida para alertas del dashboard.</summary>
public class OverdueInvoiceAlertDto
{
    public string Id { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
    public string? Concept { get; set; }
}

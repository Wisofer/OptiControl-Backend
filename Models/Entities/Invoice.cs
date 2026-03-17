using System.Text.Json.Serialization;

namespace OptiControl.Models.Entities;

/// <summary>Factura con código legible INV-001, INV-002...</summary>
public class Invoice
{
    public string Id { get; set; } = string.Empty; // INV-001
    public int ClientId { get; set; }
    public DateTime Date { get; set; }
    public DateTime? DueDate { get; set; }
    /// <summary>Fecha de viaje (opcional).</summary>
    public DateTime? TravelDate { get; set; }
    /// <summary>Fecha de retorno (opcional).</summary>
    public DateTime? ReturnDate { get; set; }
    public decimal Amount { get; set; }
    /// <summary>"Pagado" | "Pendiente" | "Vencida"</summary>
    public string Status { get; set; } = "Pendiente";
    public string? Concept { get; set; }
    /// <summary>"Cordobas" | "Dolares" | "Transferencia" - forma de pago del cliente</summary>
    public string? PaymentMethod { get; set; }

    [JsonIgnore]
    public virtual Client Client { get; set; } = null!;
}

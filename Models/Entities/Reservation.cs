using System.Text.Json.Serialization;

namespace OptiControl.Models.Entities;

public class Reservation
{
    public int Id { get; set; }
    public int ClientId { get; set; }
    public string Destination { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal Amount { get; set; }
    /// <summary>"Pagado" | "Pendiente" | "Parcial"</summary>
    public string PaymentStatus { get; set; } = "Pendiente";
    /// <summary>"Cordobas" | "Dolares" | "Transferencia" - forma de pago del cliente</summary>
    public string? PaymentMethod { get; set; }

    [JsonIgnore]
    public virtual Client Client { get; set; } = null!;
}

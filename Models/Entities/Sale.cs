using System.Text.Json.Serialization;

namespace OptiControl.Models.Entities;

public class Sale
{
    public int Id { get; set; }
    public int ClientId { get; set; }
    public DateTime Date { get; set; }
    /// <summary>Legacy OptiControl: descripción del producto.</summary>
    public string? Product { get; set; }
    /// <summary>Legacy OptiControl: monto.</summary>
    public decimal? Amount { get; set; }
    /// <summary>"Pagada" | "pendiente" | "cotizacion" | "Cancelada" | "Completado" | "Pendiente" (legacy)</summary>
    public string Status { get; set; } = "Pagada";
    public string? PaymentMethod { get; set; }
    /// <summary>OptiControl: nombre del cliente para mostrar/imprimir.</summary>
    public string? ClientName { get; set; }
    /// <summary>OptiControl: total de la venta.</summary>
    public decimal Total { get; set; }
    /// <summary>OptiControl: monto pagado hasta la fecha.</summary>
    public decimal AmountPaid { get; set; }
    /// <summary>OptiControl: NIO, USD.</summary>
    public string? Currency { get; set; }

    [JsonIgnore]
    public virtual Client Client { get; set; } = null!;
    [JsonIgnore]
    public virtual ICollection<SaleItem> SaleItems { get; set; } = new List<SaleItem>();
}

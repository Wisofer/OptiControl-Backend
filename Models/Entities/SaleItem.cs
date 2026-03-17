namespace OptiControl.Models.Entities;

/// <summary>Línea de una venta: producto o servicio.</summary>
public class SaleItem
{
    public int Id { get; set; }
    public int SaleId { get; set; }
    /// <summary>"product" | "service"</summary>
    public string Type { get; set; } = "product";
    public int? ProductId { get; set; }
    public int? ServiceId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Subtotal { get; set; }

    public virtual Sale Sale { get; set; } = null!;
}

namespace OptiControl.Models.Entities;

/// <summary>Movimiento de pago aplicado a una venta (pago inicial o abono).</summary>
public class SalePayment
{
    public int Id { get; set; }
    public int SaleId { get; set; }
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
    public string? PaymentType { get; set; }
    public string? Bank { get; set; }

    public virtual Sale Sale { get; set; } = null!;
}

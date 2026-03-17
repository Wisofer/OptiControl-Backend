namespace OptiControl.Models.Entities;

/// <summary>Caja diaria: apertura, ventas, egresos, cierre por fecha.</summary>
public class CajaDiaria
{
    public int Id { get; set; }
    public DateTime Date { get; set; } // Único por día
    public decimal Opening { get; set; }
    public decimal Sales { get; set; }
    public decimal Expenses { get; set; }
    public decimal Closing { get; set; }
}

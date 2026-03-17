namespace OptiControl.Models.Entities;

/// <summary>Egreso. Categoría: Operativo | Fijo | Marketing.</summary>
public class Expense
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public string Concept { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Category { get; set; } = "Operativo"; // Operativo | Fijo | Marketing
}

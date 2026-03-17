namespace OptiControl.Models.Entities;

/// <summary>Evento para feed del dashboard y notificaciones.</summary>
public class Activity
{
    public int Id { get; set; }
    /// <summary>"reservation" | "invoice" | "payment" | "client" | "expense" | "template" | "user"</summary>
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Time { get; set; }
    public string? EntityId { get; set; }
    public int? ClientId { get; set; }
}

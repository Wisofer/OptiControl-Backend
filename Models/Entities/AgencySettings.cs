namespace OptiControl.Models.Entities;

/// <summary>Configuración global de la agencia (un registro).</summary>
public class AgencySettings
{
    public int Id { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string Currency { get; set; } = "NIO";
    public string Language { get; set; } = "es";
    public decimal ExchangeRate { get; set; } = 36.8m;
    public string Theme { get; set; } = "light";
    public int SoundVolume { get; set; } = 80;
    public bool AlertsReservacionesPendientes { get; set; } = true;
    public bool AlertsFacturasVencidas { get; set; } = true;
    public bool AlertsRecordatorios { get; set; } = true;
    public DateTime? UpdatedAt { get; set; }
}

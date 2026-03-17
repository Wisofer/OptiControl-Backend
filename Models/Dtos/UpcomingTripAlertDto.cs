namespace OptiControl.Models.Dtos;

/// <summary>Reservación con fecha de inicio próxima (para alertas del dashboard).</summary>
public class UpcomingTripAlertDto
{
    public int Id { get; set; }
    public string Destination { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public string? ClientName { get; set; }
    /// <summary>True si la fecha de inicio está dentro de los próximos 3 días.</summary>
    public bool InNext3Days { get; set; }
}

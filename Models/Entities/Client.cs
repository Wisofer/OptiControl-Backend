namespace OptiControl.Models.Entities;

/// <summary>Cliente (OptiControl / OptiControl). Eje de reservaciones, ventas y facturas.</summary>
public class Client
{
    public int Id { get; set; }
    public string? Pasaporte { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    /// <summary>"Pendiente" | "Viajó" (OptiControl) / OptiControl: dirección</summary>
    public string? Address { get; set; }
    /// <summary>OptiControl: graduación ojo derecho.</summary>
    public string? GraduacionOd { get; set; }
    /// <summary>OptiControl: graduación ojo izquierdo.</summary>
    public string? GraduacionOi { get; set; }
    /// <summary>OptiControl: fecha de registro.</summary>
    public DateTime? FechaRegistro { get; set; }
    /// <summary>OptiControl: notas/descripción del cliente.</summary>
    public string? Descripcion { get; set; }
    /// <summary>"Pendiente" | "Viajó"</summary>
    public string Status { get; set; } = "Pendiente";
    /// <summary>Fecha del último viaje (null si nunca ha viajado).</summary>
    public DateTime? LastTrip { get; set; }

    public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    public virtual ICollection<Sale> Sales { get; set; } = new List<Sale>();
    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
}

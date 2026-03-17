using OptiControl.Models.Entities;

namespace OptiControl.Models.Dtos;

public class ClientHistoryDto
{
    public Client Client { get; set; } = null!;
    public PagedResult<ReservationSummaryDto> Reservations { get; set; } = null!;
    public PagedResult<SaleSummaryDto> Sales { get; set; } = null!;
    public PagedResult<InvoiceSummaryDto> Invoices { get; set; } = null!;
    public List<Activity> Activity { get; set; } = new();
}

public class ReservationSummaryDto
{
    public int Id { get; set; }
    public string Destination { get; set; } = "";
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal Amount { get; set; }
    public string PaymentStatus { get; set; } = "";
    public string? PaymentMethod { get; set; }
}

public class SaleSummaryDto
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public string Product { get; set; } = "";
    public decimal Amount { get; set; }
    public string Status { get; set; } = "";
    public string? PaymentMethod { get; set; }
}

public class InvoiceSummaryDto
{
    public string Id { get; set; } = "";
    public DateTime Date { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? TravelDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = "";
    public string? Concept { get; set; }
    public string? PaymentMethod { get; set; }
}

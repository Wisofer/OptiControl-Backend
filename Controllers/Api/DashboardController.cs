using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OptiControl.Data;
using OptiControl.Models.Dtos;
using OptiControl.Services.IServices;
using OptiControl.Utils;
using Microsoft.EntityFrameworkCore;

namespace OptiControl.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "Administrador")]
public class DashboardController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IActivityService _activity;
    private readonly ISettingsService _settings;
    private readonly IInvoiceService _invoiceService;
    private readonly IDashboardOpticsService _dashboardOptics;

    public DashboardController(ApplicationDbContext context, IActivityService activity, ISettingsService settings, IInvoiceService invoiceService, IDashboardOpticsService dashboardOptics)
    {
        _context = context;
        _activity = activity;
        _settings = settings;
        _invoiceService = invoiceService;
        _dashboardOptics = dashboardOptics;
    }

    [HttpGet("summary")]
    public IActionResult GetSummary()
    {
        return Ok(_dashboardOptics.GetSummary());
    }

    [HttpGet("recent-activity")]
    public IActionResult GetRecentActivity([FromQuery] int limit = 50)
    {
        return Ok(_dashboardOptics.GetRecentActivity(limit));
    }

    [HttpGet("monthly-income")]
    public IActionResult GetMonthlyIncome([FromQuery] int months = 12)
    {
        return Ok(_dashboardOptics.GetMonthlyIncome(months));
    }

    [HttpGet("top-products")]
    public IActionResult GetTopProducts()
    {
        return Ok(_dashboardOptics.GetTopProducts());
    }

    [HttpGet("reservations-status")]
    public IActionResult GetReservationsStatus()
    {
        var total = _context.Reservations.Count();
        var paid = _context.Reservations.Count(r => r.PaymentStatus == SD.PaymentStatusPagado);
        var pending = _context.Reservations.Count(r => r.PaymentStatus == SD.PaymentStatusPendiente);
        var partial = _context.Reservations.Count(r => r.PaymentStatus == SD.PaymentStatusParcial);
        return Ok(new { total, paid, pending, partial });
    }

    /// <summary>Alertas para el usuario (facturas vencidas y viajes próximos). Solo devuelve cada bloque si su opción está activa en Configuración.</summary>
    [HttpGet("alerts")]
    public IActionResult GetAlerts()
    {
        var settings = _settings.Get();
        var today = DateTime.UtcNow.Date;

        // Facturas vencidas (solo si AlertsFacturasVencidas está activa)
        var overdueInvoices = Array.Empty<OverdueInvoiceAlertDto>();
        var message = (string?)null;
        if (settings?.AlertsFacturasVencidas == true)
        {
            var overdue = _invoiceService.GetOverdueForAlerts();
            overdueInvoices = overdue.ToArray();
            message = overdue.Count > 0
                ? (overdue.Count == 1
                    ? $"La factura {overdue[0].Id} venció. Revisa."
                    : $"Tienes {overdue.Count} factura(s) vencida(s). Revisa.")
                : null;
        }

        // Viajes próximos en 7 días (y cuáles en 3 días), solo si AlertsRecordatorios está activa
        var upcomingTrips = Array.Empty<UpcomingTripAlertDto>();
        var upcomingTripsMessage = (string?)null;
        if (settings?.AlertsRecordatorios == true)
        {
            var in7Days = today.AddDays(7);
            var in3Days = today.AddDays(3);
            var list = _context.Reservations
                .Include(r => r.Client)
                .Where(r => r.StartDate.Date >= today && r.StartDate.Date <= in7Days)
                .OrderBy(r => r.StartDate)
                .Select(r => new UpcomingTripAlertDto
                {
                    Id = r.Id,
                    Destination = r.Destination,
                    StartDate = r.StartDate,
                    ClientName = r.Client != null ? r.Client.Name : null,
                    InNext3Days = r.StartDate.Date <= in3Days
                })
                .ToList();
            upcomingTrips = list.ToArray();

            if (list.Count > 0)
            {
                var in3 = list.Count(x => x.InNext3Days);
                var parts = list.Take(5).Select(t => $"{t.Destination} ({t.StartDate:dd MMM})").ToList();
                var destList = string.Join(", ", parts);
                if (list.Count > 5) destList += ", ...";
                if (list.Count == 1)
                    upcomingTripsMessage = $"Tu viaje a {list[0].Destination} empieza el {list[0].StartDate:dd MMM}.";
                else if (in3 > 0 && in3 < list.Count)
                    upcomingTripsMessage = $"Tienes {list.Count} viajes en los próximos 7 días ({in3} en los próximos 3 días): {destList}. Revisa reservaciones.";
                else if (in3 == list.Count && list.Count > 1)
                    upcomingTripsMessage = $"Tienes {list.Count} viajes en los próximos 3 días: {destList}. Revisa reservaciones.";
                else
                    upcomingTripsMessage = $"Tienes {list.Count} viajes en los próximos 7 días: {destList}. Revisa reservaciones.";
            }
        }

        return Ok(new
        {
            overdueInvoices,
            message,
            upcomingTrips,
            upcomingTripsMessage
        });
    }
}

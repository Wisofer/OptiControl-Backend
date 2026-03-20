using OptiControl.Data;
using OptiControl.Models.Dtos;
using OptiControl.Models.Entities;
using OptiControl.Services.IServices;
using OptiControl.Utils;
using Microsoft.EntityFrameworkCore;

namespace OptiControl.Services;

public class InvoiceService : IInvoiceService
{
    private readonly ApplicationDbContext _context;
    private readonly IActivityService _activity;

    public InvoiceService(ApplicationDbContext context, IActivityService activity)
    {
        _context = context;
        _activity = activity;
    }

    /// <summary>Marca como Vencida las facturas Pendientes cuya fecha de vencimiento ya pasó.</summary>
    private void MarkOverdueInvoices()
    {
        var today = TimeZoneHelper.NicaraguaToday();
        var overdue = _context.Invoices
            .Where(i => i.Status == SD.InvoiceStatusPendiente && i.DueDate.HasValue && i.DueDate.Value.Date < today)
            .ToList();
        foreach (var i in overdue) i.Status = SD.InvoiceStatusVencida;
        if (overdue.Count > 0) _context.SaveChanges();
    }

    public PagedResult<Invoice> GetPaged(int? clientId = null, string? status = null, string? paymentMethod = null, DateTime? dateFrom = null, DateTime? dateTo = null, int page = 1, int pageSize = 20)
    {
        MarkOverdueInvoices();
        pageSize = Math.Clamp(pageSize, 1, 100);
        page = Math.Max(1, page);
        var q = _context.Invoices.Include(i => i.Client).AsQueryable();
        if (clientId.HasValue) q = q.Where(i => i.ClientId == clientId.Value);
        if (!string.IsNullOrWhiteSpace(status)) q = q.Where(i => i.Status == status);
        if (!string.IsNullOrWhiteSpace(paymentMethod)) q = q.Where(i => i.PaymentMethod == paymentMethod);
        if (dateFrom.HasValue) q = q.Where(i => i.Date >= dateFrom.Value);
        if (dateTo.HasValue) q = q.Where(i => i.Date <= dateTo.Value);
        var totalCount = q.Count();
        var items = q.OrderByDescending(i => i.Date).Skip((page - 1) * pageSize).Take(pageSize).ToList();
        return PagedResult<Invoice>.Create(items, totalCount, page, pageSize);
    }
    public Invoice? GetById(string id)
    {
        var invoice = _context.Invoices.Include(i => i.Client).FirstOrDefault(i => i.Id == id);
        if (invoice != null && invoice.Status == SD.InvoiceStatusPendiente && invoice.DueDate.HasValue && invoice.DueDate.Value.Date < TimeZoneHelper.NicaraguaToday())
        {
            invoice.Status = SD.InvoiceStatusVencida;
            _context.SaveChanges();
        }
        return invoice;
    }

    public List<OverdueInvoiceAlertDto> GetOverdueForAlerts()
    {
        MarkOverdueInvoices();
        return _context.Invoices
            .Where(i => i.Status == SD.InvoiceStatusVencida)
            .OrderBy(i => i.DueDate)
            .Select(i => new OverdueInvoiceAlertDto { Id = i.Id, DueDate = i.DueDate, Concept = i.Concept })
            .ToList();
    }

    public string GetNextInvoiceCode()
    {
        var last = _context.Invoices
            .Where(i => i.Id.StartsWith("INV-"))
            .Select(i => i.Id)
            .ToList()
            .Select(id => int.TryParse(id.Replace("INV-", ""), out var n) ? n : 0)
            .DefaultIfEmpty(0)
            .Max();
        return $"INV-{(last + 1):D3}";
    }

    /// <summary>Guarda la fecha como mediodía UTC para que el día no cambie al leer en cualquier zona horaria.</summary>
    private static DateTime ToUtcNoon(DateTime d) => new DateTime(d.Year, d.Month, d.Day, 12, 0, 0, DateTimeKind.Utc);
    private static DateTime? ToUtcNoonNullable(DateTime? d) => d.HasValue ? ToUtcNoon(d.Value.Date) : null;

    public Invoice Create(Invoice invoice)
    {
        if (string.IsNullOrEmpty(invoice.Id))
            invoice.Id = GetNextInvoiceCode();
        invoice.Status = invoice.Status ?? SD.InvoiceStatusPendiente;
        var dateOnly = invoice.Date == default ? TimeZoneHelper.NicaraguaToday() : invoice.Date.Date;
        invoice.Date = ToUtcNoon(dateOnly);
        invoice.DueDate = ToUtcNoonNullable(invoice.DueDate);
        invoice.TravelDate = ToUtcNoonNullable(invoice.TravelDate);
        invoice.ReturnDate = ToUtcNoonNullable(invoice.ReturnDate);
        _context.Invoices.Add(invoice);
        _context.SaveChanges();
        var client = _context.Clients.Find(invoice.ClientId);
        _activity.Record(SD.ActivityTypeInvoice, $"Factura {invoice.Id} - {client?.Name}" + (string.IsNullOrEmpty(invoice.Concept) ? "" : $" ({invoice.Concept})"), invoice.Id, invoice.ClientId);
        return invoice;
    }

    public bool Update(Invoice invoice)
    {
        var existing = _context.Invoices.Find(invoice.Id);
        if (existing == null) return false;
        existing.ClientId = invoice.ClientId;
        existing.Date = ToUtcNoon(invoice.Date.Date);
        existing.DueDate = ToUtcNoonNullable(invoice.DueDate);
        existing.TravelDate = ToUtcNoonNullable(invoice.TravelDate);
        existing.ReturnDate = ToUtcNoonNullable(invoice.ReturnDate);
        existing.Amount = invoice.Amount;
        existing.Status = invoice.Status;
        existing.Concept = invoice.Concept;
        existing.PaymentMethod = invoice.PaymentMethod;
        _context.SaveChanges();
        return true;
    }

    public bool Delete(string id)
    {
        var i = _context.Invoices.Find(id);
        if (i == null) return false;
        _context.Invoices.Remove(i);
        _context.SaveChanges();
        return true;
    }
}

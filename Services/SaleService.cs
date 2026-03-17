using OptiControl.Data;
using OptiControl.Models.Dtos;
using OptiControl.Models.Entities;
using OptiControl.Services.IServices;
using OptiControl.Utils;
using Microsoft.EntityFrameworkCore;

namespace OptiControl.Services;

public class SaleService : ISaleService
{
    private readonly ApplicationDbContext _context;
    private readonly IActivityService _activity;
    private readonly ISettingsService _settings;

    public SaleService(ApplicationDbContext context, IActivityService activity, ISettingsService settings)
    {
        _context = context;
        _activity = activity;
        _settings = settings;
    }

    public (PagedResult<Sale> Paged, decimal TotalAmountInCordobas, decimal TotalPendingInCordobas) GetPaged(int? clientId = null, string? status = null, string? paymentMethod = null, DateTime? dateFrom = null, DateTime? dateTo = null, int page = 1, int pageSize = 20)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);
        page = Math.Max(1, page);
        var q = _context.Sales.Include(s => s.Client).AsQueryable();
        if (clientId.HasValue) q = q.Where(s => s.ClientId == clientId.Value);
        if (!string.IsNullOrWhiteSpace(status)) q = q.Where(s => s.Status == status);
        if (!string.IsNullOrWhiteSpace(paymentMethod)) q = q.Where(s => s.PaymentMethod == paymentMethod);
        if (dateFrom.HasValue) q = q.Where(s => s.Date >= dateFrom.Value);
        if (dateTo.HasValue) q = q.Where(s => s.Date <= dateTo.Value);
        var totalCount = q.Count();
        var allForTotals = q.Select(s => new { s.Amount, s.PaymentMethod, s.Status }).ToList();
        var rate = _settings.Get()?.ExchangeRate ?? 36.8m;
        var totalAmountInCordobas = allForTotals.Sum(s => CurrencyHelper.ToCordobas(s.Amount ?? 0, s.PaymentMethod, rate));
        var totalPendingInCordobas = allForTotals.Where(s => s.Status == SD.SaleStatusPendiente).Sum(s => CurrencyHelper.ToCordobas(s.Amount ?? 0, s.PaymentMethod, rate));
        var items = q.OrderByDescending(s => s.Date).Skip((page - 1) * pageSize).Take(pageSize).ToList();
        var paged = PagedResult<Sale>.Create(items, totalCount, page, pageSize);
        return (paged, totalAmountInCordobas, totalPendingInCordobas);
    }
    public Sale? GetById(int id) => _context.Sales.Include(s => s.Client).FirstOrDefault(s => s.Id == id);

    public Sale Create(Sale sale)
    {
        sale.Status = sale.Status ?? SD.SaleStatusCompletado;
        sale.Date = sale.Date == default ? DateTime.UtcNow.Date : sale.Date;
        _context.Sales.Add(sale);
        _context.SaveChanges();
        var client = _context.Clients.Find(sale.ClientId);
        _activity.Record(SD.ActivityTypePayment, $"Venta: {client?.Name} - {sale.Product}", sale.Id.ToString(), sale.ClientId);
        return sale;
    }

    public bool Update(Sale sale)
    {
        var existing = _context.Sales.Find(sale.Id);
        if (existing == null) return false;
        existing.ClientId = sale.ClientId;
        existing.Date = sale.Date;
        existing.Product = sale.Product;
        existing.Amount = sale.Amount;
        existing.Status = sale.Status;
        existing.PaymentMethod = sale.PaymentMethod;
        _context.SaveChanges();
        return true;
    }

    public bool Delete(int id)
    {
        var s = _context.Sales.Find(id);
        if (s == null) return false;
        _context.Sales.Remove(s);
        _context.SaveChanges();
        return true;
    }
}

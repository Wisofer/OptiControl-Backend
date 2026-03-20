using System.Globalization;
using OptiControl.Data;
using OptiControl.Models.Dtos;
using OptiControl.Models.Entities;
using OptiControl.Services.IServices;
using OptiControl.Utils;
using Microsoft.EntityFrameworkCore;

namespace OptiControl.Services;

public class DashboardOpticsService : IDashboardOpticsService
{
    private readonly ApplicationDbContext _context;
    private readonly ISettingsService _settings;
    private static readonly CultureInfo EsNi = new("es-NI");

    public DashboardOpticsService(ApplicationDbContext context, ISettingsService settings)
    {
        _context = context;
        _settings = settings;
    }

    private static decimal RealRevenueAmount(Sale s)
    {
        if (s.Status == SD.SaleStatusCotizacion || s.Status == SD.SaleStatusCancelada) return 0;
        if (s.Status == SD.SaleStatusPagada) return s.Total;
        return s.AmountPaid;
    }

    public DashboardSummaryDto GetSummary()
    {
        var today = TimeZoneHelper.NicaraguaToday();
        var startOfMonth = new DateTime(today.Year, today.Month, 1);
        var rate = _settings.Get()?.ExchangeRate ?? 36.8m;

        var sales = _context.Sales.Where(s => s.Total > 0).ToList();
        var totalRevenue = sales.Sum(s => CurrencyHelper.SaleAmountToCordobas(RealRevenueAmount(s), s.Currency, rate));
        var salesToday = sales.Where(s => s.Date.Date == today).Sum(s => CurrencyHelper.SaleAmountToCordobas(RealRevenueAmount(s), s.Currency, rate));
        var salesMonth = sales.Where(s => s.Date >= startOfMonth && s.Date < startOfMonth.AddMonths(1)).Sum(s => CurrencyHelper.SaleAmountToCordobas(RealRevenueAmount(s), s.Currency, rate));

        var productsCount = _context.Products.Sum(p => p.Stock);
        var clientsCount = _context.Clients.Count();
        var productsTotal = _context.Products.Count();

        return new DashboardSummaryDto
        {
            TotalRevenue = totalRevenue,
            SalesToday = salesToday,
            SalesMonth = salesMonth,
            ProductsCount = productsCount,
            ClientsCount = clientsCount,
            ProductsTotal = productsTotal
        };
    }

    public List<ActivityItemDto> GetRecentActivity(int limit = 50)
    {
        var list = _context.Activities
            .OrderByDescending(a => a.Time)
            .Take(limit)
            .ToList();
        return list.Select((a, i) => new ActivityItemDto
        {
            Id = "A" + (a.Id),
            Type = a.Type,
            Description = a.Description,
            Time = a.Time.ToString("yyyy-MM-dd HH:mm", EsNi)
        }).ToList();
    }

    public List<MonthlyIncomeDto> GetMonthlyIncome(int months = 12)
    {
        var end = TimeZoneHelper.NicaraguaToday();
        var start = end.AddMonths(-months);
        var rate = _settings.Get()?.ExchangeRate ?? 36.8m;
        var sales = _context.Sales
            .Where(s => s.Date >= start && s.Date <= end && s.Total > 0 && s.Status != SD.SaleStatusCotizacion && s.Status != SD.SaleStatusCancelada)
            .ToList();
        var byMonth = sales
            .GroupBy(s => new { s.Date.Year, s.Date.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Amount = g.Sum(s => CurrencyHelper.SaleAmountToCordobas(RealRevenueAmount(s), s.Currency, rate)) })
            .ToList();
        var result = new List<MonthlyIncomeDto>();
        for (var d = start; d <= end; d = d.AddMonths(1))
        {
            var m = byMonth.FirstOrDefault(x => x.Year == d.Year && x.Month == d.Month);
            var monthName = new DateTime(d.Year, d.Month, 1).ToString("MMM", EsNi);
            result.Add(new MonthlyIncomeDto
            {
                Month = monthName,
                MonthName = monthName,
                Amount = m?.Amount ?? 0
            });
        }
        return result;
    }

    public List<TopProductDto> GetTopProducts()
    {
        var realSaleIds = _context.Sales
            .Where(s => s.Status == SD.SaleStatusPagada || s.Status == SD.SaleStatusPendiente)
            .Select(s => s.Id)
            .ToList();
        var items = _context.SaleItems
            .Where(i => i.SaleId != 0 && realSaleIds.Contains(i.SaleId))
            .ToList();
        var byName = items
            .GroupBy(i => i.Type == "service" ? i.ServiceName : i.ProductName)
            .Where(g => !string.IsNullOrWhiteSpace(g.Key))
            .Select(g => new TopProductDto { Name = g.Key!, Quantity = g.Sum(x => x.Quantity) })
            .OrderByDescending(x => x.Quantity)
            .Take(20)
            .ToList();
        return byName;
    }
}

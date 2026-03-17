using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OptiControl.Data;
using OptiControl.Services.IServices;
using OptiControl.Utils;

namespace OptiControl.Controllers.Api;

/// <summary>Reportes esenciales para agencia de viajes: resumen financiero, ventas, facturas, reservaciones, egresos. Todos con exportación Excel y PDF.</summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "Administrador")]
public class ReportsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IExportService _export;
    private readonly ISettingsService _settings;

    private readonly IDashboardOpticsService _dashboardOptics;
    private readonly IOpticsSaleService _opticsSaleService;

    public ReportsController(ApplicationDbContext context, IExportService export, ISettingsService settings, IDashboardOpticsService dashboardOptics, IOpticsSaleService opticsSaleService)
    {
        _context = context;
        _export = export;
        _settings = settings;
        _dashboardOptics = dashboardOptics;
        _opticsSaleService = opticsSaleService;
    }

    private static (DateTime from, DateTime to) NormalizeRange(DateTime? dateFrom, DateTime? dateTo)
    {
        var to = dateTo ?? DateTime.UtcNow.Date;
        var from = dateFrom ?? to.AddYears(-1);
        return (from, to);
    }

    // ----- OptiControl: reportes de ingresos y ventas -----
    [HttpGet("ingresos-totales")]
    public IActionResult IngresosTotales()
    {
        var summary = _dashboardOptics.GetSummary();
        return Ok(new { totalIncome = summary.TotalRevenue, salesToday = summary.SalesToday, salesMonth = summary.SalesMonth });
    }

    [HttpGet("ventas-dia")]
    public IActionResult VentasDia([FromQuery] string? dateFrom, [FromQuery] string? dateTo, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        DateTime? from = DateTime.TryParse(dateFrom, out var df) ? df : null;
        DateTime? to = DateTime.TryParse(dateTo, out var dt) ? dt : null;
        if (!from.HasValue || !to.HasValue) { from = DateTime.UtcNow.Date.AddMonths(-1); to = DateTime.UtcNow.Date; }
        var allSales = _opticsSaleService.GetSalesHistoryPaged(1, 10000);
        var filtered = allSales.Items.Where(s =>
        {
            if (!DateTime.TryParse(s.Date, out var d)) return false;
            return d.Date >= from!.Value.Date && d.Date <= to!.Value.Date;
        }).ToList();
        var rate = _settings.Get()?.ExchangeRate ?? 36.8m;
        var totalAmount = filtered.Sum(s => CurrencyHelper.SaleAmountToCordobas(s.Status == "Pagada" ? s.Total : s.AmountPaid, s.Currency, rate));
        var totalCount = filtered.Count;
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        var pageList = filtered.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        return Ok(new { items = pageList, totalCount, totalPages, page, pageSize, totalAmount });
    }

    [HttpGet("ventas-mes")]
    public IActionResult VentasMes([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var rate = _settings.Get()?.ExchangeRate ?? 36.8m;
        var paged = _opticsSaleService.GetSalesHistoryPaged(page, pageSize);
        var totalAmount = paged.Items.Sum(s => CurrencyHelper.SaleAmountToCordobas(s.Status == "Pagada" ? s.Total : s.AmountPaid, s.Currency, rate));
        return Ok(new { items = paged.Items, totalCount = paged.TotalCount, totalPages = paged.TotalPages, page = paged.Page, pageSize = paged.PageSize, totalAmount });
    }

    [HttpGet("productos-mas-vendidos")]
    public IActionResult ProductosMasVendidos() => Ok(_dashboardOptics.GetTopProducts());

    [HttpGet("sales")]
    public IActionResult Sales([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? dateFrom = null, [FromQuery] string? dateTo = null)
    {
        var rate = _settings.Get()?.ExchangeRate ?? 36.8m;
        var paged = _opticsSaleService.GetSalesHistoryPaged(page, pageSize);
        var totalAmountInCordobas = paged.Items.Sum(s => CurrencyHelper.SaleAmountToCordobas(s.Status == "Pagada" ? s.Total : s.AmountPaid, s.Currency, rate));
        return Ok(new { items = paged.Items, totalCount = paged.TotalCount, totalPages = paged.TotalPages, page = paged.Page, pageSize = paged.PageSize, totalAmountInCordobas });
    }

    // ----- 1. Resumen financiero (ingresos, egresos, balance) -----
    [HttpGet("income-vs-expenses")]
    public IActionResult IncomeVsExpenses([FromQuery] DateTime? dateFrom, [FromQuery] DateTime? dateTo)
    {
        var (from, to) = NormalizeRange(dateFrom, dateTo);
        var rate = _settings.Get()?.ExchangeRate ?? 36.8m;
        var reservationsList = _context.Reservations
            .Where(r => r.StartDate >= from && r.StartDate <= to && r.PaymentStatus == SD.PaymentStatusPagado)
            .Select(r => new { r.Amount, r.PaymentMethod })
            .ToList();
        var incomeFromReservations = reservationsList.Sum(r => CurrencyHelper.ToCordobas(r.Amount, r.PaymentMethod, rate));
        var invoicesList = _context.Invoices
            .Where(i => i.Date >= from && i.Date <= to && i.Status == SD.InvoiceStatusPagado)
            .Select(i => new { i.Amount, i.PaymentMethod })
            .ToList();
        var incomeFromInvoices = invoicesList.Sum(i => CurrencyHelper.ToCordobas(i.Amount, i.PaymentMethod, rate));
        var totalIncome = incomeFromReservations + incomeFromInvoices;
        var totalExpenses = _context.Expenses
            .Where(e => e.Date >= from && e.Date <= to)
            .Sum(e => e.Amount);
        return Ok(new
        {
            dateFrom = from,
            dateTo = to,
            totalIncome,
            incomeFromReservations,
            incomeFromInvoices,
            totalExpenses,
            balance = totalIncome - totalExpenses
        });
    }

    [HttpGet("income-vs-expenses/export/excel")]
    public IActionResult IncomeVsExpensesExportExcel([FromQuery] DateTime? dateFrom, [FromQuery] DateTime? dateTo)
    {
        var bytes = _export.GetFinancialSummaryExcel(dateFrom, dateTo);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Resumen-financiero.xlsx");
    }

    [HttpGet("income-vs-expenses/export/pdf")]
    public IActionResult IncomeVsExpensesExportPdf([FromQuery] DateTime? dateFrom, [FromQuery] DateTime? dateTo)
    {
        var bytes = _export.GetFinancialSummaryPdf(dateFrom, dateTo);
        return File(bytes, "application/pdf", "Resumen-financiero.pdf");
    }

    // ----- 2. Listado de facturas con total y estado (paginado) -----
    [HttpGet("invoices")]
    public IActionResult Invoices([FromQuery] DateTime? dateFrom, [FromQuery] DateTime? dateTo, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var (from, to) = NormalizeRange(dateFrom, dateTo);
        pageSize = Math.Clamp(pageSize, 1, 100);
        page = Math.Max(1, page);
        var q = _context.Invoices
            .Include(i => i.Client)
            .Where(i => i.Date >= from && i.Date <= to)
            .OrderByDescending(i => i.Date);
        var totalCount = q.Count();
        var list = q
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(i => new
            {
                i.Id,
                i.ClientId,
                clientName = i.Client != null ? i.Client.Name : null,
                i.Date,
                i.DueDate,
                i.TravelDate,
                i.ReturnDate,
                i.Amount,
                i.Status,
                i.Concept,
                i.PaymentMethod
            })
            .ToList();
        var rate = _settings.Get()?.ExchangeRate ?? 36.8m;
        var allForTotal = _context.Invoices.Where(i => i.Date >= from && i.Date <= to).Select(i => new { i.Amount, i.PaymentMethod }).ToList();
        var totalInvoiced = allForTotal.Sum(i => CurrencyHelper.ToCordobas(i.Amount, i.PaymentMethod, rate));
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        return Ok(new { dateFrom = from, dateTo = to, items = list, totalCount, page, pageSize, totalPages, totalInvoiced });
    }

    [HttpGet("invoices/export/excel")]
    public IActionResult InvoicesExportExcel([FromQuery] DateTime? dateFrom, [FromQuery] DateTime? dateTo)
    {
        var bytes = _export.GetInvoicesExcel(null, null, null, dateFrom, dateTo);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Reporte-facturas.xlsx");
    }

    [HttpGet("invoices/export/pdf")]
    public IActionResult InvoicesExportPdf([FromQuery] DateTime? dateFrom, [FromQuery] DateTime? dateTo)
    {
        var bytes = _export.GetInvoicesPdf(null, null, null, dateFrom, dateTo);
        return File(bytes, "application/pdf", "Reporte-facturas.pdf");
    }

    // ----- 4. Listado de reservaciones (paginado) -----
    [HttpGet("reservations")]
    public IActionResult Reservations([FromQuery] DateTime? dateFrom, [FromQuery] DateTime? dateTo, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var (from, to) = NormalizeRange(dateFrom, dateTo);
        pageSize = Math.Clamp(pageSize, 1, 100);
        page = Math.Max(1, page);
        var q = _context.Reservations
            .Include(r => r.Client)
            .Where(r => r.StartDate >= from && r.EndDate <= to)
            .OrderByDescending(r => r.StartDate);
        var totalCount = q.Count();
        var list = q
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new
            {
                r.Id,
                r.ClientId,
                clientName = r.Client != null ? r.Client.Name : null,
                r.Destination,
                r.StartDate,
                r.EndDate,
                r.Amount,
                r.PaymentStatus,
                r.PaymentMethod
            })
            .ToList();
        var rate = _settings.Get()?.ExchangeRate ?? 36.8m;
        var allForTotal = _context.Reservations.Where(r => r.StartDate >= from && r.EndDate <= to).Select(r => new { r.Amount, r.PaymentMethod }).ToList();
        var totalAmountInCordobas = allForTotal.Sum(r => CurrencyHelper.ToCordobas(r.Amount, r.PaymentMethod, rate));
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        return Ok(new { dateFrom = from, dateTo = to, items = list, totalCount, page, pageSize, totalPages, totalAmountInCordobas });
    }

    [HttpGet("reservations/export/excel")]
    public IActionResult ReservationsExportExcel([FromQuery] DateTime? dateFrom, [FromQuery] DateTime? dateTo)
    {
        var bytes = _export.GetReservationsExcel(null, null, null, dateFrom, dateTo);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Reporte-reservaciones.xlsx");
    }

    [HttpGet("reservations/export/pdf")]
    public IActionResult ReservationsExportPdf([FromQuery] DateTime? dateFrom, [FromQuery] DateTime? dateTo)
    {
        var bytes = _export.GetReservationsPdf(null, null, null, dateFrom, dateTo);
        return File(bytes, "application/pdf", "Reporte-reservaciones.pdf");
    }

    // ----- 5. Listado de egresos (paginado) -----
    [HttpGet("expenses")]
    public IActionResult Expenses([FromQuery] DateTime? dateFrom, [FromQuery] DateTime? dateTo, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var (from, to) = NormalizeRange(dateFrom, dateTo);
        pageSize = Math.Clamp(pageSize, 1, 100);
        page = Math.Max(1, page);
        var q = _context.Expenses
            .Where(e => e.Date >= from && e.Date <= to)
            .OrderByDescending(e => e.Date);
        var totalCount = q.Count();
        var list = q.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        var totalAmount = _context.Expenses.Where(e => e.Date >= from && e.Date <= to).Sum(e => e.Amount);
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        return Ok(new { dateFrom = from, dateTo = to, items = list, totalCount, page, pageSize, totalPages, totalAmount });
    }

    [HttpGet("expenses/export/excel")]
    public IActionResult ExpensesExportExcel([FromQuery] DateTime? dateFrom, [FromQuery] DateTime? dateTo)
    {
        var bytes = _export.GetExpensesExcel(dateFrom, dateTo);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Reporte-egresos.xlsx");
    }

    [HttpGet("expenses/export/pdf")]
    public IActionResult ExpensesExportPdf([FromQuery] DateTime? dateFrom, [FromQuery] DateTime? dateTo)
    {
        var bytes = _export.GetExpensesPdf(dateFrom, dateTo);
        return File(bytes, "application/pdf", "Reporte-egresos.pdf");
    }
}

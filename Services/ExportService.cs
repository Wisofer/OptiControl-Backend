using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using OptiControl.Data;
using OptiControl.Models.Entities;
using OptiControl.Services.IServices;
using OptiControl.Utils;

namespace OptiControl.Services;

public class ExportService : IExportService
{
    private readonly ApplicationDbContext _context;
    private readonly ISettingsService _settings;

    public ExportService(ApplicationDbContext context, ISettingsService settings)
    {
        _context = context;
        _settings = settings;
    }

    public byte[] GetClientsExcel(string? search = null)
    {
        var list = GetClientsList(search);
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Clientes");
        ws.Cell(1, 1).Value = "Pasaporte"; ws.Cell(1, 2).Value = "Nombre"; ws.Cell(1, 3).Value = "Correo"; ws.Cell(1, 4).Value = "Teléfono";
        var headerRow = ws.Range(1, 1, 1, 4);
        headerRow.Style.Font.Bold = true; headerRow.Style.Fill.BackgroundColor = XLColor.LightGray;
        var row = 2;
        foreach (var c in list)
        {
            ws.Cell(row, 1).Value = c.Pasaporte ?? ""; ws.Cell(row, 2).Value = c.Name; ws.Cell(row, 3).Value = c.Email; ws.Cell(row, 4).Value = c.Phone ?? "";
            row++;
        }
        ws.Columns().AdjustToContents();
        using var stream = new MemoryStream();
        wb.SaveAs(stream);
        return stream.ToArray();
    }

    public byte[] GetClientsPdf(string? search = null)
    {
        var list = GetClientsList(search);
        var companyName = _settings.GetCompanyName();
        return BuildTablePdf(companyName, "Listado de clientes", new[] { "Pasaporte", "Nombre", "Correo", "Teléfono" },
            list.Select(c => new[] { c.Pasaporte ?? "-", c.Name, c.Email, c.Phone ?? "-" }).ToList());
    }

    public byte[] GetProductsExcel(string? search = null)
    {
        var list = GetProductsList(search);
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Inventario");
        ws.Cell(1, 1).Value = "Producto"; ws.Cell(1, 2).Value = "Tipo"; ws.Cell(1, 3).Value = "Marca"; ws.Cell(1, 4).Value = "Proveedor";
        ws.Cell(1, 5).Value = "P. compra"; ws.Cell(1, 6).Value = "Precio"; ws.Cell(1, 7).Value = "Stock"; ws.Cell(1, 8).Value = "Stock mínimo";
        var headerRow = ws.Range(1, 1, 1, 8);
        headerRow.Style.Font.Bold = true; headerRow.Style.Fill.BackgroundColor = XLColor.LightGray;
        var row = 2;
        foreach (var p in list)
        {
            ws.Cell(row, 1).Value = p.NombreProducto;
            ws.Cell(row, 2).Value = p.TipoProducto;
            ws.Cell(row, 3).Value = p.Marca ?? "-";
            ws.Cell(row, 4).Value = p.Proveedor ?? "-";
            ws.Cell(row, 5).Value = p.PrecioCompra;
            ws.Cell(row, 6).Value = p.Precio;
            ws.Cell(row, 7).Value = p.Stock;
            ws.Cell(row, 8).Value = p.StockMinimo;
            row++;
        }
        ws.Columns().AdjustToContents();
        using var stream = new MemoryStream();
        wb.SaveAs(stream);
        return stream.ToArray();
    }

    public byte[] GetProductsPdf(string? search = null)
    {
        var list = GetProductsList(search);
        var companyName = _settings.GetCompanyName();
        var rows = list.Select(p => new[]
        {
            p.NombreProducto,
            p.TipoProducto,
            p.Marca ?? "-",
            p.Proveedor ?? "-",
            p.PrecioCompra.ToString("N2"),
            p.Precio.ToString("N2"),
            p.Stock.ToString(),
            p.StockMinimo.ToString()
        }).ToList();
        return BuildTablePdf(companyName, "Listado de inventario", new[] { "Producto", "Tipo", "Marca", "Proveedor", "P. compra", "Precio", "Stock", "Stock mínimo" }, rows, true);
    }

    public byte[] GetOpticsServicesExcel(string? search = null)
    {
        var list = GetOpticsServicesList(search);
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Servicios");
        ws.Cell(1, 1).Value = "Servicio"; ws.Cell(1, 2).Value = "Precio"; ws.Cell(1, 3).Value = "Descripción";
        var headerRow = ws.Range(1, 1, 1, 3);
        headerRow.Style.Font.Bold = true; headerRow.Style.Fill.BackgroundColor = XLColor.LightGray;
        var row = 2;
        foreach (var s in list)
        {
            ws.Cell(row, 1).Value = s.NombreServicio;
            ws.Cell(row, 2).Value = s.Precio;
            ws.Cell(row, 3).Value = s.Descripcion ?? "-";
            row++;
        }
        ws.Columns().AdjustToContents();
        using var stream = new MemoryStream();
        wb.SaveAs(stream);
        return stream.ToArray();
    }

    public byte[] GetOpticsServicesPdf(string? search = null)
    {
        var list = GetOpticsServicesList(search);
        var companyName = _settings.GetCompanyName();
        var rows = list.Select(s => new[] { s.NombreServicio, s.Precio.ToString("N2"), s.Descripcion ?? "-" }).ToList();
        return BuildTablePdf(companyName, "Listado de servicios", new[] { "Servicio", "Precio", "Descripción" }, rows);
    }

    public byte[] GetSalesHistoryExcel(DateTime? dateFrom = null, DateTime? dateTo = null, string? status = null, string? paymentMethod = null)
    {
        var list = GetSalesHistoryList(dateFrom, dateTo, status, paymentMethod);
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Historial ventas");
        ws.Cell(1, 1).Value = "Fecha"; ws.Cell(1, 2).Value = "Cliente"; ws.Cell(1, 3).Value = "Ítems"; ws.Cell(1, 4).Value = "Total"; ws.Cell(1, 5).Value = "Pagado";
        ws.Cell(1, 6).Value = "Pendiente"; ws.Cell(1, 7).Value = "Estado"; ws.Cell(1, 8).Value = "Pago"; ws.Cell(1, 9).Value = "Moneda";
        var headerRow = ws.Range(1, 1, 1, 9);
        headerRow.Style.Font.Bold = true; headerRow.Style.Fill.BackgroundColor = XLColor.LightGray;
        var row = 2;
        foreach (var s in list)
        {
            var pending = Math.Max(0, s.Total - s.AmountPaid);
            ws.Cell(row, 1).Value = s.Date.ToString("dd/MM/yyyy HH:mm");
            ws.Cell(row, 2).Value = s.ClientName ?? "-";
            ws.Cell(row, 3).Value = s.SaleItems?.Count ?? 0;
            ws.Cell(row, 4).Value = s.Total;
            ws.Cell(row, 5).Value = s.AmountPaid;
            ws.Cell(row, 6).Value = pending;
            ws.Cell(row, 7).Value = s.Status;
            ws.Cell(row, 8).Value = s.PaymentMethod ?? "-";
            ws.Cell(row, 9).Value = s.Currency ?? "NIO";
            row++;
        }
        ws.Columns().AdjustToContents();
        using var stream = new MemoryStream();
        wb.SaveAs(stream);
        return stream.ToArray();
    }

    public byte[] GetSalesHistoryPdf(DateTime? dateFrom = null, DateTime? dateTo = null, string? status = null, string? paymentMethod = null)
    {
        var list = GetSalesHistoryList(dateFrom, dateTo, status, paymentMethod);
        var companyName = _settings.GetCompanyName();
        var rows = list.Select(s =>
        {
            var pending = Math.Max(0, s.Total - s.AmountPaid);
            return new[]
            {
                s.Date.ToString("dd/MM/yyyy HH:mm"),
                s.ClientName ?? "-",
                (s.SaleItems?.Count ?? 0).ToString(),
                s.Total.ToString("N2"),
                s.AmountPaid.ToString("N2"),
                pending.ToString("N2"),
                s.Status,
                s.PaymentMethod ?? "-",
                s.Currency ?? "NIO"
            };
        }).ToList();
        return BuildTablePdf(companyName, "Historial de ventas", new[] { "Fecha", "Cliente", "Ítems", "Total", "Pagado", "Pendiente", "Estado", "Pago", "Moneda" }, rows, true);
    }

    public byte[] GetReservationsExcel(int? clientId, string? paymentStatus, string? paymentMethod, DateTime? dateFrom, DateTime? dateTo)
    {
        var list = GetReservationsList(clientId, paymentStatus, paymentMethod, dateFrom, dateTo);
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Reservaciones");
        ws.Cell(1, 1).Value = "Cliente"; ws.Cell(1, 2).Value = "Destino"; ws.Cell(1, 3).Value = "Inicio"; ws.Cell(1, 4).Value = "Fin"; ws.Cell(1, 5).Value = "Monto"; ws.Cell(1, 6).Value = "Estado pago"; ws.Cell(1, 7).Value = "Forma de pago";
        var headerRow = ws.Range(1, 1, 1, 7);
        headerRow.Style.Font.Bold = true; headerRow.Style.Fill.BackgroundColor = XLColor.LightGray;
        var row = 2;
        foreach (var r in list)
        {
            ws.Cell(row, 1).Value = r.Client?.Name ?? ""; ws.Cell(row, 2).Value = r.Destination;
            ws.Cell(row, 3).Value = r.StartDate.ToString("dd/MM/yyyy"); ws.Cell(row, 4).Value = r.EndDate.ToString("dd/MM/yyyy");
            ws.Cell(row, 5).Value = r.Amount; ws.Cell(row, 6).Value = r.PaymentStatus; ws.Cell(row, 7).Value = r.PaymentMethod ?? "-";
            row++;
        }
        ws.Columns().AdjustToContents();
        using var stream = new MemoryStream();
        wb.SaveAs(stream);
        return stream.ToArray();
    }

    public byte[] GetReservationsPdf(int? clientId, string? paymentStatus, string? paymentMethod, DateTime? dateFrom, DateTime? dateTo)
    {
        var list = GetReservationsList(clientId, paymentStatus, paymentMethod, dateFrom, dateTo);
        var companyName = _settings.GetCompanyName();
        var rows = list.Select(r => new[] { r.Client?.Name ?? "-", r.Destination, r.StartDate.ToString("dd/MM/yyyy"), r.EndDate.ToString("dd/MM/yyyy"), r.Amount.ToString("N2"), r.PaymentStatus, r.PaymentMethod ?? "-" }).ToList();
        return BuildTablePdf(companyName, "Listado de reservaciones", new[] { "Cliente", "Destino", "Inicio", "Fin", "Monto", "Estado pago", "Forma de pago" }, rows);
    }

    public byte[] GetInvoicesExcel(int? clientId, string? status, string? paymentMethod, DateTime? dateFrom, DateTime? dateTo)
    {
        var list = GetInvoicesList(clientId, status, paymentMethod, dateFrom, dateTo);
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Facturas");
        ws.Cell(1, 1).Value = "Nº Factura"; ws.Cell(1, 2).Value = "Cliente"; ws.Cell(1, 3).Value = "Fecha"; ws.Cell(1, 4).Value = "Vencimiento"; ws.Cell(1, 5).Value = "Fecha viaje"; ws.Cell(1, 6).Value = "Fecha retorno"; ws.Cell(1, 7).Value = "Monto"; ws.Cell(1, 8).Value = "Estado"; ws.Cell(1, 9).Value = "Concepto"; ws.Cell(1, 10).Value = "Forma de pago";
        var headerRow = ws.Range(1, 1, 1, 10);
        headerRow.Style.Font.Bold = true; headerRow.Style.Fill.BackgroundColor = XLColor.LightGray;
        var row = 2;
        foreach (var i in list)
        {
            ws.Cell(row, 1).Value = i.Id; ws.Cell(row, 2).Value = i.Client?.Name ?? "";
            ws.Cell(row, 3).Value = i.Date.ToString("dd/MM/yyyy"); ws.Cell(row, 4).Value = i.DueDate?.ToString("dd/MM/yyyy") ?? "-";
            ws.Cell(row, 5).Value = i.TravelDate?.ToString("dd/MM/yyyy") ?? "-"; ws.Cell(row, 6).Value = i.ReturnDate?.ToString("dd/MM/yyyy") ?? "-";
            ws.Cell(row, 7).Value = i.Amount; ws.Cell(row, 8).Value = i.Status; ws.Cell(row, 9).Value = i.Concept ?? ""; ws.Cell(row, 10).Value = i.PaymentMethod ?? "-";
            row++;
        }
        ws.Columns().AdjustToContents();
        using var stream = new MemoryStream();
        wb.SaveAs(stream);
        return stream.ToArray();
    }

    public byte[] GetInvoicesPdf(int? clientId, string? status, string? paymentMethod, DateTime? dateFrom, DateTime? dateTo)
    {
        var list = GetInvoicesList(clientId, status, paymentMethod, dateFrom, dateTo);
        var companyName = _settings.GetCompanyName();
        var currency = _settings.Get()?.Currency ?? "NIO";
        var rows = list.Select(i => new[] { i.Id, i.Client?.Name ?? "-", i.Date.ToString("dd/MM/yyyy"), i.DueDate?.ToString("dd/MM/yyyy") ?? "-", i.TravelDate?.ToString("dd/MM/yyyy") ?? "-", i.ReturnDate?.ToString("dd/MM/yyyy") ?? "-", $"{i.Amount:N2} {currency}", i.Status, i.Concept ?? "-", i.PaymentMethod ?? "-" }).ToList();
        return BuildTablePdf(companyName, "Listado de facturas", new[] { "Nº Factura", "Cliente", "Fecha", "Vencimiento", "Fecha viaje", "Fecha retorno", "Monto", "Estado", "Concepto", "Forma de pago" }, rows, true);
    }

    public byte[] GetExpensesExcel(DateTime? dateFrom = null, DateTime? dateTo = null)
    {
        var list = GetExpensesList(dateFrom, dateTo);
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Egresos");
        ws.Cell(1, 1).Value = "Fecha"; ws.Cell(1, 2).Value = "Concepto"; ws.Cell(1, 3).Value = "Monto"; ws.Cell(1, 4).Value = "Categoría";
        var headerRow = ws.Range(1, 1, 1, 4);
        headerRow.Style.Font.Bold = true; headerRow.Style.Fill.BackgroundColor = XLColor.LightGray;
        var row = 2;
        foreach (var e in list)
        {
            ws.Cell(row, 1).Value = e.Date.ToString("dd/MM/yyyy"); ws.Cell(row, 2).Value = e.Concept; ws.Cell(row, 3).Value = e.Amount; ws.Cell(row, 4).Value = e.Category;
            row++;
        }
        if (list.Count > 0)
        {
            ws.Cell(row, 2).Value = "TOTAL"; ws.Cell(row, 2).Style.Font.Bold = true;
            ws.Cell(row, 3).FormulaA1 = $"=SUM(C2:C{row - 1})"; ws.Cell(row, 3).Style.Font.Bold = true;
        }
        ws.Columns().AdjustToContents();
        using var stream = new MemoryStream();
        wb.SaveAs(stream);
        return stream.ToArray();
    }

    public byte[] GetExpensesPdf(DateTime? dateFrom = null, DateTime? dateTo = null)
    {
        var list = GetExpensesList(dateFrom, dateTo);
        var companyName = _settings.GetCompanyName();
        var currency = _settings.Get()?.Currency ?? "NIO";
        var total = list.Sum(e => e.Amount);
        var rows = list.Select(e => new[] { e.Date.ToString("dd/MM/yyyy"), e.Concept, $"{e.Amount:N2} {currency}", e.Category }).ToList();
        rows.Add(new[] { "TOTAL", "", $"{total:N2} {currency}", "" });
        return BuildTablePdf(companyName, "Listado de egresos", new[] { "Fecha", "Concepto", "Monto", "Categoría" }, rows);
    }

    public byte[] GetFinancialSummaryExcel(DateTime? dateFrom = null, DateTime? dateTo = null)
    {
        var (from, to) = NormalizeDateRange(dateFrom, dateTo);
        var rate = _settings.Get()?.ExchangeRate ?? 36.8m;
        var reservationsList = _context.Reservations.Where(r => r.StartDate >= from && r.StartDate <= to && r.PaymentStatus == SD.PaymentStatusPagado).Select(r => new { r.Amount, r.PaymentMethod }).ToList();
        var incomeFromReservations = reservationsList.Sum(r => CurrencyHelper.ToCordobas(r.Amount, r.PaymentMethod, rate));
        var invoicesList = _context.Invoices.Where(i => i.Date >= from && i.Date <= to && i.Status == SD.InvoiceStatusPagado).Select(i => new { i.Amount, i.PaymentMethod }).ToList();
        var incomeFromInvoices = invoicesList.Sum(i => CurrencyHelper.ToCordobas(i.Amount, i.PaymentMethod, rate));
        var totalIncome = incomeFromReservations + incomeFromInvoices;
        var totalExpenses = _context.Expenses.Where(e => e.Date >= from && e.Date <= to).Sum(e => e.Amount);
        var balance = totalIncome - totalExpenses;
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Resumen financiero");
        ws.Cell(1, 1).Value = "Período"; ws.Cell(1, 2).Value = $"{from:dd/MM/yyyy} - {to:dd/MM/yyyy}";
        ws.Cell(2, 1).Value = "Ingresos por reservaciones (pagadas)"; ws.Cell(2, 2).Value = incomeFromReservations;
        ws.Cell(3, 1).Value = "Ingresos por facturas (pagadas)"; ws.Cell(3, 2).Value = incomeFromInvoices;
        ws.Cell(4, 1).Value = "Total ingresos"; ws.Cell(4, 2).Value = totalIncome; ws.Cell(4, 2).Style.Font.Bold = true;
        ws.Cell(5, 1).Value = "Total egresos"; ws.Cell(5, 2).Value = totalExpenses;
        ws.Cell(6, 1).Value = "Balance"; ws.Cell(6, 2).Value = balance; ws.Cell(6, 2).Style.Font.Bold = true;
        ws.Columns().AdjustToContents();
        using var stream = new MemoryStream();
        wb.SaveAs(stream);
        return stream.ToArray();
    }

    public byte[] GetFinancialSummaryPdf(DateTime? dateFrom = null, DateTime? dateTo = null)
    {
        var (from, to) = NormalizeDateRange(dateFrom, dateTo);
        var rate = _settings.Get()?.ExchangeRate ?? 36.8m;
        var reservationsList = _context.Reservations.Where(r => r.StartDate >= from && r.StartDate <= to && r.PaymentStatus == SD.PaymentStatusPagado).Select(r => new { r.Amount, r.PaymentMethod }).ToList();
        var incomeFromReservations = reservationsList.Sum(r => CurrencyHelper.ToCordobas(r.Amount, r.PaymentMethod, rate));
        var invoicesList = _context.Invoices.Where(i => i.Date >= from && i.Date <= to && i.Status == SD.InvoiceStatusPagado).Select(i => new { i.Amount, i.PaymentMethod }).ToList();
        var incomeFromInvoices = invoicesList.Sum(i => CurrencyHelper.ToCordobas(i.Amount, i.PaymentMethod, rate));
        var totalIncome = incomeFromReservations + incomeFromInvoices;
        var totalExpenses = _context.Expenses.Where(e => e.Date >= from && e.Date <= to).Sum(e => e.Amount);
        var balance = totalIncome - totalExpenses;
        var companyName = _settings.GetCompanyName();
        var currency = _settings.Get()?.Currency ?? "NIO";
        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.Header().Column(column =>
                {
                    column.Item().Text(companyName).Bold().FontSize(16).FontColor(Colors.Blue.Darken2);
                    column.Item().PaddingTop(2).Text("Resumen financiero").Bold().FontSize(14).FontColor(Colors.Grey.Darken1);
                    column.Item().PaddingTop(2).Text($"Período: {from:dd/MM/yyyy} - {to:dd/MM/yyyy}").FontSize(10).FontColor(Colors.Grey.Medium);
                    column.Item().PaddingTop(8).LineHorizontal(1).LineColor(Colors.Blue.Lighten2);
                });
                page.Content().PaddingTop(16).Column(column =>
                {
                    column.Item().Text("Ingresos (composición)").Bold().FontSize(11).FontColor(Colors.Grey.Darken1);
                    column.Item().PaddingTop(4).Row(r =>
                    {
                        r.RelativeItem().Column(c => { c.Item().Text("Reservaciones (pagadas)").FontSize(9).FontColor(Colors.Grey.Medium); c.Item().Text($"{incomeFromReservations:N2} {currency}").FontSize(11); });
                        r.RelativeItem().Column(c => { c.Item().Text("Facturas (pagadas)").FontSize(9).FontColor(Colors.Grey.Medium); c.Item().Text($"{incomeFromInvoices:N2} {currency}").FontSize(11); });
                    });
                    column.Item().PaddingTop(12).Background(Colors.Blue.Lighten4).Padding(16).Row(r =>
                    {
                        r.RelativeItem().Column(c => { c.Item().Text("Total ingresos").FontSize(11).FontColor(Colors.Grey.Darken1); c.Item().Text($"{totalIncome:N2} {currency}").Bold().FontSize(14).FontColor(Colors.Blue.Darken2); });
                        r.RelativeItem().Column(c => { c.Item().Text("Total egresos").FontSize(11).FontColor(Colors.Grey.Darken1); c.Item().Text($"{totalExpenses:N2} {currency}").Bold().FontSize(14).FontColor(Colors.Grey.Darken2); });
                        r.RelativeItem().Column(c => { c.Item().Text("Balance").FontSize(11).FontColor(Colors.Grey.Darken1); c.Item().Text($"{balance:N2} {currency}").Bold().FontSize(14).FontColor(balance >= 0 ? Colors.Green.Darken2 : Colors.Red.Darken2); });
                    });
                });
                page.Footer().AlignCenter().DefaultTextStyle(x => x.FontSize(8).FontColor(Colors.Grey.Medium)).Text(x => { x.Span("OptiControl · "); x.Span(DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm")); });
            });
        });
        return doc.GeneratePdf();
    }

    private List<Expense> GetExpensesList(DateTime? dateFrom, DateTime? dateTo)
    {
        var (from, to) = NormalizeDateRange(dateFrom, dateTo);
        return _context.Expenses.Where(e => e.Date >= from && e.Date <= to).OrderByDescending(e => e.Date).ToList();
    }

    private (DateTime from, DateTime to) NormalizeDateRange(DateTime? dateFrom, DateTime? dateTo)
    {
        var to = dateTo ?? DateTime.UtcNow.Date;
        var from = dateFrom ?? to.AddYears(-1);
        return (from, to);
    }

    private List<Client> GetClientsList(string? search)
    {
        var q = _context.Clients.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            q = q.Where(c => (c.Name != null && c.Name.ToLower().Contains(s)) || (c.Pasaporte != null && c.Pasaporte.ToLower().Contains(s)) || (c.Email != null && c.Email.ToLower().Contains(s)) || (c.Phone != null && c.Phone.ToLower().Contains(s)));
        }
        return q.OrderBy(c => c.Name).ToList();
    }

    private List<Reservation> GetReservationsList(int? clientId, string? paymentStatus, string? paymentMethod, DateTime? dateFrom, DateTime? dateTo)
    {
        var q = _context.Reservations.Include(r => r.Client).AsQueryable();
        if (clientId.HasValue) q = q.Where(r => r.ClientId == clientId.Value);
        if (!string.IsNullOrWhiteSpace(paymentStatus)) q = q.Where(r => r.PaymentStatus == paymentStatus);
        if (!string.IsNullOrWhiteSpace(paymentMethod)) q = q.Where(r => r.PaymentMethod == paymentMethod);
        if (dateFrom.HasValue) q = q.Where(r => r.StartDate >= dateFrom.Value);
        if (dateTo.HasValue) q = q.Where(r => r.EndDate <= dateTo.Value);
        return q.OrderByDescending(r => r.StartDate).ToList();
    }

    private List<Product> GetProductsList(string? search)
    {
        var q = _context.Products.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            q = q.Where(p =>
                (p.NombreProducto != null && p.NombreProducto.ToLower().Contains(s)) ||
                (p.Marca != null && p.Marca.ToLower().Contains(s)) ||
                (p.Proveedor != null && p.Proveedor.ToLower().Contains(s)) ||
                (p.Descripcion != null && p.Descripcion.ToLower().Contains(s)));
        }
        return q.OrderBy(p => p.NombreProducto).ToList();
    }

    private List<ServiceOptica> GetOpticsServicesList(string? search)
    {
        var q = _context.ServiceOpticas.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            q = q.Where(x =>
                (x.NombreServicio != null && x.NombreServicio.ToLower().Contains(s)) ||
                (x.Descripcion != null && x.Descripcion.ToLower().Contains(s)));
        }
        return q.OrderBy(x => x.NombreServicio).ToList();
    }

    private List<Sale> GetSalesHistoryList(DateTime? dateFrom, DateTime? dateTo, string? status, string? paymentMethod)
    {
        var q = _context.Sales
            .Include(s => s.SaleItems)
            .AsQueryable();
        if (dateFrom.HasValue) q = q.Where(s => s.Date >= dateFrom.Value);
        if (dateTo.HasValue) q = q.Where(s => s.Date <= dateTo.Value);
        if (!string.IsNullOrWhiteSpace(status)) q = q.Where(s => s.Status == status);
        if (!string.IsNullOrWhiteSpace(paymentMethod)) q = q.Where(s => s.PaymentMethod == paymentMethod);
        return q.OrderByDescending(s => s.Date).ToList();
    }

    private List<Invoice> GetInvoicesList(int? clientId, string? status, string? paymentMethod, DateTime? dateFrom, DateTime? dateTo)
    {
        var q = _context.Invoices.Include(i => i.Client).AsQueryable();
        if (clientId.HasValue) q = q.Where(i => i.ClientId == clientId.Value);
        if (!string.IsNullOrWhiteSpace(status)) q = q.Where(i => i.Status == status);
        if (!string.IsNullOrWhiteSpace(paymentMethod)) q = q.Where(i => i.PaymentMethod == paymentMethod);
        if (dateFrom.HasValue) q = q.Where(i => i.Date >= dateFrom.Value);
        if (dateTo.HasValue) q = q.Where(i => i.Date <= dateTo.Value);
        return q.OrderByDescending(i => i.Date).ToList();
    }

    private static byte[] BuildTablePdf(string companyName, string title, string[] headers, List<string[]> rows, bool narrowFont = false)
    {
        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);
                page.DefaultTextStyle(x => narrowFont ? x.FontSize(8) : x.FontSize(9));

                page.Header().Column(column =>
                {
                    column.Item().Text(companyName).Bold().FontSize(14).FontColor(Colors.Blue.Darken2);
                    column.Item().PaddingTop(2).Text(title).Bold().FontSize(12).FontColor(Colors.Grey.Darken1);
                    column.Item().PaddingTop(6).LineHorizontal(1).LineColor(Colors.Blue.Lighten2);
                });

                page.Content().Table(table =>
                {
                    var cols = headers.Length;
                    table.ColumnsDefinition(columns =>
                    {
                        for (var i = 0; i < cols; i++)
                            columns.RelativeColumn();
                    });

                    table.Header(header =>
                    {
                        for (var i = 0; i < cols; i++)
                            header.Cell().Background(Colors.Blue.Lighten3).Padding(6).DefaultTextStyle(x => x.Bold().FontSize(narrowFont ? 8 : 9)).Text(headers[i]);
                    });

                    for (var r = 0; r < rows.Count; r++)
                    {
                        var row = rows[r];
                        var bg = r % 2 == 0 ? Colors.White : Colors.Grey.Lighten4;
                        for (var i = 0; i < cols; i++)
                            table.Cell().Background(bg).Padding(5).Text(row[i]);
                    }
                });

                page.Footer().AlignCenter().DefaultTextStyle(x => x.FontSize(8).FontColor(Colors.Grey.Medium)).Text(x =>
                {
                    x.Span("Documento generado por OptiControl · ");
                    x.Span(DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm"));
                });
            });
        });
        return doc.GeneratePdf();
    }
}

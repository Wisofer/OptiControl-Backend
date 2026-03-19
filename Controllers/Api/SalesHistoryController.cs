using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OptiControl.Data;
using OptiControl.Models.Entities;
using OptiControl.Services.IServices;
using OptiControl.Utils;

namespace OptiControl.Controllers.Api;

[ApiController]
[Route("api/sales-history")]
[Authorize]
public class SalesHistoryController : ControllerBase
{
    private readonly IOpticsSaleService _service;
    private readonly IInvoiceService _invoiceService;
    private readonly ISaleTicketPdfService _saleTicketPdfService;
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContext;

    public SalesHistoryController(
        IOpticsSaleService service,
        IInvoiceService invoiceService,
        ISaleTicketPdfService saleTicketPdfService,
        ApplicationDbContext context,
        IHttpContextAccessor httpContext)
    {
        _service = service;
        _invoiceService = invoiceService;
        _saleTicketPdfService = saleTicketPdfService;
        _context = context;
        _httpContext = httpContext;
    }

    [HttpGet]
    public IActionResult GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        => Ok(_service.GetSalesHistoryPaged(page, pageSize));

    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var s = _service.GetSaleById(id);
        if (s == null) return NotFound(new { error = "Venta no encontrada" });
        return Ok(s);
    }

    /// <summary>URL del ticket PDF 80mm de la venta/cotización para imprimir desde frontend.</summary>
    [HttpGet("{id:int}/ticket-pdf-url")]
    public IActionResult GetTicketPdfUrl(int id)
    {
        var sale = _service.GetSaleById(id);
        if (sale == null) return NotFound(new { error = "Venta no encontrada" });
        var req = _httpContext.HttpContext?.Request;
        var scheme = req?.Scheme ?? "https";
        var host = req?.Host.ToString() ?? "opticontrol.cowib.es";
        var url = $"{scheme}://{host}/api/sales-history/{id}/ticket-pdf";
        return Ok(new { pdfUrl = url });
    }

    /// <summary>PDF ticket 80mm para venta/cotización.</summary>
    [HttpGet("{id:int}/ticket-pdf")]
    public IActionResult GetTicketPdf(int id)
    {
        var sale = _service.GetSaleById(id);
        if (sale == null) return NotFound(new { error = "Venta no encontrada" });
        var bytes = _saleTicketPdfService.GeneratePdf(id);
        if (bytes == null || bytes.Length == 0)
            return StatusCode(500, new { error = "No se pudo generar el ticket PDF." });
        Response.Headers.Append("Content-Disposition", $"inline; filename=\"Ticket-V{id}.pdf\"");
        return File(bytes, "application/pdf");
    }

    /// <summary>Cancelar venta o agregar abono. Body: { "status": "Cancelada" } o { "addPayment": 500 }</summary>
    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] SalesHistoryUpdateRequest request)
    {
        if (request == null) return BadRequest();
        if (!string.IsNullOrWhiteSpace(request.Status) && request.Status.Equals("Cancelada", StringComparison.OrdinalIgnoreCase))
        {
            var (success, error) = _service.CancelSale(id);
            if (!success) return BadRequest(new { error = error ?? "No se pudo cancelar" });
            var sale = _service.GetSaleById(id);
            return Ok(sale);
        }
        if (request.AddPayment.HasValue && request.AddPayment.Value > 0)
        {
            var (success, error) = _service.AddPayment(id, request.AddPayment.Value);
            if (!success) return BadRequest(new { error = error ?? "No se pudo registrar el abono" });
            var sale = _service.GetSaleById(id);
            return Ok(sale);
        }
        return BadRequest(new { error = "Envíe status: 'Cancelada' o addPayment > 0." });
    }

    /// <summary>
    /// Genera una factura desde una venta del historial y devuelve invoiceId + pdfUrl.
    /// Útil para que frontend imprima ticket 80mm del backend (como TripPilot).
    /// </summary>
    [HttpPost("{id:int}/invoice")]
    public IActionResult GenerateInvoiceFromSale(int id, [FromBody] GenerateInvoiceFromSaleRequest? request = null)
    {
        var sale = _context.Sales
            .Include(s => s.SaleItems)
            .FirstOrDefault(s => s.Id == id);
        if (sale == null) return NotFound(new { error = "Venta no encontrada." });
        if (!sale.ClientId.HasValue || sale.ClientId.Value <= 0)
            return BadRequest(new { error = "La venta no tiene cliente asociado para facturar." });
        if (string.Equals(sale.Status, SD.SaleStatusCotizacion, StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { error = "No se puede facturar una cotización. Conviértela a venta primero." });
        if (string.Equals(sale.Status, SD.SaleStatusCancelada, StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { error = "No se puede facturar una venta cancelada." });

        var defaultConcept = BuildSaleConcept(sale);
        var concept = string.IsNullOrWhiteSpace(request?.Concept) ? defaultConcept : request!.Concept!.Trim();

        // Evita duplicar facturas para la misma venta si ya existe con el mismo concepto base.
        var existing = _context.Invoices.FirstOrDefault(i => i.ClientId == sale.ClientId.Value && i.Concept == defaultConcept);
        if (existing != null)
        {
            return Ok(new
            {
                invoiceId = existing.Id,
                pdfUrl = BuildPdfUrl(existing.Id),
                reused = true
            });
        }

        var invoice = new Invoice
        {
            ClientId = sale.ClientId.Value,
            Date = DateTime.UtcNow,
            DueDate = request?.DueDate?.Date ?? DateTime.UtcNow.Date.AddDays(7),
            Amount = sale.Total,
            Status = IsPaidSale(sale.Status) ? SD.InvoiceStatusPagado : SD.InvoiceStatusPendiente,
            Concept = concept,
            PaymentMethod = MapInvoicePaymentMethod(sale.PaymentMethod, sale.Currency)
        };

        var created = _invoiceService.Create(invoice);
        return Ok(new
        {
            invoiceId = created.Id,
            pdfUrl = BuildPdfUrl(created.Id),
            reused = false
        });
    }

    private static bool IsPaidSale(string? status)
    {
        if (string.IsNullOrWhiteSpace(status)) return false;
        return status.Equals(SD.SaleStatusPagada, StringComparison.OrdinalIgnoreCase) ||
               status.Equals(SD.SaleStatusCompletado, StringComparison.OrdinalIgnoreCase);
    }

    private static string BuildSaleConcept(Sale sale)
    {
        var topItems = sale.SaleItems
            .Take(3)
            .Select(i => !string.IsNullOrWhiteSpace(i.ProductName) ? i.ProductName : i.ServiceName)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToList();
        var details = topItems.Count > 0 ? $" ({string.Join(", ", topItems)})" : "";
        return $"Venta V{sale.Id}{details}";
    }

    private static string MapInvoicePaymentMethod(string? salePaymentMethod, string? currency)
    {
        var isUsd = string.Equals(currency, SD.CurrencyUSD, StringComparison.OrdinalIgnoreCase);
        var isTransfer = !string.IsNullOrWhiteSpace(salePaymentMethod) &&
                         salePaymentMethod.Contains("transfer", StringComparison.OrdinalIgnoreCase);

        if (isUsd)
            return isTransfer ? SD.FormaPagoTransferenciaDolares : SD.FormaPagoDolares;
        return isTransfer ? SD.FormaPagoTransferenciaCordobas : SD.FormaPagoCordobas;
    }

    private string BuildPdfUrl(string invoiceId)
    {
        var req = _httpContext.HttpContext?.Request;
        var scheme = req?.Scheme ?? "https";
        var host = req?.Host.ToString() ?? "opticontrol.cowib.es";
        return $"{scheme}://{host}/api/invoices/{Uri.EscapeDataString(invoiceId)}/pdf";
    }
}

public class SalesHistoryUpdateRequest
{
    public string? Status { get; set; }
    public decimal? AddPayment { get; set; }
}

public class GenerateInvoiceFromSaleRequest
{
    public DateTime? DueDate { get; set; }
    public string? Concept { get; set; }
}

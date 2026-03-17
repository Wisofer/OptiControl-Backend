using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OptiControl.Models.Entities;
using OptiControl.Services.IServices;

namespace OptiControl.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InvoicesController : ControllerBase
{
    private readonly IInvoiceService _service;
    private readonly IInvoicePdfService _pdfService;
    private readonly IExportService _export;
    private readonly IHttpContextAccessor _httpContext;

    public InvoicesController(IInvoiceService service, IInvoicePdfService pdfService, IExportService export, IHttpContextAccessor httpContext)
    {
        _service = service;
        _pdfService = pdfService;
        _export = export;
        _httpContext = httpContext;
    }

    [HttpGet]
    public IActionResult GetAll([FromQuery] int? clientId, [FromQuery] string? status, [FromQuery] string? paymentMethod, [FromQuery] DateTime? dateFrom, [FromQuery] DateTime? dateTo, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        return Ok(_service.GetPaged(clientId, status, paymentMethod, dateFrom, dateTo, page, pageSize));
    }

    [HttpGet("export/excel")]
    public IActionResult ExportExcel([FromQuery] int? clientId, [FromQuery] string? status, [FromQuery] string? paymentMethod, [FromQuery] DateTime? dateFrom, [FromQuery] DateTime? dateTo)
    {
        var bytes = _export.GetInvoicesExcel(clientId, status, paymentMethod, dateFrom, dateTo);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Facturas.xlsx");
    }

    [HttpGet("export/pdf")]
    public IActionResult ExportPdf([FromQuery] int? clientId, [FromQuery] string? status, [FromQuery] string? paymentMethod, [FromQuery] DateTime? dateFrom, [FromQuery] DateTime? dateTo)
    {
        var bytes = _export.GetInvoicesPdf(clientId, status, paymentMethod, dateFrom, dateTo);
        return File(bytes, "application/pdf", "Facturas.pdf");
    }

    [HttpGet("{id}")]
    public IActionResult GetById(string id)
    {
        var i = _service.GetById(id);
        if (i == null) return NotFound();
        return Ok(i);
    }

    /// <summary>URL pública del PDF para usar en plantilla WhatsApp ({EnlacePDF}).</summary>
    [HttpGet("{id}/pdf-url")]
    public IActionResult GetPdfUrl(string id)
    {
        var i = _service.GetById(id);
        if (i == null) return NotFound();
        var req = _httpContext.HttpContext?.Request;
        var scheme = req?.Scheme ?? "https";
        var host = req?.Host.ToString() ?? "trippilot.cowib.es";
        var baseUrl = $"{scheme}://{host}";
        var pdfUrl = $"{baseUrl}/api/public/invoices/{Uri.EscapeDataString(id)}/pdf";
        return Ok(new { pdfUrl });
    }

    /// <summary>Misma factura PDF (estilo ticket) para imprimir o descargar desde la app. Requiere autenticación.</summary>
    [HttpGet("{id}/pdf")]
    public IActionResult GetPdf(string id)
    {
        var i = _service.GetById(id);
        if (i == null) return NotFound();
        var pdfBytes = _pdfService.GeneratePdf(id);
        if (pdfBytes == null || pdfBytes.Length == 0)
            return StatusCode(500, new { error = "No se pudo generar el PDF." });
        var fileName = $"Factura-{id}.pdf";
        Response.Headers.Append("Content-Disposition", $"inline; filename=\"{fileName}\"");
        return File(pdfBytes, "application/pdf");
    }

    [HttpGet("next-code")]
    public IActionResult GetNextCode()
    {
        return Ok(new { code = _service.GetNextInvoiceCode() });
    }

    [HttpPost]
    public IActionResult Create([FromBody] Invoice invoice)
    {
        if (invoice == null || invoice.ClientId == 0) return BadRequest();
        var created = _service.Create(invoice);
        return Ok(created);
    }

    [HttpPut("{id}")]
    public IActionResult Update(string id, [FromBody] Invoice invoice)
    {
        if (invoice == null || invoice.Id != id) return BadRequest();
        if (!_service.Update(invoice)) return NotFound();
        return Ok(invoice);
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(string id)
    {
        if (!_service.Delete(id)) return NotFound();
        return NoContent();
    }
}

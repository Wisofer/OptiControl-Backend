using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OptiControl.Services.IServices;

namespace OptiControl.Controllers.Api;

/// <summary>Endpoint público para descarga del PDF de factura (sin login). Para enlaces enviados por WhatsApp.</summary>
[ApiController]
[Route("api/public/invoices")]
[AllowAnonymous]
public class PublicInvoicesController : ControllerBase
{
    private readonly IInvoicePdfService _pdfService;
    private readonly IInvoiceService _invoiceService;

    public PublicInvoicesController(IInvoicePdfService pdfService, IInvoiceService invoiceService)
    {
        _pdfService = pdfService;
        _invoiceService = invoiceService;
    }

    /// <summary>Descarga el PDF de la factura. Content-Disposition: attachment.</summary>
    [HttpGet("{id}/pdf")]
    public IActionResult GetPdf(string id)
    {
        var invoice = _invoiceService.GetById(id);
        if (invoice == null) return NotFound(new { error = "Factura no encontrada." });

        var pdfBytes = _pdfService.GeneratePdf(id);
        if (pdfBytes == null || pdfBytes.Length == 0)
            return StatusCode(500, new { error = "No se pudo generar el PDF." });

        var fileName = $"Factura-{id}.pdf";
        return File(pdfBytes, "application/pdf", fileName);
    }
}

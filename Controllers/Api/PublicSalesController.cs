using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OptiControl.Services.IServices;

namespace OptiControl.Controllers.Api;

/// <summary>Endpoint público para descarga del PDF de ticket de venta (sin login). Para enlaces enviados por WhatsApp.</summary>
[ApiController]
[Route("api/public/sales")]
[AllowAnonymous]
public class PublicSalesController : ControllerBase
{
    private readonly IOpticsSaleService _saleService;
    private readonly ISaleTicketPdfService _saleTicketPdfService;

    public PublicSalesController(IOpticsSaleService saleService, ISaleTicketPdfService saleTicketPdfService)
    {
        _saleService = saleService;
        _saleTicketPdfService = saleTicketPdfService;
    }

    /// <summary>Descarga el PDF del ticket de venta/cotización. Content-Disposition: attachment.</summary>
    [HttpGet("{id:int}/ticket-pdf")]
    public IActionResult GetTicketPdf(int id)
    {
        var sale = _saleService.GetSaleById(id);
        if (sale == null) return NotFound(new { error = "Venta no encontrada." });

        var bytes = _saleTicketPdfService.GeneratePdf(id);
        if (bytes == null || bytes.Length == 0)
            return StatusCode(500, new { error = "No se pudo generar el PDF." });

        var fileName = $"Ticket-V{id}.pdf";
        return File(bytes, "application/pdf", fileName);
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OptiControl.Services.IServices;

namespace OptiControl.Controllers.Api;

[ApiController]
[Route("api/sales-history")]
[Authorize]
public class SalesHistoryController : ControllerBase
{
    private readonly IOpticsSaleService _service;

    public SalesHistoryController(IOpticsSaleService service) => _service = service;

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
}

public class SalesHistoryUpdateRequest
{
    public string? Status { get; set; }
    public decimal? AddPayment { get; set; }
}

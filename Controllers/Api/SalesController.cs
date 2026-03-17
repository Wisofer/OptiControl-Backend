using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OptiControl.Models.Dtos;
using OptiControl.Services.IServices;

namespace OptiControl.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SalesController : ControllerBase
{
    private readonly IOpticsSaleService _service;

    public SalesController(IOpticsSaleService service) => _service = service;

    /// <summary>Registrar venta o cotización (POS).</summary>
    [HttpPost]
    public IActionResult Create([FromBody] CreateSaleRequestDto dto)
    {
        if (dto == null || (dto.Items == null || dto.Items.Count == 0))
            return BadRequest(new { error = "items es requerido." });
        var result = _service.CreateSale(dto);
        if (result == null)
            return BadRequest(new { error = "Stock insuficiente en uno o más productos." });
        return Ok(result);
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OptiControl.Models.Entities;
using OptiControl.Services.IServices;

namespace OptiControl.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "Administrador")]
public class CajaController : ControllerBase
{
    private readonly ICajaService _service;

    public CajaController(ICajaService service) => _service = service;

    [HttpGet]
    public IActionResult GetByRange([FromQuery] DateTime? dateFrom, [FromQuery] DateTime? dateTo)
    {
        var from = dateFrom ?? DateTime.UtcNow.Date.AddMonths(-1);
        var to = dateTo ?? DateTime.UtcNow.Date;
        return Ok(_service.GetByRange(from, to));
    }

    [HttpGet("{date}")]
    public IActionResult GetByDate(string date)
    {
        if (!DateTime.TryParse(date, out var d)) return BadRequest();
        var c = _service.GetByDate(d);
        if (c == null) return NotFound();
        return Ok(c);
    }

    [HttpPost]
    public IActionResult CreateOrUpdate([FromBody] CajaDiaria caja)
    {
        if (caja == null) return BadRequest();
        var result = _service.CreateOrUpdate(caja);
        return Ok(result);
    }

    [HttpPut("{date}")]
    public IActionResult Update(string date, [FromBody] CajaDiaria caja)
    {
        if (caja == null || !DateTime.TryParse(date, out var d)) return BadRequest();
        caja.Date = d.Date;
        var result = _service.CreateOrUpdate(caja);
        return Ok(result);
    }
}

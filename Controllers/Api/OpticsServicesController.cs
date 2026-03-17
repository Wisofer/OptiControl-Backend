using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OptiControl.Models.Entities;
using OptiControl.Services.IServices;

namespace OptiControl.Controllers.Api;

[ApiController]
[Route("api/services")]
[Authorize]
public class OpticsServicesController : ControllerBase
{
    private readonly IServiceOpticaService _service;

    public OpticsServicesController(IServiceOpticaService service) => _service = service;

    [HttpGet]
    public IActionResult GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null)
        => Ok(_service.GetPaged(page, pageSize, search));

    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var s = _service.GetById(id);
        if (s == null) return NotFound();
        return Ok(s);
    }

    [HttpPost]
    public IActionResult Create([FromBody] ServiceOptica service)
    {
        if (service == null || string.IsNullOrWhiteSpace(service.NombreServicio))
            return BadRequest(new { error = "nombre_servicio es requerido." });
        var created = _service.Create(service);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] ServiceOptica service)
    {
        if (service == null) return BadRequest();
        var updated = _service.Update(id, service);
        if (updated == null) return NotFound();
        return Ok(updated);
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        if (!_service.Delete(id)) return NotFound();
        return NoContent();
    }
}

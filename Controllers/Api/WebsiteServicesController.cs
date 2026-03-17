using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OptiControl.Models.Entities;
using OptiControl.Services.IServices;

namespace OptiControl.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "Administrador")]
public class WebsiteServicesController : ControllerBase
{
    private readonly IWebsiteServiceService _service;

    public WebsiteServicesController(IWebsiteServiceService service) => _service = service;

    [HttpGet]
    public IActionResult GetAll() => Ok(_service.GetAll());

    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var s = _service.GetById(id);
        if (s == null) return NotFound();
        return Ok(s);
    }

    [HttpPost]
    public IActionResult Create([FromBody] WebsiteService service)
    {
        if (service == null || string.IsNullOrWhiteSpace(service.Title)) return BadRequest();
        var created = _service.Create(service);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] WebsiteService service)
    {
        if (service == null || service.Id != id) return BadRequest();
        if (!_service.Update(service)) return NotFound();
        return Ok(service);
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        if (!_service.Delete(id)) return NotFound();
        return NoContent();
    }
}

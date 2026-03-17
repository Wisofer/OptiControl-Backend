using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OptiControl.Models.Entities;
using OptiControl.Services.IServices;

namespace OptiControl.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "Administrador")]
public class TestimonialsController : ControllerBase
{
    private readonly ITestimonialService _service;

    public TestimonialsController(ITestimonialService service) => _service = service;

    [HttpGet]
    public IActionResult GetAll() => Ok(_service.GetAll());

    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var t = _service.GetById(id);
        if (t == null) return NotFound();
        return Ok(t);
    }

    [HttpPost]
    public IActionResult Create([FromBody] Testimonial testimonial)
    {
        if (testimonial == null || string.IsNullOrWhiteSpace(testimonial.Quote) || string.IsNullOrWhiteSpace(testimonial.AuthorName))
            return BadRequest();
        var created = _service.Create(testimonial);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] Testimonial testimonial)
    {
        if (testimonial == null || testimonial.Id != id) return BadRequest();
        if (!_service.Update(testimonial)) return NotFound();
        return Ok(testimonial);
    }

    [HttpPut("{id:int}/approve")]
    public IActionResult SetApproved(int id, [FromBody] bool approved)
    {
        if (!_service.SetApproved(id, approved)) return NotFound();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        if (!_service.Delete(id)) return NotFound();
        return NoContent();
    }
}

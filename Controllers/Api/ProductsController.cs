using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OptiControl.Models.Entities;
using OptiControl.Services.IServices;

namespace OptiControl.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductsController : ControllerBase
{
    private readonly IProductService _service;

    public ProductsController(IProductService service) => _service = service;

    [HttpGet]
    public IActionResult GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null)
        => Ok(_service.GetPaged(page, pageSize, search));

    /// <summary>Productos con stock actual por debajo del stock mínimo (para alertas).</summary>
    [HttpGet("low-stock")]
    public IActionResult GetLowStock() => Ok(_service.GetLowStock());

    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var p = _service.GetById(id);
        if (p == null) return NotFound();
        return Ok(p);
    }

    [HttpPost]
    public IActionResult Create([FromBody] Product product)
    {
        if (product == null || string.IsNullOrWhiteSpace(product.NombreProducto))
            return BadRequest(new { error = "nombre_producto es requerido." });
        var created = _service.Create(product);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] Product product)
    {
        if (product == null) return BadRequest();
        var updated = _service.Update(id, product);
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

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OptiControl.Models.Entities;
using OptiControl.Services.IServices;

namespace OptiControl.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "Administrador")]
public class ExpensesController : ControllerBase
{
    private readonly IExpenseService _service;

    public ExpensesController(IExpenseService service) => _service = service;

    [HttpGet]
    public IActionResult GetAll([FromQuery] DateTime? dateFrom, [FromQuery] DateTime? dateTo, [FromQuery] string? category, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        => Ok(_service.GetPaged(dateFrom, dateTo, category, page, pageSize));

    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var e = _service.GetById(id);
        if (e == null) return NotFound();
        return Ok(e);
    }

    [HttpPost]
    public IActionResult Create([FromBody] Expense expense)
    {
        if (expense == null || string.IsNullOrWhiteSpace(expense.Concept)) return BadRequest();
        var created = _service.Create(expense);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] Expense expense)
    {
        if (expense == null || expense.Id != id) return BadRequest();
        if (!_service.Update(expense)) return NotFound();
        return Ok(expense);
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        if (!_service.Delete(id)) return NotFound();
        return NoContent();
    }
}

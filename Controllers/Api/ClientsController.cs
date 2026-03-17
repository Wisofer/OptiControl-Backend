using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OptiControl.Models.Entities;
using OptiControl.Services.IServices;

namespace OptiControl.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ClientsController : ControllerBase
{
    private readonly IClientService _service;
    private readonly IExportService _export;

    public ClientsController(IClientService service, IExportService export)
    {
        _service = service;
        _export = export;
    }

    [HttpGet]
    public IActionResult GetAll([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        => Ok(_service.GetPagedOptics(search, page, pageSize));

    [HttpGet("export/excel")]
    public IActionResult ExportExcel([FromQuery] string? search)
    {
        var bytes = _export.GetClientsExcel(search);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Clientes.xlsx");
    }

    [HttpGet("export/pdf")]
    public IActionResult ExportPdf([FromQuery] string? search)
    {
        var bytes = _export.GetClientsPdf(search);
        return File(bytes, "application/pdf", "Clientes.pdf");
    }

    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var c = _service.GetByIdOptics(id);
        if (c == null) return NotFound();
        return Ok(c);
    }

    [HttpGet("{id:int}/history")]
    public IActionResult GetHistory(int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var h = _service.GetHistoryOptics(id, page, pageSize);
        if (h == null) return NotFound();
        return Ok(h);
    }

    [HttpPost]
    public IActionResult Create([FromBody] Client client)
    {
        if (client == null || string.IsNullOrWhiteSpace(client.Name))
            return BadRequest();
        if (client.FechaRegistro == default) client.FechaRegistro = DateTime.UtcNow.Date;
        var created = _service.Create(client);
        var optics = _service.GetByIdOptics(created.Id);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, optics);
    }

    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] Client client)
    {
        if (client == null || client.Id != id) return BadRequest();
        client.Id = id;
        if (!_service.Update(client)) return NotFound();
        return Ok(_service.GetByIdOptics(id));
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        if (!_service.Delete(id)) return NotFound();
        return NoContent();
    }
}

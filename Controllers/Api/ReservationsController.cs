using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OptiControl.Models.Entities;
using OptiControl.Services.IServices;

namespace OptiControl.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReservationsController : ControllerBase
{
    private readonly IReservationService _service;
    private readonly IExportService _export;

    public ReservationsController(IReservationService service, IExportService export)
    {
        _service = service;
        _export = export;
    }

    [HttpGet]
    public IActionResult GetAll(
        [FromQuery] int? clientId,
        [FromQuery] string? paymentStatus,
        [FromQuery] string? paymentMethod,
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        return Ok(_service.GetPaged(clientId, paymentStatus, paymentMethod, dateFrom, dateTo, page, pageSize));
    }

    [HttpGet("export/excel")]
    public IActionResult ExportExcel(
        [FromQuery] int? clientId,
        [FromQuery] string? paymentStatus,
        [FromQuery] string? paymentMethod,
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo)
    {
        var bytes = _export.GetReservationsExcel(clientId, paymentStatus, paymentMethod, dateFrom, dateTo);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Reservaciones.xlsx");
    }

    [HttpGet("export/pdf")]
    public IActionResult ExportPdf(
        [FromQuery] int? clientId,
        [FromQuery] string? paymentStatus,
        [FromQuery] string? paymentMethod,
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo)
    {
        var bytes = _export.GetReservationsPdf(clientId, paymentStatus, paymentMethod, dateFrom, dateTo);
        return File(bytes, "application/pdf", "Reservaciones.pdf");
    }

    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var r = _service.GetById(id);
        if (r == null) return NotFound();
        return Ok(r);
    }

    [HttpPost]
    public IActionResult Create([FromBody] Reservation reservation)
    {
        if (reservation == null || reservation.ClientId == 0 || string.IsNullOrWhiteSpace(reservation.Destination))
            return BadRequest();
        var created = _service.Create(reservation);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] Reservation reservation)
    {
        if (reservation == null || reservation.Id != id) return BadRequest();
        if (!_service.Update(reservation)) return NotFound();
        return Ok(reservation);
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        if (!_service.Delete(id)) return NotFound();
        return NoContent();
    }
}

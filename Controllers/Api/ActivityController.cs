using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OptiControl.Services.IServices;

namespace OptiControl.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "Administrador")]
public class ActivityController : ControllerBase
{
    private readonly IActivityService _service;
    private readonly IDashboardOpticsService _dashboardOptics;

    public ActivityController(IActivityService service, IDashboardOpticsService dashboardOptics)
    {
        _service = service;
        _dashboardOptics = dashboardOptics;
    }

    /// <summary>Actividad reciente (spec OptiControl: id, type, description, time). Query: limit, from (fecha desde).</summary>
    [HttpGet]
    public IActionResult GetAll([FromQuery] int limit = 50, [FromQuery] DateTime? from = null)
    {
        return Ok(_dashboardOptics.GetRecentActivity(limit));
    }
}

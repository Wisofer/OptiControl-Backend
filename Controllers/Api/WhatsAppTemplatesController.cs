using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OptiControl.Models.Entities;
using OptiControl.Services.IServices;

namespace OptiControl.Controllers.Api;

[ApiController]
[Route("api/whatsapp-templates")]
[Authorize(Policy = "Administrador")]
public class WhatsAppTemplatesController : ControllerBase
{
    private readonly IWhatsAppTemplateService _service;

    public WhatsAppTemplatesController(IWhatsAppTemplateService service) => _service = service;

    /// <summary>Listar plantillas. Query: onlyActive = true para solo activas.</summary>
    [HttpGet]
    public IActionResult GetAll([FromQuery] bool onlyActive = false)
        => Ok(_service.GetAll(onlyActive).Select(ToDto));

    /// <summary>Obtener plantilla predeterminada.</summary>
    [HttpGet("default")]
    public IActionResult GetDefault()
    {
        var t = _service.GetDefault();
        if (t == null) return NotFound(new { error = "No hay plantilla predeterminada." });
        return Ok(ToDto(t));
    }

    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var t = _service.GetById(id);
        if (t == null) return NotFound();
        return Ok(ToDto(t));
    }

    [HttpPost]
    public IActionResult Create([FromBody] WhatsAppTemplateDto dto)
    {
        if (dto == null || string.IsNullOrWhiteSpace(dto.Nombre) || string.IsNullOrWhiteSpace(dto.Mensaje))
            return BadRequest(new { error = "Nombre y mensaje son requeridos." });
        var t = new WhatsAppTemplate
        {
            Nombre = dto.Nombre.Trim(),
            Mensaje = dto.Mensaje.Trim(),
            Activa = dto.Activa ?? true,
            Predeterminada = dto.Predeterminada ?? false
        };
        var created = _service.Create(t);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, ToDto(created));
    }

    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] WhatsAppTemplateDto dto)
    {
        if (dto == null) return BadRequest();
        var t = _service.GetById(id);
        if (t == null) return NotFound();
        t.Nombre = dto.Nombre ?? t.Nombre;
        t.Mensaje = dto.Mensaje ?? t.Mensaje;
        if (dto.Activa.HasValue) t.Activa = dto.Activa.Value;
        if (dto.Predeterminada.HasValue) t.Predeterminada = dto.Predeterminada.Value;
        if (!_service.Update(t)) return NotFound();
        return Ok(ToDto(t));
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var t = _service.GetById(id);
        if (t == null) return NotFound();
        if (t.Predeterminada && !_service.GetAll().Any(x => x.Id != id))
            return BadRequest(new { error = "No puedes eliminar la única plantilla. Asigna otra como predeterminada antes." });
        if (!_service.Delete(id)) return BadRequest(new { error = "No se puede eliminar la plantilla predeterminada si es la única." });
        return NoContent();
    }

    static object ToDto(WhatsAppTemplate t) => new
    {
        id = t.Id,
        nombre = t.Nombre,
        mensaje = t.Mensaje,
        activa = t.Activa,
        predeterminada = t.Predeterminada
    };
}

public class WhatsAppTemplateDto
{
    public string? Nombre { get; set; }
    public string? Mensaje { get; set; }
    public bool? Activa { get; set; }
    public bool? Predeterminada { get; set; }
}

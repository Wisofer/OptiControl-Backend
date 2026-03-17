using Microsoft.AspNetCore.Mvc;
using OptiControl.Models.Dtos;
using OptiControl.Models.Entities;
using OptiControl.Services.IServices;

namespace OptiControl.Controllers.Api;

/// <summary>Endpoints públicos para la página web (sin autenticación).</summary>
[ApiController]
[Route("api/public")]
public class PublicController : ControllerBase
{
    private readonly IWebsiteServiceService _websiteServiceService;
    private readonly ITestimonialService _testimonialService;

    public PublicController(IWebsiteServiceService websiteServiceService, ITestimonialService testimonialService)
    {
        _websiteServiceService = websiteServiceService;
        _testimonialService = testimonialService;
    }

    /// <summary>Lista de servicios activos para la sección "Nuestros Servicios" de la web.</summary>
    [HttpGet("services")]
    public IActionResult GetServices() => Ok(_websiteServiceService.GetActiveForPublic());

    /// <summary>Lista de testimonios aprobados y activos para "Lo que dicen nuestros clientes". Incluye Rating (estrellas 1-5).</summary>
    [HttpGet("testimonials")]
    public IActionResult GetTestimonials() => Ok(_testimonialService.GetApprovedForPublic());

    /// <summary>Envía un testimonio desde el formulario de la web. Se guarda como pendiente de aprobación por el admin.</summary>
    [HttpPost("testimonials")]
    public IActionResult CreateTestimonial([FromBody] CreateTestimonialDto dto)
    {
        if (dto == null) return BadRequest();
        var testimonial = new Testimonial
        {
            Quote = dto.Quote,
            AuthorName = dto.AuthorName,
            Location = dto.Location,
            Rating = Math.Clamp(dto.Rating, 1, 5),
            SortOrder = 0,
            IsActive = true,
            IsApproved = false
        };
        var created = _testimonialService.Create(testimonial);
        return Created($"/api/public/testimonials", new { id = created.Id, message = "Gracias. Tu comentario será revisado antes de publicarse." });
    }
}

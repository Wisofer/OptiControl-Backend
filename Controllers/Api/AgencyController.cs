using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OptiControl.Models.Dtos;
using OptiControl.Models.Entities;
using OptiControl.Services.IServices;

namespace OptiControl.Controllers.Api;

/// <summary>Datos de la agencia (nombre de empresa, contacto, moneda). Independiente del tipo de cambio.</summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "Administrador")]
public class AgencyController : ControllerBase
{
    private readonly ISettingsService _settings;

    public AgencyController(ISettingsService settings)
    {
        _settings = settings;
    }

    /// <summary>Obtener datos de la agencia para el formulario "Datos de la Agencia".</summary>
    [HttpGet]
    public IActionResult Get()
    {
        var s = _settings.Get();
        if (s == null)
        {
            return Ok(new AgencyDataDto
            {
                Id = 0,
                CompanyName = "Aventours",
                Currency = "NIO",
                Email = null,
                Phone = null,
                Address = null,
                UpdatedAt = null
            });
        }
        return Ok(new AgencyDataDto
        {
            Id = s.Id,
            CompanyName = s.CompanyName ?? "",
            Email = s.Email,
            Phone = s.Phone,
            Address = s.Address,
            Currency = s.Currency ?? "NIO",
            UpdatedAt = s.UpdatedAt
        });
    }

    /// <summary>Guardar datos de la agencia (nombre, correo, teléfono, dirección, moneda). No modifica el tipo de cambio.</summary>
    [HttpPut]
    public IActionResult Put([FromBody] AgencyDataDto dto)
    {
        if (dto == null || string.IsNullOrWhiteSpace(dto.CompanyName))
            return BadRequest(new { error = "companyName es requerido." });
        var existing = _settings.Get();
        if (existing == null)
        {
            existing = new AgencySettings
            {
                CompanyName = dto.CompanyName.Trim(),
                Email = dto.Email?.Trim(),
                Phone = dto.Phone?.Trim(),
                Address = dto.Address?.Trim(),
                Currency = dto.Currency?.Trim() ?? "NIO",
                Language = "es",
                ExchangeRate = 36.8m,
                Theme = "light",
                SoundVolume = 80,
                AlertsReservacionesPendientes = true,
                AlertsFacturasVencidas = true,
                AlertsRecordatorios = true
            };
            _settings.Save(existing);
        }
        else
        {
            existing.CompanyName = dto.CompanyName.Trim();
            existing.Email = dto.Email?.Trim();
            existing.Phone = dto.Phone?.Trim();
            existing.Address = dto.Address?.Trim();
            if (!string.IsNullOrWhiteSpace(dto.Currency))
                existing.Currency = dto.Currency.Trim();
            existing.UpdatedAt = DateTime.UtcNow;
            _settings.Save(existing);
        }
        var updated = _settings.Get();
        return Ok(new AgencyDataDto
        {
            Id = updated!.Id,
            CompanyName = updated.CompanyName ?? "",
            Email = updated.Email,
            Phone = updated.Phone,
            Address = updated.Address,
            Currency = updated.Currency ?? "NIO",
            UpdatedAt = updated.UpdatedAt
        });
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OptiControl.Models.Dtos;
using OptiControl.Services.IServices;

namespace OptiControl.Controllers.Api;

/// <summary>Tipo de cambio (C$ = 1 USD). Independiente de la configuración general.</summary>
[ApiController]
[Route("api/exchange-rate")]
[Authorize(Policy = "Administrador")]
public class ExchangeRateController : ControllerBase
{
    private readonly ISettingsService _settings;

    public ExchangeRateController(ISettingsService settings)
    {
        _settings = settings;
    }

    /// <summary>Obtener el tipo de cambio actual (cuántos córdobas por 1 USD).</summary>
    [HttpGet]
    public IActionResult Get()
    {
        var settings = _settings.Get();
        var rate = settings?.ExchangeRate ?? 36.8m;
        return Ok(new ExchangeRateDto { ExchangeRate = rate });
    }

    /// <summary>Guardar el tipo de cambio. Se usa para convertir ventas/pagos en dólares a C$ en totales y reportes.</summary>
    [HttpPut]
    public IActionResult Put([FromBody] ExchangeRateDto dto)
    {
        if (dto == null || dto.ExchangeRate <= 0)
            return BadRequest(new { error = "exchangeRate debe ser un número positivo." });
        var settings = _settings.Get();
        if (settings == null)
        {
            settings = new OptiControl.Models.Entities.AgencySettings
            {
                CompanyName = "Aventours",
                Currency = "NIO",
                Language = "es",
                ExchangeRate = dto.ExchangeRate,
                Theme = "light",
                SoundVolume = 80,
                AlertsReservacionesPendientes = true,
                AlertsFacturasVencidas = true,
                AlertsRecordatorios = true
            };
            _settings.Save(settings);
        }
        else
        {
            settings.ExchangeRate = dto.ExchangeRate;
            _settings.Save(settings);
        }
        return Ok(new ExchangeRateDto { ExchangeRate = dto.ExchangeRate });
    }
}

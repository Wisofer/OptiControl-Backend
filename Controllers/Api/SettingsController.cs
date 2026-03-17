using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OptiControl.Models.Entities;
using OptiControl.Services.IServices;

namespace OptiControl.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "Administrador")]
public class SettingsController : ControllerBase
{
    private readonly ISettingsService _settings;

    public SettingsController(ISettingsService settings)
    {
        _settings = settings;
    }

    [HttpGet]
    public IActionResult Get()
    {
        var s = _settings.Get();
        if (s == null)
            return Ok(new AgencySettings
            {
                CompanyName = "Aventours",
                Currency = "NIO",
                Language = "es",
                ExchangeRate = 36.8m,
                Theme = "light",
                SoundVolume = 80,
                AlertsReservacionesPendientes = true,
                AlertsFacturasVencidas = true,
                AlertsRecordatorios = true
            });
        return Ok(s);
    }

    /// <summary>Acepta cuerpos parciales: solo se actualizan los campos presentes en el body. No se borran ni sobrescriben con null/vacío los que no vengan.</summary>
    [HttpPut]
    public IActionResult Put([FromBody] JsonElement body)
    {
        var existing = _settings.Get();
        if (existing == null)
        {
            existing = new AgencySettings
            {
                CompanyName = "Aventours",
                Currency = "NIO",
                Language = "es",
                ExchangeRate = 36.8m,
                Theme = "light",
                SoundVolume = 80,
                AlertsReservacionesPendientes = true,
                AlertsFacturasVencidas = true,
                AlertsRecordatorios = true
            };
        }

        if (body.TryGetProperty("theme", out var themeEl) && themeEl.ValueKind == JsonValueKind.String)
            existing.Theme = themeEl.GetString() ?? existing.Theme;
        if (body.TryGetProperty("exchangeRate", out var rateEl) && rateEl.ValueKind == JsonValueKind.Number)
            existing.ExchangeRate = rateEl.GetDecimal();
        if (body.TryGetProperty("companyName", out var cn) && cn.ValueKind != JsonValueKind.Null && cn.ValueKind != JsonValueKind.Undefined)
            existing.CompanyName = cn.GetString() ?? existing.CompanyName;
        if (body.TryGetProperty("email", out var em))
            existing.Email = (em.ValueKind == JsonValueKind.Null || em.ValueKind == JsonValueKind.Undefined) ? existing.Email : em.GetString();
        if (body.TryGetProperty("phone", out var ph))
            existing.Phone = (ph.ValueKind == JsonValueKind.Null || ph.ValueKind == JsonValueKind.Undefined) ? existing.Phone : ph.GetString();
        if (body.TryGetProperty("address", out var ad))
            existing.Address = (ad.ValueKind == JsonValueKind.Null || ad.ValueKind == JsonValueKind.Undefined) ? existing.Address : ad.GetString();
        if (body.TryGetProperty("currency", out var cur) && cur.ValueKind == JsonValueKind.String)
            existing.Currency = cur.GetString() ?? existing.Currency;
        if (body.TryGetProperty("language", out var lang) && lang.ValueKind == JsonValueKind.String)
            existing.Language = lang.GetString() ?? existing.Language;
        if (body.TryGetProperty("soundVolume", out var vol) && vol.ValueKind == JsonValueKind.Number)
            existing.SoundVolume = vol.GetInt32();
        if (body.TryGetProperty("alertsReservacionesPendientes", out var a1) && (a1.ValueKind == JsonValueKind.True || a1.ValueKind == JsonValueKind.False))
            existing.AlertsReservacionesPendientes = a1.GetBoolean();
        if (body.TryGetProperty("alertsFacturasVencidas", out var a2) && (a2.ValueKind == JsonValueKind.True || a2.ValueKind == JsonValueKind.False))
            existing.AlertsFacturasVencidas = a2.GetBoolean();
        if (body.TryGetProperty("alertsRecordatorios", out var a3) && (a3.ValueKind == JsonValueKind.True || a3.ValueKind == JsonValueKind.False))
            existing.AlertsRecordatorios = a3.GetBoolean();

        existing.UpdatedAt = DateTime.UtcNow;
        _settings.Save(existing);
        return Ok(existing);
    }
}

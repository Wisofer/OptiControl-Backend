using OptiControl.Data;
using OptiControl.Models.Entities;
using OptiControl.Services.IServices;
using OptiControl.Utils;

namespace OptiControl.Services;

public class SettingsService : ISettingsService
{
    private const string DefaultCompanyName = "Aventours";
    private readonly ApplicationDbContext _context;

    public SettingsService(ApplicationDbContext context)
    {
        _context = context;
    }

    public AgencySettings? Get()
    {
        return _context.AgencySettings.OrderBy(s => s.Id).FirstOrDefault();
    }

    public void Save(AgencySettings settings)
    {
        var existing = Get();
        if (existing == null)
        {
            settings.Id = 0;
            _context.AgencySettings.Add(settings);
        }
        else
        {
            // Merge: no sobrescribir datos de la agencia con valores vacíos (evita que PUT /api/settings con body incompleto borre nombre, correo, etc.)
            existing.CompanyName = string.IsNullOrWhiteSpace(settings.CompanyName) ? existing.CompanyName : settings.CompanyName.Trim();
            existing.Email = settings.Email ?? existing.Email;
            existing.Phone = settings.Phone ?? existing.Phone;
            existing.Address = settings.Address ?? existing.Address;
            existing.Currency = string.IsNullOrWhiteSpace(settings.Currency) ? existing.Currency : settings.Currency.Trim();
            existing.Language = string.IsNullOrWhiteSpace(settings.Language) ? existing.Language : settings.Language.Trim();
            // Tipo de cambio y preferencias siempre se actualizan con lo que venga
            existing.ExchangeRate = settings.ExchangeRate;
            existing.Theme = settings.Theme;
            existing.SoundVolume = settings.SoundVolume;
            existing.AlertsReservacionesPendientes = settings.AlertsReservacionesPendientes;
            existing.AlertsFacturasVencidas = settings.AlertsFacturasVencidas;
            existing.AlertsRecordatorios = settings.AlertsRecordatorios;
            existing.UpdatedAt = TimeZoneHelper.UtcNow();
        }
        _context.SaveChanges();
    }

    public string GetCompanyName()
    {
        return Get()?.CompanyName?.Trim() ?? DefaultCompanyName;
    }
}

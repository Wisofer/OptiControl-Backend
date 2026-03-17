using OptiControl.Models.Entities;

namespace OptiControl.Services.IServices;

public interface ISettingsService
{
    AgencySettings? Get();
    void Save(AgencySettings settings);
    string GetCompanyName();
}

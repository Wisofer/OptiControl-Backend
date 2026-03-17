using OptiControl.Models.Entities;

namespace OptiControl.Services.IServices;

public interface IWhatsAppTemplateService
{
    List<WhatsAppTemplate> GetAll(bool onlyActive = false);
    WhatsAppTemplate? GetById(int id);
    WhatsAppTemplate? GetDefault();
    WhatsAppTemplate Create(WhatsAppTemplate template);
    bool Update(WhatsAppTemplate template);
    bool Delete(int id);
}

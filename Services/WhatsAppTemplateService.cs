using OptiControl.Data;
using OptiControl.Models.Entities;
using OptiControl.Services.IServices;
using OptiControl.Utils;

namespace OptiControl.Services;

public class WhatsAppTemplateService : IWhatsAppTemplateService
{
    private readonly ApplicationDbContext _context;
    private readonly IActivityService _activity;

    public WhatsAppTemplateService(ApplicationDbContext context, IActivityService activity)
    {
        _context = context;
        _activity = activity;
    }

    public List<WhatsAppTemplate> GetAll(bool onlyActive = false)
    {
        var q = _context.WhatsAppTemplates.AsQueryable();
        if (onlyActive) q = q.Where(t => t.Activa);
        return q.OrderBy(t => t.Nombre).ToList();
    }

    public WhatsAppTemplate? GetById(int id) => _context.WhatsAppTemplates.Find(id);

    public WhatsAppTemplate? GetDefault() => _context.WhatsAppTemplates.FirstOrDefault(t => t.Predeterminada);

    public WhatsAppTemplate Create(WhatsAppTemplate template)
    {
        template.Activa = true;
        if (template.Predeterminada)
            UnsetOtherDefault();
        _context.WhatsAppTemplates.Add(template);
        _context.SaveChanges();
        _activity.Record(SD.ActivityTypeTemplate, $"Plantilla WhatsApp creada: {template.Nombre}", template.Id.ToString(), null);
        return template;
    }

    public bool Update(WhatsAppTemplate template)
    {
        var existing = _context.WhatsAppTemplates.Find(template.Id);
        if (existing == null) return false;
        existing.Nombre = template.Nombre;
        existing.Mensaje = template.Mensaje;
        existing.Activa = template.Activa;
        if (template.Predeterminada && !existing.Predeterminada)
            UnsetOtherDefault();
        existing.Predeterminada = template.Predeterminada;
        _context.SaveChanges();
        _activity.Record(SD.ActivityTypeTemplate, $"Plantilla WhatsApp actualizada: {template.Nombre}", template.Id.ToString(), null);
        return true;
    }

    public bool Delete(int id)
    {
        var t = _context.WhatsAppTemplates.Find(id);
        if (t == null) return false;
        if (t.Predeterminada)
        {
            var anyOther = _context.WhatsAppTemplates.Any(x => x.Id != id);
            if (!anyOther) return false; // no permitir borrar la única predeterminada sin otra alternativa
        }
        var nombre = t.Nombre;
        _context.WhatsAppTemplates.Remove(t);
        _context.SaveChanges();
        _activity.Record(SD.ActivityTypeTemplate, $"Plantilla WhatsApp eliminada: {nombre}", id.ToString(), null);
        return true;
    }

    private void UnsetOtherDefault()
    {
        foreach (var x in _context.WhatsAppTemplates.Where(x => x.Predeterminada))
            x.Predeterminada = false;
    }
}

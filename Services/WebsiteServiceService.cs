using OptiControl.Data;
using OptiControl.Models.Entities;
using OptiControl.Services.IServices;

namespace OptiControl.Services;

public class WebsiteServiceService : IWebsiteServiceService
{
    private readonly ApplicationDbContext _context;

    public WebsiteServiceService(ApplicationDbContext context) => _context = context;

    public List<WebsiteService> GetAll() =>
        _context.WebsiteServices.OrderBy(s => s.SortOrder).ThenBy(s => s.Id).ToList();

    public List<WebsiteService> GetActiveForPublic() =>
        _context.WebsiteServices.Where(s => s.IsActive).OrderBy(s => s.SortOrder).ThenBy(s => s.Id).ToList();

    public WebsiteService? GetById(int id) => _context.WebsiteServices.Find(id);

    public WebsiteService Create(WebsiteService service)
    {
        service.CreatedAt = DateTime.UtcNow;
        service.UpdatedAt = DateTime.UtcNow;
        _context.WebsiteServices.Add(service);
        _context.SaveChanges();
        return service;
    }

    public bool Update(WebsiteService service)
    {
        var existing = _context.WebsiteServices.Find(service.Id);
        if (existing == null) return false;
        existing.Title = service.Title;
        existing.ShortDescription = service.ShortDescription;
        existing.Description = service.Description;
        existing.SortOrder = service.SortOrder;
        existing.IsActive = service.IsActive;
        existing.Icon = service.Icon;
        existing.UpdatedAt = DateTime.UtcNow;
        _context.SaveChanges();
        return true;
    }

    public bool Delete(int id)
    {
        var s = _context.WebsiteServices.Find(id);
        if (s == null) return false;
        _context.WebsiteServices.Remove(s);
        _context.SaveChanges();
        return true;
    }
}

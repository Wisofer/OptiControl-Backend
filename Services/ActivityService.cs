using OptiControl.Data;
using OptiControl.Models.Dtos;
using OptiControl.Models.Entities;
using OptiControl.Services.IServices;
using Microsoft.EntityFrameworkCore;
using OptiControl.Utils;

namespace OptiControl.Services;

public class ActivityService : IActivityService
{
    private readonly ApplicationDbContext _context;

    public ActivityService(ApplicationDbContext context)
    {
        _context = context;
    }

    public List<Activity> GetRecent(int limit = 20, DateTime? from = null)
    {
        var q = _context.Activities.OrderByDescending(a => a.Time);
        if (from.HasValue)
            q = (IOrderedQueryable<Activity>)q.Where(a => a.Time >= from.Value);
        return q.Take(limit).ToList();
    }

    public PagedResult<Activity> GetPaged(int page = 1, int pageSize = 20, DateTime? from = null, DateTime? to = null, string? type = null)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);
        page = Math.Max(1, page);
        var q = _context.Activities.AsQueryable();
        if (from.HasValue) q = q.Where(a => a.Time >= from.Value);
        if (to.HasValue) q = q.Where(a => a.Time <= to.Value);
        if (!string.IsNullOrWhiteSpace(type)) q = q.Where(a => a.Type == type);
        var totalCount = q.Count();
        var items = q.OrderByDescending(a => a.Time).Skip((page - 1) * pageSize).Take(pageSize).ToList();
        return PagedResult<Activity>.Create(items, totalCount, page, pageSize);
    }

    public List<Activity> GetByClientId(int clientId, int limit = 50)
    {
        return _context.Activities
            .Where(a => a.ClientId == clientId)
            .OrderByDescending(a => a.Time)
            .Take(limit)
            .ToList();
    }

    public void Record(string type, string description, string? entityId = null, int? clientId = null)
    {
        _context.Activities.Add(new Activity
        {
            Type = type,
            Description = description,
            Time = TimeZoneHelper.UtcNow(),
            EntityId = entityId,
            ClientId = clientId
        });
        _context.SaveChanges();
    }
}

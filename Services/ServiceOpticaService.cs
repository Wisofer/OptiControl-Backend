using OptiControl.Data;
using OptiControl.Models.Dtos;
using OptiControl.Models.Entities;
using OptiControl.Services.IServices;
using OptiControl.Utils;
using Microsoft.EntityFrameworkCore;

namespace OptiControl.Services;

public class ServiceOpticaService : IServiceOpticaService
{
    private readonly ApplicationDbContext _context;
    private readonly IActivityService _activity;

    public ServiceOpticaService(ApplicationDbContext context, IActivityService activity)
    {
        _context = context;
        _activity = activity;
    }

    private static ServiceOpticaResponseDto ToDto(ServiceOptica s)
    {
        return new ServiceOpticaResponseDto
        {
            Id = s.Id,
            NombreServicio = s.NombreServicio,
            Precio = s.Precio,
            Descripcion = s.Descripcion,
            FechaCreacion = s.FechaCreacion.ToString("yyyy-MM-dd")
        };
    }

    public PagedResult<ServiceOpticaResponseDto> GetPaged(int page = 1, int pageSize = 20, string? search = null)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);
        page = Math.Max(1, page);
        var q = _context.ServiceOpticas.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            q = q.Where(x =>
                (x.NombreServicio != null && x.NombreServicio.ToLower().Contains(s)) ||
                (x.Descripcion != null && x.Descripcion.ToLower().Contains(s)));
        }
        var totalCount = q.Count();
        var items = q.OrderBy(x => x.NombreServicio)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList()
            .Select(ToDto)
            .ToList();
        return PagedResult<ServiceOpticaResponseDto>.Create(items, totalCount, page, pageSize);
    }

    public ServiceOpticaResponseDto? GetById(int id)
    {
        var s = _context.ServiceOpticas.Find(id);
        return s == null ? null : ToDto(s);
    }

    public ServiceOptica? GetEntityById(int id) => _context.ServiceOpticas.Find(id);

    public ServiceOpticaResponseDto Create(ServiceOptica service)
    {
        if (service.FechaCreacion == default)
            service.FechaCreacion = DateTime.UtcNow.Date;
        _context.ServiceOpticas.Add(service);
        _context.SaveChanges();
        _activity.Record(SD.ActivityTypeService, $"Servicio agregado: {service.NombreServicio}", service.Id.ToString(), null);
        return ToDto(service);
    }

    public ServiceOpticaResponseDto? Update(int id, ServiceOptica service)
    {
        var existing = _context.ServiceOpticas.Find(id);
        if (existing == null) return null;
        existing.NombreServicio = service.NombreServicio;
        existing.Precio = service.Precio;
        existing.Descripcion = service.Descripcion;
        if (service.FechaCreacion != default)
            existing.FechaCreacion = service.FechaCreacion;
        _context.SaveChanges();
        _activity.Record(SD.ActivityTypeService, $"Servicio actualizado: {service.NombreServicio}", id.ToString(), null);
        return ToDto(existing);
    }

    public bool Delete(int id)
    {
        var s = _context.ServiceOpticas.Find(id);
        if (s == null) return false;
        var name = s.NombreServicio;
        _context.ServiceOpticas.Remove(s);
        _context.SaveChanges();
        _activity.Record(SD.ActivityTypeService, $"Servicio eliminado: {name}", id.ToString(), null);
        return true;
    }
}

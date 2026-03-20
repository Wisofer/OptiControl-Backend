using System.Globalization;
using OptiControl.Data;
using OptiControl.Models.Dtos;
using OptiControl.Models.Entities;
using OptiControl.Services.IServices;
using OptiControl.Utils;
using Microsoft.EntityFrameworkCore;

namespace OptiControl.Services;

public class ClientService : IClientService
{
    private readonly ApplicationDbContext _context;
    private readonly IActivityService _activity;
    private readonly IOpticsSaleService _opticsSaleService;
    private static readonly CultureInfo EsNi = new("es-NI");

    public ClientService(ApplicationDbContext context, IActivityService activity, IOpticsSaleService opticsSaleService)
    {
        _context = context;
        _activity = activity;
        _opticsSaleService = opticsSaleService;
    }

    private static ClientOpticsResponseDto ToOpticsDto(Client c)
    {
        return new ClientOpticsResponseDto
        {
            Id = c.Id,
            Name = c.Name ?? "",
            Phone = c.Phone,
            Address = c.Address,
            GraduacionOd = c.GraduacionOd,
            GraduacionOi = c.GraduacionOi,
            FechaRegistro = c.FechaRegistro?.ToString("yyyy-MM-dd", EsNi) ?? TimeZoneHelper.NicaraguaToday().ToString("yyyy-MM-dd", EsNi),
            Email = c.Email,
            Descripcion = c.Descripcion
        };
    }

    public PagedResult<Client> GetPaged(string? search = null, int page = 1, int pageSize = 20)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);
        page = Math.Max(1, page);
        var q = _context.Clients.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            q = q.Where(c =>
                (c.Name != null && c.Name.ToLower().Contains(s)) ||
                (c.Pasaporte != null && c.Pasaporte.ToLower().Contains(s)) ||
                (c.Email != null && c.Email.ToLower().Contains(s)) ||
                (c.Phone != null && c.Phone.ToLower().Contains(s)));
        }
        var totalCount = q.Count();
        var items = q.OrderBy(c => c.Name).Skip((page - 1) * pageSize).Take(pageSize).ToList();
        return PagedResult<Client>.Create(items, totalCount, page, pageSize);
    }

    public PagedResult<ClientOpticsResponseDto> GetPagedOptics(string? search = null, int page = 1, int pageSize = 20)
    {
        var paged = GetPaged(search, page, pageSize);
        var dtos = paged.Items.Select(ToOpticsDto).ToList();
        return PagedResult<ClientOpticsResponseDto>.Create(dtos, paged.TotalCount, paged.Page, paged.PageSize);
    }

    public Client? GetById(int id) => _context.Clients.Find(id);

    public ClientOpticsResponseDto? GetByIdOptics(int id)
    {
        var c = _context.Clients.Find(id);
        return c == null ? null : ToOpticsDto(c);
    }

    public Client Create(Client client)
    {
        client.Status = client.Status ?? SD.ClientStatusPendiente;
        client.LastTrip = null;
        _context.Clients.Add(client);
        _context.SaveChanges();
        _activity.Record(SD.ActivityTypeClient, $"Cliente creado: {client.Name}", null, client.Id);
        return client;
    }

    public bool Update(Client client)
    {
        var existing = _context.Clients.Find(client.Id);
        if (existing == null) return false;
        existing.Pasaporte = client.Pasaporte;
        existing.Name = client.Name;
        existing.Email = client.Email;
        existing.Phone = client.Phone;
        existing.Status = client.Status;
        existing.LastTrip = client.LastTrip;
        _context.SaveChanges();
        return true;
    }

    public bool Delete(int id)
    {
        var c = _context.Clients.Find(id);
        if (c == null) return false;
        _context.Clients.Remove(c);
        _context.SaveChanges();
        return true;
    }

    public ClientHistoryDto? GetHistory(int clientId, int page = 1, int pageSize = 10)
    {
        var client = _context.Clients.Find(clientId);
        if (client == null) return null;

        pageSize = Math.Clamp(pageSize, 1, 100);
        page = Math.Max(1, page);

        var resQuery = _context.Reservations
            .Where(r => r.ClientId == clientId)
            .OrderByDescending(r => r.StartDate);
        var resTotal = resQuery.Count();
        var reservations = resQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new ReservationSummaryDto
            {
                Id = r.Id,
                Destination = r.Destination,
                StartDate = r.StartDate,
                EndDate = r.EndDate,
                Amount = r.Amount,
                PaymentStatus = r.PaymentStatus ?? "",
                PaymentMethod = r.PaymentMethod
            }).ToList();

        var invQuery = _context.Invoices
            .Where(i => i.ClientId == clientId)
            .OrderByDescending(i => i.Date);
        var invTotal = invQuery.Count();
        var invoices = invQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(i => new InvoiceSummaryDto
            {
                Id = i.Id,
                Date = i.Date,
                DueDate = i.DueDate,
                TravelDate = i.TravelDate,
                ReturnDate = i.ReturnDate,
                Amount = i.Amount,
                Status = i.Status ?? "",
                Concept = i.Concept,
                PaymentMethod = i.PaymentMethod
            }).ToList();

        var activity = _activity.GetByClientId(clientId, 50);

        return new ClientHistoryDto
        {
            Client = client,
            Reservations = PagedResult<ReservationSummaryDto>.Create(reservations, resTotal, page, pageSize),
            Sales = PagedResult<SaleSummaryDto>.Create(new List<SaleSummaryDto>(), 0, page, pageSize),
            Invoices = PagedResult<InvoiceSummaryDto>.Create(invoices, invTotal, page, pageSize),
            Activity = activity
        };
    }

    public ClientHistoryOpticsDto? GetHistoryOptics(int clientId, int page = 1, int pageSize = 10)
    {
        var client = _context.Clients.Find(clientId);
        if (client == null) return null;
        var salesPaged = _opticsSaleService.GetSalesByClientId(clientId, page, pageSize);
        var activityList = _activity.GetByClientId(clientId, 50).Select(a => new ActivityItemDto
        {
            Id = "h-" + a.Id,
            Type = a.Type,
            Description = a.Description,
            Time = a.Time.ToString("yyyy-MM-dd HH:mm", EsNi)
        }).ToList();
        var emptyRes = PagedResult<object>.Create(new List<object>(), 0, page, pageSize);
        var emptyInv = PagedResult<object>.Create(new List<object>(), 0, page, pageSize);
        return new ClientHistoryOpticsDto
        {
            Client = ToOpticsDto(client),
            Reservations = new { items = Array.Empty<object>(), totalCount = 0, totalPages = 1, page, pageSize },
            Sales = salesPaged,
            Invoices = new { items = Array.Empty<object>(), totalCount = 0, totalPages = 1, page, pageSize },
            Activity = activityList
        };
    }
}

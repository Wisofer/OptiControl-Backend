using OptiControl.Data;
using OptiControl.Models.Dtos;
using OptiControl.Models.Entities;
using OptiControl.Services.IServices;
using OptiControl.Utils;
using Microsoft.EntityFrameworkCore;

namespace OptiControl.Services;

public class ReservationService : IReservationService
{
    private readonly ApplicationDbContext _context;
    private readonly IActivityService _activity;
    private readonly IClientService _clientService;

    public ReservationService(ApplicationDbContext context, IActivityService activity, IClientService clientService)
    {
        _context = context;
        _activity = activity;
        _clientService = clientService;
    }

    public PagedResult<Reservation> GetPaged(int? clientId = null, string? paymentStatus = null, string? paymentMethod = null, DateTime? dateFrom = null, DateTime? dateTo = null, int page = 1, int pageSize = 20)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);
        page = Math.Max(1, page);
        var q = _context.Reservations.Include(r => r.Client).AsQueryable();
        if (clientId.HasValue) q = q.Where(r => r.ClientId == clientId.Value);
        if (!string.IsNullOrWhiteSpace(paymentStatus)) q = q.Where(r => r.PaymentStatus == paymentStatus);
        if (!string.IsNullOrWhiteSpace(paymentMethod)) q = q.Where(r => r.PaymentMethod == paymentMethod);
        if (dateFrom.HasValue) q = q.Where(r => r.StartDate >= dateFrom.Value);
        if (dateTo.HasValue) q = q.Where(r => r.EndDate <= dateTo.Value);
        var totalCount = q.Count();
        var items = q.OrderByDescending(r => r.StartDate).Skip((page - 1) * pageSize).Take(pageSize).ToList();
        return PagedResult<Reservation>.Create(items, totalCount, page, pageSize);
    }
    public Reservation? GetById(int id) => _context.Reservations.Include(r => r.Client).FirstOrDefault(r => r.Id == id);

    public Reservation Create(Reservation reservation)
    {
        reservation.PaymentStatus = reservation.PaymentStatus ?? SD.PaymentStatusPendiente;
        _context.Reservations.Add(reservation);
        _context.SaveChanges();
        var client = _context.Clients.Find(reservation.ClientId);
        _activity.Record(SD.ActivityTypeReservation, $"Nueva reservación: {client?.Name} - {reservation.Destination}", reservation.Id.ToString(), reservation.ClientId);
        UpdateClientLastTrip(reservation.ClientId, reservation.EndDate);
        return reservation;
    }

    public bool Update(Reservation reservation)
    {
        var existing = _context.Reservations.Find(reservation.Id);
        if (existing == null) return false;
        existing.ClientId = reservation.ClientId;
        existing.Destination = reservation.Destination;
        existing.StartDate = reservation.StartDate;
        existing.EndDate = reservation.EndDate;
        existing.Amount = reservation.Amount;
        existing.PaymentStatus = reservation.PaymentStatus;
        existing.PaymentMethod = reservation.PaymentMethod;
        _context.SaveChanges();
        UpdateClientLastTrip(reservation.ClientId, reservation.EndDate);
        return true;
    }

    public bool Delete(int id)
    {
        var r = _context.Reservations.Find(id);
        if (r == null) return false;
        _context.Reservations.Remove(r);
        _context.SaveChanges();
        return true;
    }

    void UpdateClientLastTrip(int clientId, DateTime endDate)
    {
        var client = _context.Clients.Find(clientId);
        if (client == null) return;
        if (client.LastTrip == null || client.LastTrip < endDate)
        {
            client.LastTrip = endDate;
            client.Status = SD.ClientStatusViajo;
            _context.SaveChanges();
        }
    }
}

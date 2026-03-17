using OptiControl.Models.Dtos;
using OptiControl.Models.Entities;

namespace OptiControl.Services.IServices;

public interface IReservationService
{
    PagedResult<Reservation> GetPaged(int? clientId = null, string? paymentStatus = null, string? paymentMethod = null, DateTime? dateFrom = null, DateTime? dateTo = null, int page = 1, int pageSize = 20);
    Reservation? GetById(int id);
    Reservation Create(Reservation reservation);
    bool Update(Reservation reservation);
    bool Delete(int id);
}

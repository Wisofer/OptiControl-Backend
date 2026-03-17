using OptiControl.Models.Dtos;
using OptiControl.Models.Entities;

namespace OptiControl.Services.IServices;

public interface ISaleService
{
    (PagedResult<Sale> Paged, decimal TotalAmountInCordobas, decimal TotalPendingInCordobas) GetPaged(int? clientId = null, string? status = null, string? paymentMethod = null, DateTime? dateFrom = null, DateTime? dateTo = null, int page = 1, int pageSize = 20);
    Sale? GetById(int id);
    Sale Create(Sale sale);
    bool Update(Sale sale);
    bool Delete(int id);
}

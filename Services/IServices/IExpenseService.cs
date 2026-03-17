using OptiControl.Models.Dtos;
using OptiControl.Models.Entities;

namespace OptiControl.Services.IServices;

public interface IExpenseService
{
    PagedResult<Expense> GetPaged(DateTime? dateFrom = null, DateTime? dateTo = null, string? category = null, int page = 1, int pageSize = 20);
    Expense? GetById(int id);
    Expense Create(Expense expense);
    bool Update(Expense expense);
    bool Delete(int id);
}

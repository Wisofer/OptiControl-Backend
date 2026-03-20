using OptiControl.Data;
using OptiControl.Models.Dtos;
using OptiControl.Models.Entities;
using OptiControl.Services.IServices;
using OptiControl.Utils;
using Microsoft.EntityFrameworkCore;

namespace OptiControl.Services;

public class ExpenseService : IExpenseService
{
    private readonly ApplicationDbContext _context;
    private readonly IActivityService _activity;

    public ExpenseService(ApplicationDbContext context, IActivityService activity)
    {
        _context = context;
        _activity = activity;
    }

    public PagedResult<Expense> GetPaged(DateTime? dateFrom = null, DateTime? dateTo = null, string? category = null, int page = 1, int pageSize = 20)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);
        page = Math.Max(1, page);
        var q = _context.Expenses.AsQueryable();
        if (dateFrom.HasValue) q = q.Where(e => e.Date >= dateFrom.Value);
        if (dateTo.HasValue) q = q.Where(e => e.Date <= dateTo.Value);
        if (!string.IsNullOrWhiteSpace(category)) q = q.Where(e => e.Category == category);
        var totalCount = q.Count();
        var items = q.OrderByDescending(e => e.Date).Skip((page - 1) * pageSize).Take(pageSize).ToList();
        return PagedResult<Expense>.Create(items, totalCount, page, pageSize);
    }
    public Expense? GetById(int id) => _context.Expenses.Find(id);

    public Expense Create(Expense expense)
    {
        expense.Category = expense.Category ?? SD.ExpenseCategoryOperativo;
        expense.Date = expense.Date == default ? TimeZoneHelper.NicaraguaToday() : expense.Date;
        _context.Expenses.Add(expense);
        _context.SaveChanges();
        _activity.Record(SD.ActivityTypeExpense, $"Egreso: {expense.Concept} - {expense.Amount:N2}", expense.Id.ToString(), null);
        return expense;
    }

    public bool Update(Expense expense)
    {
        var existing = _context.Expenses.Find(expense.Id);
        if (existing == null) return false;
        existing.Date = expense.Date;
        existing.Concept = expense.Concept;
        existing.Amount = expense.Amount;
        existing.Category = expense.Category;
        _context.SaveChanges();
        _activity.Record(SD.ActivityTypeExpense, $"Egreso actualizado: {expense.Concept} - {expense.Amount:N2}", expense.Id.ToString(), null);
        return true;
    }

    public bool Delete(int id)
    {
        var e = _context.Expenses.Find(id);
        if (e == null) return false;
        var desc = $"Egreso eliminado: {e.Concept} - {e.Amount:N2}";
        _context.Expenses.Remove(e);
        _context.SaveChanges();
        _activity.Record(SD.ActivityTypeExpense, desc, id.ToString(), null);
        return true;
    }
}

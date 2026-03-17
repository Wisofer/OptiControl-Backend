using OptiControl.Data;
using OptiControl.Models.Entities;
using OptiControl.Services.IServices;
using Microsoft.EntityFrameworkCore;

namespace OptiControl.Services;

public class CajaService : ICajaService
{
    private readonly ApplicationDbContext _context;

    public CajaService(ApplicationDbContext context)
    {
        _context = context;
    }

    public List<CajaDiaria> GetByRange(DateTime dateFrom, DateTime dateTo)
    {
        return _context.CajaDiaria
            .Where(c => c.Date >= dateFrom.Date && c.Date <= dateTo.Date)
            .OrderByDescending(c => c.Date)
            .ToList();
    }

    public CajaDiaria? GetByDate(DateTime date)
    {
        var d = date.Date;
        return _context.CajaDiaria.FirstOrDefault(c => c.Date == d);
    }

    public CajaDiaria CreateOrUpdate(CajaDiaria caja)
    {
        caja.Date = caja.Date.Date;
        var existing = _context.CajaDiaria.FirstOrDefault(c => c.Date == caja.Date);
        if (existing != null)
        {
            existing.Opening = caja.Opening;
            existing.Sales = caja.Sales;
            existing.Expenses = caja.Expenses;
            existing.Closing = caja.Closing;
            _context.SaveChanges();
            return existing;
        }
        _context.CajaDiaria.Add(caja);
        _context.SaveChanges();
        return caja;
    }
}

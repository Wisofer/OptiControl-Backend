using OptiControl.Models.Entities;

namespace OptiControl.Services.IServices;

public interface ICajaService
{
    List<CajaDiaria> GetByRange(DateTime dateFrom, DateTime dateTo);
    CajaDiaria? GetByDate(DateTime date);
    CajaDiaria CreateOrUpdate(CajaDiaria caja);
}

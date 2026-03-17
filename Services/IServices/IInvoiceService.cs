using OptiControl.Models.Dtos;
using OptiControl.Models.Entities;

namespace OptiControl.Services.IServices;

public interface IInvoiceService
{
    PagedResult<Invoice> GetPaged(int? clientId = null, string? status = null, string? paymentMethod = null, DateTime? dateFrom = null, DateTime? dateTo = null, int page = 1, int pageSize = 20);
    Invoice? GetById(string id);
    /// <summary>Marca vencidas y devuelve la lista de facturas vencidas para alertas (solo si AlertsFacturasVencidas está activo en front).</summary>
    List<OverdueInvoiceAlertDto> GetOverdueForAlerts();
    Invoice Create(Invoice invoice);
    bool Update(Invoice invoice);
    bool Delete(string id);
    string GetNextInvoiceCode();
}

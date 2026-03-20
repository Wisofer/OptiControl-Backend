using OptiControl.Models.Dtos;

namespace OptiControl.Services.IServices;

public interface IOpticsSaleService
{
    SaleResponseDto? CreateSale(CreateSaleRequestDto dto);
    PagedResult<SaleResponseDto> GetSalesHistoryPaged(int page = 1, int pageSize = 20);
    PagedResult<SaleResponseDto> GetSalesByClientId(int clientId, int page = 1, int pageSize = 10);
    SaleResponseDto? GetSaleById(int id);
    (bool Success, string? Error) CancelSale(int id);
    (bool Success, string? Error) AddPayment(int id, decimal addPayment, string? paymentType = null, string? bank = null);
}

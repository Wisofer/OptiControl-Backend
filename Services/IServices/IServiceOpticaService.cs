using OptiControl.Models.Dtos;
using OptiControl.Models.Entities;

namespace OptiControl.Services.IServices;

public interface IServiceOpticaService
{
    PagedResult<ServiceOpticaResponseDto> GetPaged(int page = 1, int pageSize = 20, string? search = null);
    ServiceOpticaResponseDto? GetById(int id);
    ServiceOpticaResponseDto Create(ServiceOptica service);
    ServiceOpticaResponseDto? Update(int id, ServiceOptica service);
    bool Delete(int id);
    ServiceOptica? GetEntityById(int id);
}

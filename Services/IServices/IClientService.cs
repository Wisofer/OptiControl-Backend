using OptiControl.Models.Dtos;
using OptiControl.Models.Entities;

namespace OptiControl.Services.IServices;

public interface IClientService
{
    PagedResult<Client> GetPaged(string? search = null, int page = 1, int pageSize = 20);
    PagedResult<ClientOpticsResponseDto> GetPagedOptics(string? search = null, int page = 1, int pageSize = 20);
    Client? GetById(int id);
    ClientOpticsResponseDto? GetByIdOptics(int id);
    Client Create(Client client);
    bool Update(Client client);
    bool Delete(int id);
    ClientHistoryDto? GetHistory(int clientId, int page = 1, int pageSize = 10);
    ClientHistoryOpticsDto? GetHistoryOptics(int clientId, int page = 1, int pageSize = 10);
}

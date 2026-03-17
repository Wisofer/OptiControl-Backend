using OptiControl.Models.Dtos;
using OptiControl.Models.Entities;

namespace OptiControl.Services.IServices;

public interface IActivityService
{
    List<Activity> GetRecent(int limit = 20, DateTime? from = null);
    PagedResult<Activity> GetPaged(int page = 1, int pageSize = 20, DateTime? from = null, DateTime? to = null, string? type = null);
    List<Activity> GetByClientId(int clientId, int limit = 50);
    void Record(string type, string description, string? entityId = null, int? clientId = null);
}

using OptiControl.Models.Entities;

namespace OptiControl.Services.IServices;

public interface IWebsiteServiceService
{
    List<WebsiteService> GetAll();
    List<WebsiteService> GetActiveForPublic();
    WebsiteService? GetById(int id);
    WebsiteService Create(WebsiteService service);
    bool Update(WebsiteService service);
    bool Delete(int id);
}

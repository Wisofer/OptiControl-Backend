using OptiControl.Models.Dtos;
using OptiControl.Models.Entities;

namespace OptiControl.Services.IServices;

public interface IProductService
{
    PagedResult<ProductResponseDto> GetPaged(int page = 1, int pageSize = 20, string? search = null);
    ProductResponseDto? GetById(int id);
    ProductResponseDto Create(Product product);
    ProductResponseDto? Update(int id, Product product);
    bool Delete(int id);
    Product? GetEntityById(int id);
    /// <summary>Productos con stock actual por debajo del stock mínimo.</summary>
    List<ProductResponseDto> GetLowStock();
}

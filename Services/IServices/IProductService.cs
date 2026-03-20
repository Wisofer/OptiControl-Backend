using OptiControl.Models.Dtos;
using OptiControl.Models.Entities;

namespace OptiControl.Services.IServices;

public interface IProductService
{
    PagedResult<ProductResponseDto> GetPaged(int page = 1, int pageSize = 20, string? search = null);
    ProductResponseDto? GetById(int id);
    ProductResponseDto Create(Product product);
    ProductResponseDto? Update(int id, Product product);
    /// <summary>Suma cantidad al stock (reabastecimiento rápido).</summary>
    ProductResponseDto? Restock(int id, int cantidad);
    bool Delete(int id);
    Product? GetEntityById(int id);
    /// <summary>Productos con stock en o por debajo del stock mínimo (alertas).</summary>
    List<ProductResponseDto> GetLowStock();
}

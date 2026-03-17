using OptiControl.Data;
using OptiControl.Models.Dtos;
using OptiControl.Models.Entities;
using OptiControl.Services.IServices;
using OptiControl.Utils;
using Microsoft.EntityFrameworkCore;

namespace OptiControl.Services;

public class ProductService : IProductService
{
    private readonly ApplicationDbContext _context;
    private readonly IActivityService _activity;

    public ProductService(ApplicationDbContext context, IActivityService activity)
    {
        _context = context;
        _activity = activity;
    }

    private static ProductResponseDto ToDto(Product p)
    {
        return new ProductResponseDto
        {
            Id = p.Id,
            NombreProducto = p.NombreProducto,
            TipoProducto = p.TipoProducto,
            Marca = p.Marca,
            PrecioCompra = p.PrecioCompra,
            Precio = p.Precio,
            Stock = p.Stock,
            StockMinimo = p.StockMinimo,
            FechaCreacion = p.FechaCreacion.ToString("yyyy-MM-dd"),
            Descripcion = p.Descripcion,
            Proveedor = p.Proveedor
        };
    }

    public PagedResult<ProductResponseDto> GetPaged(int page = 1, int pageSize = 20, string? search = null)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);
        page = Math.Max(1, page);
        var q = _context.Products.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            q = q.Where(p =>
                (p.NombreProducto != null && p.NombreProducto.ToLower().Contains(s)) ||
                (p.Marca != null && p.Marca.ToLower().Contains(s)) ||
                (p.Descripcion != null && p.Descripcion.ToLower().Contains(s)) ||
                (p.Proveedor != null && p.Proveedor.ToLower().Contains(s)));
        }
        var totalCount = q.Count();
        var items = q.OrderBy(p => p.NombreProducto)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList()
            .Select(ToDto)
            .ToList();
        return PagedResult<ProductResponseDto>.Create(items, totalCount, page, pageSize);
    }

    public ProductResponseDto? GetById(int id)
    {
        var p = _context.Products.Find(id);
        return p == null ? null : ToDto(p);
    }

    public Product? GetEntityById(int id) => _context.Products.Find(id);

    public ProductResponseDto Create(Product product)
    {
        if (product.FechaCreacion == default)
            product.FechaCreacion = DateTime.UtcNow.Date;
        _context.Products.Add(product);
        _context.SaveChanges();
        _activity.Record(SD.ActivityTypeProduct, $"Producto agregado: {product.NombreProducto}", product.Id.ToString(), null);
        return ToDto(product);
    }

    public ProductResponseDto? Update(int id, Product product)
    {
        var existing = _context.Products.Find(id);
        if (existing == null) return null;
        existing.NombreProducto = product.NombreProducto;
        existing.TipoProducto = product.TipoProducto;
        existing.Marca = product.Marca;
        existing.PrecioCompra = product.PrecioCompra;
        existing.Precio = product.Precio;
        existing.Stock = product.Stock;
        existing.StockMinimo = product.StockMinimo;
        existing.Descripcion = product.Descripcion;
        existing.Proveedor = product.Proveedor;
        if (product.FechaCreacion != default)
            existing.FechaCreacion = product.FechaCreacion;
        _context.SaveChanges();
        _activity.Record(SD.ActivityTypeInventory, $"Producto actualizado: {product.NombreProducto}", id.ToString(), null);
        return ToDto(existing);
    }

    public bool Delete(int id)
    {
        var p = _context.Products.Find(id);
        if (p == null) return false;
        var name = p.NombreProducto;
        _context.Products.Remove(p);
        _context.SaveChanges();
        _activity.Record(SD.ActivityTypeProduct, $"Producto eliminado: {name}", id.ToString(), null);
        return true;
    }

    public List<ProductResponseDto> GetLowStock()
    {
        return _context.Products
            .Where(p => p.Stock < p.StockMinimo)
            .OrderBy(p => p.Stock)
            .ToList()
            .Select(ToDto)
            .ToList();
    }
}

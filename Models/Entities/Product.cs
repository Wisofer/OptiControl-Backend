namespace OptiControl.Models.Entities;

/// <summary>Producto de inventario (óptica): monturas, lentes, accesorios.</summary>
public class Product
{
    public int Id { get; set; }
    public string NombreProducto { get; set; } = string.Empty;
    public string TipoProducto { get; set; } = string.Empty; // montura, lente, accesorio
    public string? Marca { get; set; }
    /// <summary>Precio de compra (costo) al proveedor.</summary>
    public decimal PrecioCompra { get; set; }
    /// <summary>Precio de venta al cliente.</summary>
    public decimal Precio { get; set; }
    public int Stock { get; set; }
    /// <summary>Stock mínimo recomendado; si Stock &lt; StockMinimo se puede alertar.</summary>
    public int StockMinimo { get; set; }
    public DateTime FechaCreacion { get; set; }
    /// <summary>Descripción o especificaciones del producto.</summary>
    public string? Descripcion { get; set; }
    /// <summary>Nombre del proveedor (campo de texto libre).</summary>
    public string? Proveedor { get; set; }
}

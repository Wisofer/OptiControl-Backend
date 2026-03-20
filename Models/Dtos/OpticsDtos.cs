using System.Text.Json.Serialization;

namespace OptiControl.Models.Dtos;

/// <summary>Respuesta de producto según spec OptiControl (snake_case).</summary>
public class ProductResponseDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("nombre_producto")]
    public string NombreProducto { get; set; } = "";
    [JsonPropertyName("tipo_producto")]
    public string TipoProducto { get; set; } = "";
    [JsonPropertyName("marca")]
    public string? Marca { get; set; }
    [JsonPropertyName("precio_compra")]
    public decimal PrecioCompra { get; set; }
    [JsonPropertyName("precio")]
    public decimal Precio { get; set; }
    [JsonPropertyName("stock")]
    public int Stock { get; set; }
    [JsonPropertyName("stock_minimo")]
    public int StockMinimo { get; set; }
    /// <summary>True si el stock actual está en o por debajo del mínimo (para resaltar en UI / alertas).</summary>
    [JsonPropertyName("stock_bajo")]
    public bool StockBajo { get; set; }
    [JsonPropertyName("fecha_creacion")]
    public string FechaCreacion { get; set; } = "";
    [JsonPropertyName("descripcion")]
    public string? Descripcion { get; set; }
    [JsonPropertyName("proveedor")]
    public string? Proveedor { get; set; }
}

/// <summary>Entrada para reponer inventario sin editar el producto completo.</summary>
public class RestockProductRequestDto
{
    /// <summary>Cantidad positiva a sumar al stock actual.</summary>
    [JsonPropertyName("cantidad")]
    public int Cantidad { get; set; }
}

/// <summary>Respuesta de servicio óptica según spec (snake_case).</summary>
public class ServiceOpticaResponseDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("nombre_servicio")]
    public string NombreServicio { get; set; } = "";
    [JsonPropertyName("precio")]
    public decimal Precio { get; set; }
    [JsonPropertyName("descripcion")]
    public string? Descripcion { get; set; }
    [JsonPropertyName("fecha_creacion")]
    public string FechaCreacion { get; set; } = "";
}

/// <summary>Cliente para listado OptiControl (snake_case donde aplica).</summary>
public class ClientOpticsResponseDto
{
    [JsonPropertyName("id")]
    public object Id { get; set; } = 0; // string o number
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";
    [JsonPropertyName("phone")]
    public string? Phone { get; set; }
    [JsonPropertyName("address")]
    public string? Address { get; set; }
    [JsonPropertyName("graduacion_od")]
    public string? GraduacionOd { get; set; }
    [JsonPropertyName("graduacion_oi")]
    public string? GraduacionOi { get; set; }
    [JsonPropertyName("fecha_registro")]
    public string FechaRegistro { get; set; } = "";
    [JsonPropertyName("email")]
    public string? Email { get; set; }
    [JsonPropertyName("descripcion")]
    public string? Descripcion { get; set; }
}

/// <summary>Item de venta en request/response.</summary>
public class SaleItemDto
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }
    [JsonPropertyName("productId")]
    public int? ProductId { get; set; }
    [JsonPropertyName("serviceId")]
    public int? ServiceId { get; set; }
    [JsonPropertyName("productName")]
    public string? ProductName { get; set; }
    [JsonPropertyName("serviceName")]
    public string? ServiceName { get; set; }
    [JsonPropertyName("quantity")]
    public int Quantity { get; set; }
    [JsonPropertyName("unitPrice")]
    public decimal UnitPrice { get; set; }
    [JsonPropertyName("subtotal")]
    public decimal Subtotal { get; set; }
}

/// <summary>Request POST /api/sales (venta o cotización).</summary>
public class CreateSaleRequestDto
{
    [JsonPropertyName("clientId")]
    public string? ClientId { get; set; }
    [JsonPropertyName("clientName")]
    public string? ClientName { get; set; }
    [JsonPropertyName("items")]
    public List<SaleItemDto> Items { get; set; } = new();
    [JsonPropertyName("total")]
    public decimal Total { get; set; }
    [JsonPropertyName("amountPaid")]
    public decimal AmountPaid { get; set; }
    [JsonPropertyName("paymentMethod")]
    public string? PaymentMethod { get; set; }
    [JsonPropertyName("currency")]
    public string? Currency { get; set; }
    [JsonPropertyName("status")]
    public string? Status { get; set; } // "cotizacion" para cotización
}

/// <summary>Respuesta venta (detalle / creada).</summary>
public class SaleResponseDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";
    [JsonPropertyName("date")]
    public string Date { get; set; } = "";
    [JsonPropertyName("clientId")]
    public string? ClientId { get; set; }
    [JsonPropertyName("clientName")]
    public string? ClientName { get; set; }
    [JsonPropertyName("items")]
    public List<SaleItemDto> Items { get; set; } = new();
    [JsonPropertyName("total")]
    public decimal Total { get; set; }
    [JsonPropertyName("amountPaid")]
    public decimal AmountPaid { get; set; }
    [JsonPropertyName("paymentMethod")]
    public string? PaymentMethod { get; set; }
    [JsonPropertyName("currency")]
    public string? Currency { get; set; }
    [JsonPropertyName("status")]
    public string Status { get; set; } = "";
    [JsonPropertyName("invoiceId")]
    public string? InvoiceId { get; set; }
    [JsonPropertyName("invoicePdfUrl")]
    public string? InvoicePdfUrl { get; set; }
    [JsonPropertyName("invoicePublicPdfUrl")]
    public string? InvoicePublicPdfUrl { get; set; }
    [JsonPropertyName("saleTicketPdfUrl")]
    public string? SaleTicketPdfUrl { get; set; }
    [JsonPropertyName("exchangeRate")]
    public decimal? ExchangeRate { get; set; }
    [JsonPropertyName("totalNio")]
    public decimal? TotalNio { get; set; }
    [JsonPropertyName("totalUsd")]
    public decimal? TotalUsd { get; set; }
    [JsonPropertyName("amountPaidNio")]
    public decimal? AmountPaidNio { get; set; }
    [JsonPropertyName("amountPaidUsd")]
    public decimal? AmountPaidUsd { get; set; }
    [JsonPropertyName("changeDue")]
    public decimal? ChangeDue { get; set; }
    [JsonPropertyName("changeCurrency")]
    public string? ChangeCurrency { get; set; }
    [JsonPropertyName("paymentHistory")]
    public List<SalePaymentDto> PaymentHistory { get; set; } = new();
}

public class SalePaymentDto
{
    [JsonPropertyName("date")]
    public string Date { get; set; } = "";
    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }
    [JsonPropertyName("paymentType")]
    public string? PaymentType { get; set; }
    [JsonPropertyName("bank")]
    public string? Bank { get; set; }
}

/// <summary>Actividad reciente para dashboard (spec).</summary>
public class ActivityItemDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";
    [JsonPropertyName("type")]
    public string Type { get; set; } = "";
    [JsonPropertyName("description")]
    public string Description { get; set; } = "";
    [JsonPropertyName("time")]
    public string Time { get; set; } = "";
}

/// <summary>Dashboard summary (spec).</summary>
public class DashboardSummaryDto
{
    [JsonPropertyName("totalRevenue")]
    public decimal TotalRevenue { get; set; }
    [JsonPropertyName("salesToday")]
    public decimal SalesToday { get; set; }
    [JsonPropertyName("salesMonth")]
    public decimal SalesMonth { get; set; }
    [JsonPropertyName("productsCount")]
    public int ProductsCount { get; set; }
    [JsonPropertyName("clientsCount")]
    public int ClientsCount { get; set; }
    [JsonPropertyName("productsTotal")]
    public int ProductsTotal { get; set; }
}

/// <summary>Monthly income item (spec).</summary>
public class MonthlyIncomeDto
{
    [JsonPropertyName("month")]
    public string Month { get; set; } = "";
    [JsonPropertyName("monthName")]
    public string MonthName { get; set; } = "";
    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }
}

/// <summary>Top product/service (spec).</summary>
public class TopProductDto
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";
    [JsonPropertyName("quantity")]
    public int Quantity { get; set; }
}

/// <summary>Historial del cliente (spec OptiControl).</summary>
public class ClientHistoryOpticsDto
{
    [JsonPropertyName("client")]
    public ClientOpticsResponseDto Client { get; set; } = null!;
    [JsonPropertyName("reservations")]
    public object Reservations { get; set; } = null!; // { items, totalCount, totalPages, page, pageSize }
    [JsonPropertyName("sales")]
    public object Sales { get; set; } = null!; // PagedResult<SaleResponseDto>
    [JsonPropertyName("invoices")]
    public object Invoices { get; set; } = null!;
    [JsonPropertyName("activity")]
    public List<ActivityItemDto> Activity { get; set; } = new();
}

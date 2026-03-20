using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OptiControl.Models.Dtos;
using OptiControl.Services.IServices;

namespace OptiControl.Controllers.Api;

/// <summary>API reducida para app móvil (Flutter). Mismos datos, payloads más ligeros.</summary>
[ApiController]
[Route("api/mobile")]
[Authorize]
public class MobileController : ControllerBase
{
    private readonly IDashboardOpticsService _dashboard;
    private readonly IProductService _productService;
    private readonly IServiceOpticaService _serviceOpticaService;
    private readonly IClientService _clientService;
    private readonly IOpticsSaleService _opticsSaleService;
    private readonly ISettingsService _settingsService;

    public MobileController(
        IDashboardOpticsService dashboard,
        IProductService productService,
        IServiceOpticaService serviceOpticaService,
        IClientService clientService,
        IOpticsSaleService opticsSaleService,
        ISettingsService settingsService)
    {
        _dashboard = dashboard;
        _productService = productService;
        _serviceOpticaService = serviceOpticaService;
        _clientService = clientService;
        _opticsSaleService = opticsSaleService;
        _settingsService = settingsService;
    }

    /// <summary>Resumen para home (una sola llamada).</summary>
    [HttpGet("summary")]
    public IActionResult GetSummary()
    {
        var s = _dashboard.GetSummary();
        return Ok(new MobileSummaryDto
        {
            TotalRevenue = s.TotalRevenue,
            SalesToday = s.SalesToday,
            SalesMonth = s.SalesMonth,
            ProductsCount = s.ProductsCount,
            ClientsCount = s.ClientsCount
        });
    }

    /// <summary>Productos lite (id, name, price, stock, type) para POS/búsqueda.</summary>
    [HttpGet("products")]
    public IActionResult GetProducts([FromQuery] int page = 1, [FromQuery] int pageSize = 50, [FromQuery] string? search = null)
    {
        var paged = _productService.GetPaged(page, pageSize, search);
        var items = paged.Items.Select(p => new MobileProductDto
        {
            Id = p.Id,
            Name = p.NombreProducto,
            Price = p.Precio,
            Stock = p.Stock,
            StockMinimo = p.StockMinimo,
            StockBajo = p.StockBajo,
            Type = p.TipoProducto
        }).ToList();
        return Ok(new { items, totalCount = paged.TotalCount, page = paged.Page, pageSize = paged.PageSize });
    }

    /// <summary>Servicios lite (id, name, price) para POS.</summary>
    [HttpGet("services")]
    public IActionResult GetServices([FromQuery] int page = 1, [FromQuery] int pageSize = 50, [FromQuery] string? search = null)
    {
        var paged = _serviceOpticaService.GetPaged(page, pageSize, search);
        var items = paged.Items.Select(s => new MobileServiceDto
        {
            Id = s.Id,
            Name = s.NombreServicio,
            Price = s.Precio
        }).ToList();
        return Ok(new { items, totalCount = paged.TotalCount, page = paged.Page, pageSize = paged.PageSize });
    }

    /// <summary>Clientes lite (id, name, phone) para selector en ventas.</summary>
    [HttpGet("clients")]
    public IActionResult GetClients([FromQuery] int page = 1, [FromQuery] int pageSize = 50, [FromQuery] string? search = null)
    {
        var paged = _clientService.GetPagedOptics(search, page, pageSize);
        var items = paged.Items.Select(c => new MobileClientDto
        {
            Id = c.Id is int i ? i : 0,
            Name = c.Name ?? "",
            Phone = c.Phone
        }).ToList();
        return Ok(new { items, totalCount = paged.TotalCount, page = paged.Page, pageSize = paged.PageSize });
    }

    /// <summary>Registrar venta o cotización (mismo body que POST /api/sales).</summary>
    [HttpPost("sales")]
    public IActionResult CreateSale([FromBody] CreateSaleRequestDto dto)
    {
        if (dto?.Items == null || dto.Items.Count == 0)
            return BadRequest(new { error = "items es requerido." });
        var result = _opticsSaleService.CreateSale(dto);
        if (result == null)
            return BadRequest(new { error = "Stock insuficiente." });
        return Ok(result);
    }

    /// <summary>Últimas ventas (lista reducida) para historial rápido.</summary>
    [HttpGet("sales/recent")]
    public IActionResult GetRecentSales([FromQuery] int limit = 20)
    {
        limit = Math.Clamp(limit, 1, 100);
        var paged = _opticsSaleService.GetSalesHistoryPaged(1, limit);
        var items = paged.Items.Select(s => new MobileSaleDto
        {
            Id = s.Id,
            Date = s.Date,
            ClientName = s.ClientName,
            Total = s.Total,
            Status = s.Status ?? ""
        }).ToList();
        return Ok(new { items });
    }

    /// <summary>Configuración mínima (empresa, moneda) para mostrar en app.</summary>
    [HttpGet("settings")]
    public IActionResult GetSettings()
    {
        var s = _settingsService.Get();
        return Ok(new MobileSettingsDto
        {
            CompanyName = s?.CompanyName ?? "OptiControl",
            Currency = s?.Currency ?? "NIO",
            ExchangeRate = s?.ExchangeRate ?? 36.8m
        });
    }
}

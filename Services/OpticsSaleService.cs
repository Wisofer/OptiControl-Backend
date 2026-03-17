using OptiControl.Data;
using OptiControl.Models.Dtos;
using OptiControl.Models.Entities;
using OptiControl.Services.IServices;
using OptiControl.Utils;
using Microsoft.EntityFrameworkCore;

namespace OptiControl.Services;

public class OpticsSaleService : IOpticsSaleService
{
    private readonly ApplicationDbContext _context;
    private readonly IActivityService _activity;
    private readonly IProductService _productService;

    public OpticsSaleService(ApplicationDbContext context, IActivityService activity, IProductService productService)
    {
        _context = context;
        _activity = activity;
        _productService = productService;
    }

    private static SaleItemDto ToItemDto(SaleItem i)
    {
        return new SaleItemDto
        {
            Type = i.Type,
            ProductId = i.ProductId,
            ServiceId = i.ServiceId,
            ProductName = i.ProductName,
            ServiceName = i.ServiceName,
            Quantity = i.Quantity,
            UnitPrice = i.UnitPrice,
            Subtotal = i.Subtotal
        };
    }

    private static SaleResponseDto ToSaleResponse(Sale s)
    {
        var items = s.SaleItems?.Select(ToItemDto).ToList() ?? new List<SaleItemDto>();
        return new SaleResponseDto
        {
            Id = "V" + s.Id,
            Date = s.Date.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
            ClientId = s.ClientId?.ToString() ?? "",
            ClientName = s.ClientName,
            Items = items,
            Total = s.Total,
            AmountPaid = s.AmountPaid,
            PaymentMethod = s.PaymentMethod,
            Currency = s.Currency ?? "NIO",
            Status = s.Status ?? ""
        };
    }

    /// <summary>Ingreso real: Pagada = total, pendiente = amountPaid, cotizacion/Cancelada = 0.</summary>
    private static decimal GetRealRevenue(Sale s)
    {
        if (s.Status == SD.SaleStatusCotizacion || s.Status == SD.SaleStatusCancelada)
            return 0;
        if (s.Status == SD.SaleStatusPagada)
            return s.Total;
        return s.AmountPaid; // pendiente
    }

    public SaleResponseDto? CreateSale(CreateSaleRequestDto dto)
    {
        var isCotizacion = string.Equals(dto.Status, SD.SaleStatusCotizacion, StringComparison.OrdinalIgnoreCase);
        int? clientIdParsed = null;
        if (!string.IsNullOrWhiteSpace(dto.ClientId) && int.TryParse(dto.ClientId.Trim(), out var cid))
            clientIdParsed = cid;

        var sale = new Sale
        {
            ClientId = clientIdParsed,
            ClientName = dto.ClientName ?? "",
            Date = DateTime.UtcNow,
            Total = dto.Total,
            AmountPaid = isCotizacion ? 0 : dto.AmountPaid,
            PaymentMethod = dto.PaymentMethod,
            Currency = dto.Currency ?? "NIO",
            Status = isCotizacion ? SD.SaleStatusCotizacion : (dto.AmountPaid >= dto.Total ? SD.SaleStatusPagada : SD.SaleStatusPendiente)
        };
        _context.Sales.Add(sale);
        _context.SaveChanges();

        foreach (var it in dto.Items ?? new List<SaleItemDto>())
        {
            var type = (it.Type ?? "product").ToLowerInvariant();
            var quantity = Math.Max(0, it.Quantity);
            var unitPrice = it.UnitPrice;
            var subtotal = it.Subtotal;
            var productName = it.ProductName ?? "";
            var serviceName = it.ServiceName ?? "";

            if (type == "product" && it.ProductId.HasValue && quantity > 0 && !isCotizacion)
            {
                var product = _context.Products.Find(it.ProductId.Value);
                if (product != null)
                {
                    if (product.Stock < quantity)
                    {
                        _context.Sales.Remove(sale);
                        _context.SaveChanges();
                        return null; // caller should check and return 400 "Stock insuficiente"
                    }
                    product.Stock -= quantity;
                    _context.SaveChanges();
                }
            }

            _context.SaleItems.Add(new SaleItem
            {
                SaleId = sale.Id,
                Type = type,
                ProductId = it.ProductId,
                ServiceId = it.ServiceId,
                ProductName = productName,
                ServiceName = serviceName,
                Quantity = quantity,
                UnitPrice = unitPrice,
                Subtotal = subtotal
            });
        }
        _context.SaveChanges();

        var currencySym = sale.Currency == "USD" ? "$" : "C$";
        var desc = isCotizacion
            ? $"Cotización guardada - {sale.ClientName} · {currencySym}{sale.Total:N0}"
            : $"Venta registrada - {sale.ClientName} · {currencySym}{sale.Total:N0}";
        _activity.Record(SD.ActivityTypeSale, desc, "V" + sale.Id, sale.ClientId);

        _context.Entry(sale).Collection(s => s.SaleItems).Load();
        return ToSaleResponse(sale);
    }

    public PagedResult<SaleResponseDto> GetSalesHistoryPaged(int page = 1, int pageSize = 20)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);
        page = Math.Max(1, page);
        var q = _context.Sales.Include(s => s.SaleItems).OrderByDescending(s => s.Date);
        var totalCount = q.Count();
        var list = q.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        var dtos = list.Select(ToSaleResponse).ToList();
        return PagedResult<SaleResponseDto>.Create(dtos, totalCount, page, pageSize);
    }

    public PagedResult<SaleResponseDto> GetSalesByClientId(int clientId, int page = 1, int pageSize = 10)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);
        page = Math.Max(1, page);
        var q = _context.Sales.Include(s => s.SaleItems).Where(s => s.ClientId == clientId).OrderByDescending(s => s.Date);
        var totalCount = q.Count();
        var list = q.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        var dtos = list.Select(ToSaleResponse).ToList();
        return PagedResult<SaleResponseDto>.Create(dtos, totalCount, page, pageSize);
    }

    public SaleResponseDto? GetSaleById(int id)
    {
        var s = _context.Sales.Include(sale => sale.SaleItems).FirstOrDefault(sale => sale.Id == id);
        return s == null ? null : ToSaleResponse(s);
    }

    public (bool Success, string? Error) CancelSale(int id)
    {
        var sale = _context.Sales.Include(s => s.SaleItems).FirstOrDefault(s => s.Id == id);
        if (sale == null) return (false, "Venta no encontrada");
        if (sale.Status == SD.SaleStatusCancelada) return (false, "La venta ya está cancelada");

        if (sale.Status == SD.SaleStatusPagada || sale.Status == SD.SaleStatusPendiente)
        {
            foreach (var item in sale.SaleItems.Where(i => i.Type == "product" && i.ProductId.HasValue && i.Quantity > 0))
            {
                var product = _context.Products.Find(item.ProductId!.Value);
                if (product != null)
                {
                    product.Stock += item.Quantity;
                }
            }
            _context.SaveChanges();
        }

        sale.Status = SD.SaleStatusCancelada;
        _context.SaveChanges();

        var currencySym = sale.Currency == "USD" ? "$" : "C$";
        _activity.Record(SD.ActivityTypeSale, $"Venta cancelada - {sale.ClientName} · {currencySym}{sale.Total:N0}", "V" + sale.Id, sale.ClientId);
        return (true, null);
    }

    public (bool Success, string? Error) AddPayment(int id, decimal addPayment)
    {
        if (addPayment <= 0) return (false, "El monto a abonar debe ser mayor a 0");
        var sale = _context.Sales.Find(id);
        if (sale == null) return (false, "Venta no encontrada");
        if (sale.Status != SD.SaleStatusPendiente) return (false, "Solo se puede abonar en ventas con estado Pendiente");

        sale.AmountPaid += addPayment;
        if (sale.AmountPaid >= sale.Total)
            sale.Status = SD.SaleStatusPagada;
        _context.SaveChanges();

        var currencySym = sale.Currency == "USD" ? "$" : "C$";
        _activity.Record(SD.ActivityTypeSale, $"Abono registrado - {sale.ClientName} · +{currencySym}{addPayment:N0}", "V" + sale.Id, sale.ClientId);
        return (true, null);
    }
}

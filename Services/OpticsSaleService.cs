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
    private readonly ISettingsService _settingsService;
    private readonly IHttpContextAccessor _httpContext;

    public OpticsSaleService(
        ApplicationDbContext context,
        IActivityService activity,
        ISettingsService settingsService,
        IHttpContextAccessor httpContext)
    {
        _context = context;
        _activity = activity;
        _settingsService = settingsService;
        _httpContext = httpContext;
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

    private static decimal Round2(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);

    private static bool IsUsdCurrency(string? currency)
        => string.Equals(currency, SD.CurrencyUSD, StringComparison.OrdinalIgnoreCase);

    private static decimal ToSaleCurrencyFromNio(decimal nio, string? saleCurrency, decimal rate)
    {
        if (IsUsdCurrency(saleCurrency))
            return rate > 0 ? Round2(nio / rate) : nio;
        return Round2(nio);
    }

    private decimal ResolveUnitPriceNio(SaleItemDto it)
    {
        var type = (it.Type ?? "product").ToLowerInvariant();
        if (type == "product" && it.ProductId.HasValue)
        {
            var p = _context.Products.Find(it.ProductId.Value);
            if (p != null) return p.Precio;
        }
        if (type == "service" && it.ServiceId.HasValue)
        {
            var s = _context.ServiceOpticas.Find(it.ServiceId.Value);
            if (s != null) return s.Precio;
        }
        // Fallback por compatibilidad (si no viene id, usamos lo enviado).
        return it.UnitPrice;
    }

    private string BuildSaleTicketPdfUrl(int saleId)
    {
        var req = _httpContext.HttpContext?.Request;
        var scheme = req?.Scheme ?? "https";
        var host = req?.Host.ToString() ?? "opticontrol.cowib.es";
        return $"{scheme}://{host}/api/sales-history/{saleId}/ticket-pdf";
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
        var saleCurrency = dto.Currency ?? SD.CurrencyNIO;
        var exchangeRate = _settingsService.Get()?.ExchangeRate ?? 36.8m;
        int? clientIdParsed = null;
        if (!string.IsNullOrWhiteSpace(dto.ClientId) && int.TryParse(dto.ClientId.Trim(), out var cid))
            clientIdParsed = cid;

        var computedItems = new List<SaleItem>();
        decimal totalNio = 0;
        foreach (var it in dto.Items ?? new List<SaleItemDto>())
        {
            var type = (it.Type ?? "product").ToLowerInvariant();
            var quantity = Math.Max(0, it.Quantity);
            var productName = it.ProductName ?? "";
            var serviceName = it.ServiceName ?? "";
            var unitNio = ResolveUnitPriceNio(it);
            var subtotalNio = unitNio * quantity;
            totalNio += subtotalNio;

            var unitSaleCurrency = ToSaleCurrencyFromNio(unitNio, saleCurrency, exchangeRate);
            var subtotalSaleCurrency = Round2(unitSaleCurrency * quantity);

            computedItems.Add(new SaleItem
            {
                Type = type,
                ProductId = it.ProductId,
                ServiceId = it.ServiceId,
                ProductName = productName,
                ServiceName = serviceName,
                Quantity = quantity,
                UnitPrice = unitSaleCurrency,
                Subtotal = subtotalSaleCurrency
            });
        }

        var totalSaleCurrency = Round2(computedItems.Sum(x => x.Subtotal));
        var amountPaidSaleCurrency = isCotizacion ? 0 : Round2(dto.AmountPaid);
        var saleStatus = isCotizacion ? SD.SaleStatusCotizacion : (amountPaidSaleCurrency >= totalSaleCurrency ? SD.SaleStatusPagada : SD.SaleStatusPendiente);

        var sale = new Sale
        {
            ClientId = clientIdParsed,
            ClientName = dto.ClientName ?? "",
            Date = DateTime.UtcNow,
            Total = totalSaleCurrency,
            AmountPaid = amountPaidSaleCurrency,
            PaymentMethod = dto.PaymentMethod,
            Currency = saleCurrency,
            Status = saleStatus
        };
        _context.Sales.Add(sale);
        _context.SaveChanges();

        foreach (var item in computedItems)
        {
            var type = item.Type;
            var quantity = item.Quantity;

            if (type == "product" && item.ProductId.HasValue && quantity > 0 && !isCotizacion)
            {
                var product = _context.Products.Find(item.ProductId.Value);
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
                ProductId = item.ProductId,
                ServiceId = item.ServiceId,
                ProductName = item.ProductName,
                ServiceName = item.ServiceName,
                Quantity = quantity,
                UnitPrice = item.UnitPrice,
                Subtotal = item.Subtotal
            });
        }
        _context.SaveChanges();

        var currencySym = sale.Currency == "USD" ? "$" : "C$";
        var desc = isCotizacion
            ? $"Cotización guardada - {sale.ClientName} · {currencySym}{sale.Total:N0}"
            : $"Venta registrada - {sale.ClientName} · {currencySym}{sale.Total:N0}";
        _activity.Record(SD.ActivityTypeSale, desc, "V" + sale.Id, sale.ClientId);

        _context.Entry(sale).Collection(s => s.SaleItems).Load();
        var response = ToSaleResponse(sale);
        response.SaleTicketPdfUrl = BuildSaleTicketPdfUrl(sale.Id);
        var totalUsd = exchangeRate > 0 ? Round2(totalNio / exchangeRate) : totalNio;
        var amountPaidNio = IsUsdCurrency(saleCurrency) ? Round2(amountPaidSaleCurrency * exchangeRate) : amountPaidSaleCurrency;
        var amountPaidUsd = IsUsdCurrency(saleCurrency) ? amountPaidSaleCurrency : (exchangeRate > 0 ? Round2(amountPaidSaleCurrency / exchangeRate) : amountPaidSaleCurrency);
        response.ExchangeRate = exchangeRate;
        response.TotalNio = Round2(totalNio);
        response.TotalUsd = totalUsd;
        response.AmountPaidNio = amountPaidNio;
        response.AmountPaidUsd = amountPaidUsd;
        response.ChangeDue = amountPaidSaleCurrency > totalSaleCurrency ? Round2(amountPaidSaleCurrency - totalSaleCurrency) : 0;
        response.ChangeCurrency = saleCurrency;
        return response;
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

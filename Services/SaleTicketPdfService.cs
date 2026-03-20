using Microsoft.EntityFrameworkCore;
using OptiControl.Data;
using OptiControl.Services.IServices;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace OptiControl.Services;

public class SaleTicketPdfService : ISaleTicketPdfService
{
    private readonly ApplicationDbContext _context;
    private readonly ISettingsService _settings;
    private readonly ILogger<SaleTicketPdfService> _logger;

    public SaleTicketPdfService(ApplicationDbContext context, ISettingsService settings, ILogger<SaleTicketPdfService> logger)
    {
        _context = context;
        _settings = settings;
        _logger = logger;
    }

    public byte[]? GeneratePdf(int saleId)
    {
        try
        {
            var sale = _context.Sales
                .Include(s => s.SaleItems)
                .FirstOrDefault(s => s.Id == saleId);
            if (sale == null) return null;

            var companyName = _settings.GetCompanyName();
            var title = string.Equals(sale.Status, "cotizacion", StringComparison.OrdinalIgnoreCase) ? "COTIZACION" : "TICKET DE VENTA";
            var settings = _settings.Get();
            var rate = settings?.ExchangeRate ?? 36.8m;
            var isUsdSale = string.Equals(sale.Currency, "USD", StringComparison.OrdinalIgnoreCase);

            // Base de negocio en C$: intentamos usar catálogo actual; si no hay id, convertimos desde la moneda de la venta.
            var productIds = sale.SaleItems.Where(x => x.ProductId.HasValue).Select(x => x.ProductId!.Value).Distinct().ToList();
            var serviceIds = sale.SaleItems.Where(x => x.ServiceId.HasValue).Select(x => x.ServiceId!.Value).Distinct().ToList();
            var productPriceMap = _context.Products
                .Where(p => productIds.Contains(p.Id))
                .ToDictionary(p => p.Id, p => p.Precio);
            var servicePriceMap = _context.ServiceOpticas
                .Where(s => serviceIds.Contains(s.Id))
                .ToDictionary(s => s.Id, s => s.Precio);

            decimal UnitPriceNio(Models.Entities.SaleItem item)
            {
                if (item.ProductId.HasValue && productPriceMap.TryGetValue(item.ProductId.Value, out var p)) return p;
                if (item.ServiceId.HasValue && servicePriceMap.TryGetValue(item.ServiceId.Value, out var s)) return s;
                return isUsdSale && rate > 0 ? Math.Round(item.UnitPrice * rate, 2, MidpointRounding.AwayFromZero) : item.UnitPrice;
            }

            var calc = sale.SaleItems.Select(item =>
            {
                var unitNio = UnitPriceNio(item);
                var subtotalNio = Math.Round(unitNio * item.Quantity, 2, MidpointRounding.AwayFromZero);
                return new
                {
                    Item = item,
                    UnitNio = unitNio,
                    SubtotalNio = subtotalNio
                };
            }).ToList();

            var totalNio = calc.Sum(x => x.SubtotalNio);
            var totalUsd = rate > 0 ? Math.Round(totalNio / rate, 2, MidpointRounding.AwayFromZero) : totalNio;
            var amountPaidNio = isUsdSale ? Math.Round(sale.AmountPaid * rate, 2, MidpointRounding.AwayFromZero) : sale.AmountPaid;
            var amountPaidUsd = isUsdSale ? sale.AmountPaid : (rate > 0 ? Math.Round(sale.AmountPaid / rate, 2, MidpointRounding.AwayFromZero) : sale.AmountPaid);
            var changeNio = amountPaidNio > totalNio ? Math.Round(amountPaidNio - totalNio, 2, MidpointRounding.AwayFromZero) : 0m;

            var doc = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.ContinuousSize(80, Unit.Millimetre);
                    page.Margin(8);
                    page.DefaultTextStyle(x => x.FontSize(8));

                    page.Header().Column(c =>
                    {
                        c.Item().AlignCenter().Text($"{companyName}\n{title}").Bold().FontSize(10);
                        c.Item().PaddingTop(4).LineHorizontal(1).LineColor(Colors.Grey.Medium);
                    });

                    page.Content().Column(c =>
                    {
                        c.Item().PaddingTop(4).Text($"No: V{sale.Id}");
                        c.Item().Text($"Fecha: {sale.Date:dd/MM/yyyy HH:mm}");
                        c.Item().Text($"Cliente: {sale.ClientName}");
                        c.Item().Text($"Estado: {sale.Status}");
                        if (!string.IsNullOrWhiteSpace(sale.PaymentMethod))
                            c.Item().Text($"Forma de pago: {sale.PaymentMethod}");
                        c.Item().PaddingTop(3).LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten2);

                        c.Item().PaddingTop(3).Text("Producto / Servicio").Bold();
                        c.Item().Text("Cant.  Subtotal").FontSize(7).FontColor(Colors.Grey.Darken1);

                        foreach (var x in calc)
                        {
                            var item = x.Item;
                            var name = !string.IsNullOrWhiteSpace(item.ProductName) ? item.ProductName : item.ServiceName;
                            c.Item().PaddingTop(2).Text(name);
                            c.Item().Row(r =>
                            {
                                r.RelativeItem().Text($"{item.Quantity}");
                                r.RelativeItem().AlignRight().Text($"C$ {x.SubtotalNio:N2}");
                            });
                        }

                        c.Item().PaddingTop(4).LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten2);
                        c.Item().PaddingTop(4).Row(r =>
                        {
                            r.RelativeItem().Text("Total:").Bold().FontSize(10);
                            r.RelativeItem().AlignRight().Text($"C$ {totalNio:N2}").Bold().FontSize(10);
                        });

                        if (isUsdSale)
                        {
                            c.Item().PaddingTop(1).AlignRight().Text($"Equiv. USD {totalUsd:N2} (TC {rate:N2})").FontSize(8);
                            c.Item().PaddingTop(1).AlignRight().Text($"Pagó: USD {amountPaidUsd:N2}").FontSize(8);
                            c.Item().PaddingTop(1).AlignRight().Text($"Vuelto: C$ {changeNio:N2}").FontSize(8);
                        }
                    });
                });
            });

            return doc.GeneratePdf();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar PDF de ticket para venta {SaleId}", saleId);
            return null;
        }
    }
}

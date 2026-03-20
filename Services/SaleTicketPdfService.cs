using System.IO;
using Microsoft.AspNetCore.Hosting;
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
    private readonly string[] _logoPaths;

    public SaleTicketPdfService(
        ApplicationDbContext context,
        ISettingsService settings,
        IWebHostEnvironment env,
        ILogger<SaleTicketPdfService> logger)
    {
        _context = context;
        _settings = settings;
        _logger = logger;
        var webRoot = env.WebRootPath ?? Path.Combine(env.ContentRootPath, "wwwroot");
        _logoPaths = new[]
        {
            Path.Combine(webRoot, "images", "tulogo.png"),
            Path.Combine(webRoot, "images", "logo.png")
        };
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
            var agencyEmail = settings?.Email?.Trim() ?? "";
            var agencyPhone = settings?.Phone?.Trim() ?? "";
            var agencyAddress = settings?.Address?.Trim() ?? "";
            var rate = settings?.ExchangeRate ?? 36.8m;
            var isUsdSale = string.Equals(sale.Currency, "USD", StringComparison.OrdinalIgnoreCase);
            byte[]? logoBytes = null;
            var logoPath = _logoPaths.FirstOrDefault(File.Exists);
            if (!string.IsNullOrWhiteSpace(logoPath))
            {
                try { logoBytes = File.ReadAllBytes(logoPath); } catch { /* ignorar */ }
            }

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

            var doc = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.ContinuousSize(80, Unit.Millimetre);
                    page.Margin(8);
                    page.DefaultTextStyle(x => x.FontSize(8));

                    page.Header().Column(c =>
                    {
                        c.Item().Row(row =>
                        {
                            row.ConstantItem(64).AlignLeft().Element(x =>
                            {
                                if (logoBytes != null && logoBytes.Length > 0)
                                    x.Width(64).MaxHeight(26).Image(logoBytes).FitArea();
                            });
                            row.RelativeItem().AlignRight().AlignMiddle()
                                .Text($"{companyName}\n{title}").Bold().FontSize(9);
                        });
                        c.Item().PaddingTop(3).Column(contact =>
                        {
                            if (!string.IsNullOrWhiteSpace(agencyEmail))
                                contact.Item().Text(agencyEmail).FontSize(7);
                            if (!string.IsNullOrWhiteSpace(agencyPhone))
                                contact.Item().Text($"Tel/WhatsApp: {agencyPhone}").FontSize(7);
                            if (!string.IsNullOrWhiteSpace(agencyAddress))
                                contact.Item().Text(agencyAddress).FontSize(6);
                        });
                        c.Item().PaddingTop(4).LineHorizontal(1).LineColor(Colors.Grey.Medium);
                    });

                    page.Content().Column(c =>
                    {
                        c.Item().PaddingTop(4).Row(r =>
                        {
                            r.RelativeItem().Text($"No: V{sale.Id}");
                            r.RelativeItem().AlignRight().Text($"Estado: {sale.Status}");
                        });
                        c.Item().Row(r =>
                        {
                            r.RelativeItem().Text($"Fecha: {sale.Date:dd/MM/yyyy HH:mm}");
                            r.RelativeItem().AlignRight().Text($"Pago: {sale.PaymentMethod}");
                        });
                        c.Item().Text($"Cliente: {sale.ClientName}");
                        c.Item().PaddingTop(3).LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten2);

                        c.Item().PaddingTop(3).Text("Producto / Servicio").Bold();
                        c.Item().Row(r =>
                        {
                            r.RelativeItem().Text("Detalle")
                                .FontSize(7).FontColor(Colors.Grey.Darken1);
                            r.RelativeItem().AlignRight().Text("Subtotal")
                                .FontSize(7).FontColor(Colors.Grey.Darken1);
                        });

                        foreach (var x in calc)
                        {
                            var item = x.Item;
                            var name = !string.IsNullOrWhiteSpace(item.ProductName) ? item.ProductName : item.ServiceName;
                            c.Item().PaddingTop(2).Row(r =>
                            {
                                r.RelativeItem().Text(name);
                                r.RelativeItem().AlignRight().Text($"C$ {x.SubtotalNio:N2}");
                            });
                            c.Item().Row(r =>
                            {
                                r.RelativeItem().Text($"{item.Quantity} x C$ {x.UnitNio:N2}")
                                    .FontSize(7).FontColor(Colors.Grey.Darken1);
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
                        }

                        c.Item().PaddingTop(5).LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten2);
                        c.Item().PaddingTop(3).AlignCenter()
                            .Text("Gracias por su compra.")
                            .FontSize(8).FontColor(Colors.Grey.Darken1);
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

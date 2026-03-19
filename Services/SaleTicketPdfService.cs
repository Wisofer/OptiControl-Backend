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
            var currency = string.Equals(sale.Currency, "USD", StringComparison.OrdinalIgnoreCase) ? "USD" : "NIO";

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

                        foreach (var item in sale.SaleItems)
                        {
                            var name = !string.IsNullOrWhiteSpace(item.ProductName) ? item.ProductName : item.ServiceName;
                            c.Item().PaddingTop(2).Text(name);
                            c.Item().Row(r =>
                            {
                                r.RelativeItem().Text($"{item.Quantity}");
                                r.RelativeItem().AlignRight().Text($"{currency} {item.Subtotal:N2}");
                            });
                        }

                        c.Item().PaddingTop(4).LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten2);
                        c.Item().PaddingTop(4).Row(r =>
                        {
                            r.RelativeItem().Text("Total:").Bold().FontSize(10);
                            r.RelativeItem().AlignRight().Text($"{currency} {sale.Total:N2}").Bold().FontSize(10);
                        });
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

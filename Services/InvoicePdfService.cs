using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using OptiControl.Data;
using OptiControl.Services.IServices;
using OptiControl.Utils;

namespace OptiControl.Services;

public class InvoicePdfService : IInvoicePdfService
{
    private readonly ApplicationDbContext _context;
    private readonly ISettingsService _settings;
    private readonly ILogger<InvoicePdfService> _logger;
    private readonly string _logoPath;

    public InvoicePdfService(ApplicationDbContext context, ISettingsService settings, IWebHostEnvironment env, ILogger<InvoicePdfService> logger)
    {
        _context = context;
        _settings = settings;
        _logger = logger;
        // Logo en wwwroot/images para orden: mismo sitio que el resto de estáticos y accesible como /images/logo.png
        var webRoot = env.WebRootPath ?? Path.Combine(env.ContentRootPath, "wwwroot");
        _logoPath = Path.Combine(webRoot, "images", "logo.png");
    }

    public byte[]? GeneratePdf(string invoiceId)
    {
        try
        {
            return GeneratePdfCore(invoiceId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar PDF de factura {InvoiceId}", invoiceId);
            return null;
        }
    }

    private byte[]? GeneratePdfCore(string invoiceId)
    {
        var invoice = _context.Invoices
            .Include(i => i.Client)
            .FirstOrDefault(i => i.Id == invoiceId);
        if (invoice == null) return null;

        var companyName = _settings.GetCompanyName();
        var settings = _settings.Get();
        var currencyNio = settings?.Currency ?? "NIO";
        var rate = settings?.ExchangeRate ?? 36.8m;
        // Datos de contacto de la agencia: vienen de "Datos de la Agencia" (configuración). Una línea de teléfono es fija (empresa).
        var agencyEmail = settings?.Email?.Trim() ?? "";
        var agencyPhone = settings?.Phone?.Trim() ?? "";
        var agencyAddress = settings?.Address?.Trim() ?? "";
        var isPaidInUsd = string.Equals(invoice.PaymentMethod, SD.FormaPagoDolares, StringComparison.OrdinalIgnoreCase) ||
                         string.Equals(invoice.PaymentMethod, SD.FormaPagoTransferenciaDolares, StringComparison.OrdinalIgnoreCase);
        var totalCurrency = isPaidInUsd ? "USD" : currencyNio;
        var client = invoice.Client;

        // Fechas en UTC para que el día no cambie al imprimir (evita 01/03 en vez de 02/03 por zona horaria)
        static DateTime UtcDate(DateTime d) => d.Kind == DateTimeKind.Utc ? d.Date : d.ToUniversalTime().Date;
        static string FormatUtcDate(DateTime d) => UtcDate(d).ToString("dd/MM/yyyy");
        static string FormatUtcDateNullable(DateTime? d) => d.HasValue ? FormatUtcDate(d.Value) : "-";

        byte[]? logoBytes = null;
        if (File.Exists(_logoPath))
        {
            try { logoBytes = File.ReadAllBytes(_logoPath); } catch { /* ignorar */ }
        }

        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                // Formato ticket 80 mm de ancho; alto según contenido
                page.ContinuousSize(80, Unit.Millimetre);
                page.Margin(8);
                page.DefaultTextStyle(x => x.FontSize(8));

                // ----- Cabecera: logo + nombre + FACTURA + datos de la agencia (correo, teléfono, dirección) -----
                page.Header().Column(headerCol =>
                {
                    headerCol.Item().Row(row =>
                    {
                        row.ConstantItem(120).AlignLeft().Element(c =>
                        {
                            if (logoBytes != null && logoBytes.Length > 0)
                                c.Width(120).MaxHeight(36).Image(logoBytes).FitArea();
                        });
                        row.RelativeItem().AlignCenter().AlignMiddle()
                            .Text((companyName ?? "Aventours") + "\nFACTURA").Bold().FontSize(10);
                    });
                    headerCol.Item().PaddingTop(4).Column(contact =>
                    {
                        contact.Item().Text(string.IsNullOrWhiteSpace(agencyEmail) ? "-" : agencyEmail).FontSize(7);
                        if (!string.IsNullOrWhiteSpace(agencyPhone))
                            contact.Item().Text($"Tel/WhatsApp: {agencyPhone}").FontSize(7);
                        contact.Item().Text("Tel/WhatsApp empresa: 8484 9254").FontSize(7);
                        contact.Item().Text(string.IsNullOrWhiteSpace(agencyAddress) ? "-" : agencyAddress).FontSize(6);
                    });
                    headerCol.Item().PaddingTop(4).LineHorizontal(1).LineColor(Colors.Grey.Medium);
                });

                page.Content().Column(column =>
                {
                    // ----- Datos documento (ticket: una línea por dato) -----
                    column.Item().PaddingTop(4).Row(row =>
                    {
                        row.RelativeItem().Text($"Nº {invoice.Id}").Bold();
                        row.RelativeItem().AlignRight().Text($"{FormatUtcDate(invoice.Date)} | {invoice.Status}");
                    });
                    column.Item().PaddingTop(4).LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten2);

                    // ----- Cliente (solo nombre y teléfono; no correo) -----
                    column.Item().PaddingTop(4).Text("CLIENTE").Bold().FontSize(9);
                    column.Item().PaddingTop(1).Text(client?.Name ?? "-");
                    if (client != null && !string.IsNullOrWhiteSpace(client.Phone))
                        column.Item().Text($"Tel: {client.Phone}");
                    column.Item().PaddingTop(3).LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten2);

                    // ----- Concepto -----
                    column.Item().PaddingTop(4).Text("CONCEPTO").Bold().FontSize(9);
                    column.Item().PaddingTop(1).Text(string.IsNullOrWhiteSpace(invoice.Concept) ? "-" : invoice.Concept);
                    column.Item().PaddingTop(3).LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten2);

                    // ----- Forma de pago -----
                    if (!string.IsNullOrWhiteSpace(invoice.PaymentMethod))
                    {
                        column.Item().PaddingTop(3).Row(row =>
                        {
                            row.RelativeItem().Text("Forma de pago:").Bold();
                            row.RelativeItem().AlignRight().Text(invoice.PaymentMethod);
                        });
                        column.Item().PaddingTop(3).LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten2);
                    }

                    // ----- Vencimiento -----
                    var dueText = FormatUtcDateNullable(invoice.DueDate);
                    column.Item().PaddingTop(3).Row(row =>
                    {
                        row.RelativeItem().Text("Vencimiento:").Bold();
                        row.RelativeItem().AlignRight().Text(dueText);
                    });
                    column.Item().PaddingTop(3).LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten2);

                    // ----- Fecha de viaje y retorno -----
                    if (invoice.TravelDate.HasValue || invoice.ReturnDate.HasValue)
                    {
                        var travelText = FormatUtcDateNullable(invoice.TravelDate);
                        var returnText = FormatUtcDateNullable(invoice.ReturnDate);
                        column.Item().PaddingTop(3).Row(row =>
                        {
                            row.RelativeItem().Text("Fecha viaje:").Bold();
                            row.RelativeItem().AlignRight().Text(travelText);
                        });
                        column.Item().PaddingTop(1).Row(row =>
                        {
                            row.RelativeItem().Text("Fecha retorno:").Bold();
                            row.RelativeItem().AlignRight().Text(returnText);
                        });
                        column.Item().PaddingTop(3).LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten2);
                    }

                    // ----- Total (tipo ticket, destacado) -----
                    column.Item().PaddingTop(6).PaddingBottom(4)
                        .Background(Colors.Grey.Lighten3)
                        .Padding(8)
                        .Column(totalColumn =>
                        {
                            totalColumn.Item().Row(row =>
                            {
                                row.RelativeItem().AlignMiddle().Text("TOTAL").Bold().FontSize(11);
                                row.RelativeItem().AlignRight().AlignMiddle()
                                    .Text($"{invoice.Amount:N2} {totalCurrency}").Bold().FontSize(11);
                            });
                            if (isPaidInUsd)
                            {
                                var equivalentCordobas = CurrencyHelper.ToCordobas(invoice.Amount, invoice.PaymentMethod, rate);
                                totalColumn.Item().PaddingTop(2).AlignRight()
                                    .Text($"Equiv. {equivalentCordobas:N2} {currencyNio}").FontSize(9);
                            }
                        });
                    column.Item().LineHorizontal(1).LineColor(Colors.Grey.Medium);
                });

                page.Footer().Height(0);
            });
        });

        return doc.GeneratePdf();
    }
}

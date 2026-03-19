using OptiControl.Models.Entities;

namespace OptiControl.Utils;

public static class SaleInvoiceHelper
{
    public static bool IsPaidSale(string? status)
    {
        if (string.IsNullOrWhiteSpace(status)) return false;
        return status.Equals(SD.SaleStatusPagada, StringComparison.OrdinalIgnoreCase) ||
               status.Equals(SD.SaleStatusCompletado, StringComparison.OrdinalIgnoreCase);
    }

    public static string BuildSaleConcept(Sale sale)
    {
        var topItems = sale.SaleItems
            .Take(3)
            .Select(i => !string.IsNullOrWhiteSpace(i.ProductName) ? i.ProductName : i.ServiceName)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToList();
        var details = topItems.Count > 0 ? $" ({string.Join(", ", topItems)})" : "";
        return $"Venta V{sale.Id}{details}";
    }

    public static string MapInvoicePaymentMethod(string? salePaymentMethod, string? currency)
    {
        var isUsd = string.Equals(currency, SD.CurrencyUSD, StringComparison.OrdinalIgnoreCase);
        var isTransfer = !string.IsNullOrWhiteSpace(salePaymentMethod) &&
                         salePaymentMethod.Contains("transfer", StringComparison.OrdinalIgnoreCase);

        if (isUsd)
            return isTransfer ? SD.FormaPagoTransferenciaDolares : SD.FormaPagoDolares;
        return isTransfer ? SD.FormaPagoTransferenciaCordobas : SD.FormaPagoCordobas;
    }
}

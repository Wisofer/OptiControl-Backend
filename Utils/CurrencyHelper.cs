namespace OptiControl.Utils;

/// <summary>Convierte montos a córdobas (NIO). USD se multiplica por la tasa.</summary>
public static class CurrencyHelper
{
    /// <summary>Devuelve el monto en córdobas. Si paymentMethod es "Dolares" o "TransferenciaDolares", convierte con la tasa; en caso contrario (Córdobas, Transferencia, TransferenciaCordobas o null) se asume C$.</summary>
    public static decimal ToCordobas(decimal amount, string? paymentMethod, decimal rateUsdToNio)
    {
        if (string.Equals(paymentMethod, SD.FormaPagoDolares, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(paymentMethod, SD.FormaPagoTransferenciaDolares, StringComparison.OrdinalIgnoreCase))
            return amount * rateUsdToNio;
        return amount;
    }

    /// <summary>Convierte un monto a córdobas según la moneda de la venta (Sale.Currency). Si es USD, multiplica por rateUsdToNio.</summary>
    public static decimal SaleAmountToCordobas(decimal amount, string? currency, decimal rateUsdToNio)
    {
        if (string.Equals(currency, SD.CurrencyUSD, StringComparison.OrdinalIgnoreCase))
            return amount * rateUsdToNio;
        return amount; // NIO o null se asume córdobas
    }
}

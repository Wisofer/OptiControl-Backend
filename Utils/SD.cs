namespace OptiControl.Utils;

/// <summary>Constantes Aventours (OptiControl).</summary>
public static class SD
{
    public const string RolAdministrador = "Administrador";
    public const string RolUsuario = "Usuario";
    public const string RolNormal = "Usuario"; // alias para compatibilidad

    public const string ClientStatusPendiente = "Pendiente";
    public const string ClientStatusViajo = "Viajó";

    public const string PaymentStatusPagado = "Pagado";
    public const string PaymentStatusPendiente = "Pendiente";
    public const string PaymentStatusParcial = "Parcial";

    public const string SaleStatusCompletado = "Completado";
    public const string SaleStatusPendiente = "Pendiente";
    public const string SaleStatusPagada = "Pagada";
    public const string SaleStatusCotizacion = "cotizacion";
    public const string SaleStatusCancelada = "Cancelada";

    public const string InvoiceStatusPagado = "Pagado";
    public const string InvoiceStatusPendiente = "Pendiente";
    public const string InvoiceStatusVencida = "Vencida";

    public const string ExpenseCategoryOperativo = "Operativo";
    public const string ExpenseCategoryFijo = "Fijo";
    public const string ExpenseCategoryMarketing = "Marketing";

    public const string ActivityTypeReservation = "reservation";
    public const string ActivityTypeInvoice = "invoice";
    public const string ActivityTypePayment = "payment";
    public const string ActivityTypeClient = "client";
    public const string ActivityTypeExpense = "expense";
    public const string ActivityTypeTemplate = "template";
    public const string ActivityTypeUser = "user";
    public const string ActivityTypeSale = "sale";
    public const string ActivityTypeProduct = "product";
    public const string ActivityTypeInventory = "inventory";
    public const string ActivityTypeService = "service";

    public const string CurrencyNIO = "NIO";
    public const string CurrencyUSD = "USD";

    /// <summary>Forma de pago: Córdobas | Dólares | Transferencia (legacy) | TransferenciaCordobas | TransferenciaDolares</summary>
    public const string FormaPagoCordobas = "Cordobas";
    public const string FormaPagoDolares = "Dolares";
    /// <summary>Legacy: se trata como C$. Preferir TransferenciaCordobas o TransferenciaDolares.</summary>
    public const string FormaPagoTransferencia = "Transferencia";
    public const string FormaPagoTransferenciaCordobas = "TransferenciaCordobas";
    public const string FormaPagoTransferenciaDolares = "TransferenciaDolares";
}

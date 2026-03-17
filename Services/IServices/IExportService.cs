namespace OptiControl.Services.IServices;

public interface IExportService
{
    byte[] GetClientsExcel(string? search = null);
    byte[] GetClientsPdf(string? search = null);
    byte[] GetReservationsExcel(int? clientId = null, string? paymentStatus = null, string? paymentMethod = null, DateTime? dateFrom = null, DateTime? dateTo = null);
    byte[] GetReservationsPdf(int? clientId = null, string? paymentStatus = null, string? paymentMethod = null, DateTime? dateFrom = null, DateTime? dateTo = null);
    byte[] GetInvoicesExcel(int? clientId = null, string? status = null, string? paymentMethod = null, DateTime? dateFrom = null, DateTime? dateTo = null);
    byte[] GetInvoicesPdf(int? clientId = null, string? status = null, string? paymentMethod = null, DateTime? dateFrom = null, DateTime? dateTo = null);
    byte[] GetExpensesExcel(DateTime? dateFrom = null, DateTime? dateTo = null);
    byte[] GetExpensesPdf(DateTime? dateFrom = null, DateTime? dateTo = null);
    byte[] GetFinancialSummaryExcel(DateTime? dateFrom = null, DateTime? dateTo = null);
    byte[] GetFinancialSummaryPdf(DateTime? dateFrom = null, DateTime? dateTo = null);
}

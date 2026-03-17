namespace OptiControl.Services.IServices;

public interface IInvoicePdfService
{
    /// <summary>Genera el PDF de la factura y lo devuelve como bytes.</summary>
    byte[]? GeneratePdf(string invoiceId);
}

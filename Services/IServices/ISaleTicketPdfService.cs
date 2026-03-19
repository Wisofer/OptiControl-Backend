namespace OptiControl.Services.IServices;

public interface ISaleTicketPdfService
{
    byte[]? GeneratePdf(int saleId);
}

namespace OptiControl.Models.Entities;

/// <summary>Plantilla de mensaje WhatsApp para envío de facturas. Variables: {NombreCliente}, {CodigoCliente}, {NumeroFactura}, {Monto}, {Mes}, {Categoria}, {Estado}, {FechaCreacion}, {EnlacePDF}</summary>
public class WhatsAppTemplate
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
    public bool Activa { get; set; } = true;
    public bool Predeterminada { get; set; } = false;
}

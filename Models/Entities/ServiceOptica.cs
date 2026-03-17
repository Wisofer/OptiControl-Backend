namespace OptiControl.Models.Entities;

/// <summary>Servicio de la óptica (examen visual, ajuste de lentes, etc.). No tiene stock.</summary>
public class ServiceOptica
{
    public int Id { get; set; }
    public string NombreServicio { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public string? Descripcion { get; set; }
    public DateTime FechaCreacion { get; set; }
}

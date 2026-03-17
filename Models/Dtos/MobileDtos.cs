using System.Text.Json.Serialization;

namespace OptiControl.Models.Dtos;

/// <summary>Versión reducida para Flutter/móvil. Respuestas en camelCase, menos campos.</summary>

public class MobileSummaryDto
{
    [JsonPropertyName("totalRevenue")]
    public decimal TotalRevenue { get; set; }
    [JsonPropertyName("salesToday")]
    public decimal SalesToday { get; set; }
    [JsonPropertyName("salesMonth")]
    public decimal SalesMonth { get; set; }
    [JsonPropertyName("productsCount")]
    public int ProductsCount { get; set; }
    [JsonPropertyName("clientsCount")]
    public int ClientsCount { get; set; }
}

public class MobileProductDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";
    [JsonPropertyName("price")]
    public decimal Price { get; set; }
    [JsonPropertyName("stock")]
    public int Stock { get; set; }
    [JsonPropertyName("type")]
    public string Type { get; set; } = "";
}

public class MobileServiceDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";
    [JsonPropertyName("price")]
    public decimal Price { get; set; }
}

public class MobileClientDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";
    [JsonPropertyName("phone")]
    public string? Phone { get; set; }
}

public class MobileSaleDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";
    [JsonPropertyName("date")]
    public string Date { get; set; } = "";
    [JsonPropertyName("clientName")]
    public string? ClientName { get; set; }
    [JsonPropertyName("total")]
    public decimal Total { get; set; }
    [JsonPropertyName("status")]
    public string Status { get; set; } = "";
}

public class MobileSettingsDto
{
    [JsonPropertyName("companyName")]
    public string CompanyName { get; set; } = "";
    [JsonPropertyName("currency")]
    public string Currency { get; set; } = "NIO";
    [JsonPropertyName("exchangeRate")]
    public decimal ExchangeRate { get; set; }
}

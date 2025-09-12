using System.Text.Json.Serialization;

namespace MochaStorefront.Core.Models;

public class Price
{
    [JsonPropertyName("currencyType")]
    public string CurrencyType { get; set; } = string.Empty;

    [JsonPropertyName("currencySubType")]
    public string CurrencySubType { get; set; } = string.Empty;

    [JsonPropertyName("regularPrice")]
    public int RegularPrice { get; set; }

    [JsonPropertyName("dynamicRegularPrice")]
    public int DynamicRegularPrice { get; set; }

    [JsonPropertyName("finalPrice")]
    public int FinalPrice { get; set; }

    [JsonPropertyName("saleExpiration")]
    public string? SaleExpiration { get; set; }

    [JsonPropertyName("saleType")]
    public string? SaleType { get; set; }

    [JsonPropertyName("basePrice")]
    public int BasePrice { get; set; }
}
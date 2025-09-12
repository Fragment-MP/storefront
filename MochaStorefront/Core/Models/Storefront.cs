using System.Text.Json.Serialization;

namespace MochaStorefront.Core.Models;

public class Storefront
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("catalogEntries")]
    public List<CatalogEntry> CatalogEntries { get; set; } = new List<CatalogEntry>();
}
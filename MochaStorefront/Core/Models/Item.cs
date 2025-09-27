using System.Text.Json.Serialization;

namespace MochaStorefront.Core.Models;

public class Item
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("id")]
    public string ID { get; set; } = string.Empty;
    [JsonPropertyName("full_name")]
    public string FullName { get; set; } = string.Empty;
    [JsonPropertyName("isBundle")]
    public bool IsBundle { get; set; }

    [JsonPropertyName("price")]
    public int Price { get; set; }

    [JsonPropertyName("backpack")]
    public KnownItem Backpack { get; set; }

    [JsonPropertyName("category")]
    public string Category { get; set; } = string.Empty;

    [JsonPropertyName("set")]
    public ItemSet Set { get; set; } = new ItemSet();

    [JsonPropertyName("item")]
    public ItemDetails ItemDetail { get; set; } = new ItemDetails();

    [JsonPropertyName("images")]
    public ItemImages Images { get; set; } = new ItemImages();

    [JsonIgnore]
    public int Position { get; set; }
}
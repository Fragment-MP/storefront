using System.Text.Json.Serialization;

namespace MochaStorefront.Core.Models;

public class KnownItem
{
    [JsonPropertyName("id")]
    public string ID { get; set; } = string.Empty;

    [JsonPropertyName("set")]
    public ItemSet Set { get; set; } = new ItemSet();

    [JsonPropertyName("item")]
    public ItemDetails Item { get; set; } = new ItemDetails();
    [JsonPropertyName("backpack")]
    public KnownItem Backpack { get; set; }

    [JsonPropertyName("images")]
    public ItemImages Images { get; set; } = new ItemImages();
    [JsonPropertyName("itemPreviewHeroPath")]
    public string ItemPreviewHeroPath { get; set; } = string.Empty;
}
using System.Text.Json.Serialization;

namespace MochaStorefront.Core.Models;

public class Meta
{
    [JsonPropertyName("templateId")]
    public string? TemplateId { get; set; }

    [JsonPropertyName("displayAssetPath")]
    public string? DisplayAssetPath { get; set; }

    [JsonPropertyName("newDisplayAssetPath")]
    public string? NewDisplayAssetPath { get; set; }

    [JsonPropertyName("layoutId")]
    public string? LayoutId { get; set; }

    [JsonPropertyName("tileSize")]
    public string? TileSize { get; set; }

    [JsonPropertyName("analyticOfferGroupId")]
    public string? AnalyticOfferGroupId { get; set; }

    [JsonPropertyName("sectionId")]
    public string? SectionId { get; set; }
}
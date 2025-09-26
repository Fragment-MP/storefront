using System.Text.Json.Serialization;

namespace MochaStorefront.Core.Models;

public class Meta
{
    [JsonPropertyName("TemplateId")]
    public string? TemplateId { get; set; }

    [JsonPropertyName("DisplayAssetPath")]
    public string? DisplayAssetPath { get; set; }

    [JsonPropertyName("NewDisplayAssetPath")]
    public string? NewDisplayAssetPath { get; set; }

    [JsonPropertyName("LayoutId")]
    public string? LayoutId { get; set; }

    [JsonPropertyName("TileSize")]
    public string? TileSize { get; set; }

    [JsonPropertyName("AnalyticOfferGroupId")]
    public string? AnalyticOfferGroupId { get; set; }

    [JsonPropertyName("SectionId")]
    public string? SectionId { get; set; }
}
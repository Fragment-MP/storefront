using System.Text.Json.Serialization;

namespace MochaStorefront.Core.Models;

public class ItemImages
{
    [JsonPropertyName("icon")]
    public string Icon { get; set; } = string.Empty;

    [JsonPropertyName("smallIcon")]
    public string SmallIcon { get; set; } = string.Empty;
}
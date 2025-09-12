using System.Text.Json.Serialization;

namespace MochaStorefront.Core.Models;

public class ItemDetails
{
    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;

    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;

    [JsonPropertyName("backendValue")]
    public string BackendValue { get; set; } = string.Empty;
}
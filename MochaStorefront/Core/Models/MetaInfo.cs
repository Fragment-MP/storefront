using System.Text.Json.Serialization;

namespace MochaStorefront.Core.Models;

public class MetaInfo
{
    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;

    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;
}
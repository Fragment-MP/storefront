using System.Text.Json.Serialization;

namespace MochaStorefront.Core.Models;

public class Requirement
{
    [JsonPropertyName("requirementType")]
    public string RequirementType { get; set; } = string.Empty;

    [JsonPropertyName("requiredId")]
    public string RequiredId { get; set; } = string.Empty;

    [JsonPropertyName("minQuantity")]
    public int MinQuantity { get; set; }
}
using System.Text.Json.Serialization;

namespace MochaStorefront.Core.Models;

public class ItemGrant
{
    [JsonPropertyName("templateId")]
    public string TemplateId { get; set; } = string.Empty;

    [JsonPropertyName("quantity")]
    public int Quantity { get; set; }
}
using System.Text.Json.Serialization;

namespace MochaStorefront.Core.Models;

public class GiftInfo
{
    [JsonPropertyName("bIsEnabled")]
    public bool IsEnabled { get; set; }

    [JsonPropertyName("forcedGiftBoxTemplateId")]
    public string ForcedGiftBoxTemplateId { get; set; } = string.Empty;

    [JsonPropertyName("purchaseRequirements")]
    public List<Requirement> PurchaseRequirements { get; set; } = new List<Requirement>();

    [JsonPropertyName("giftRecordIds")]
    public List<string> GiftRecordIds { get; set; } = new List<string>();
}
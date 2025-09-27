using System.Text.Json.Serialization;

namespace MochaStorefront.Core.Models;

public class CatalogEntry
{
    [JsonPropertyName("offerId")] public string OfferId { get; set; } = string.Empty;

    [JsonPropertyName("offerType")] public string OfferType { get; set; } = string.Empty;

    [JsonPropertyName("devName")] public string DevName { get; set; } = string.Empty;

    [JsonPropertyName("itemGrants")] public List<ItemGrant> ItemGrants { get; set; } = new List<ItemGrant>();

    [JsonPropertyName("requirements")] public List<Requirement> Requirements { get; set; } = new List<Requirement>();

    [JsonPropertyName("categories")] public List<string> Categories { get; set; } = new List<string>();

    [JsonPropertyName("metaInfo")] public List<MetaInfo> MetaInfo { get; set; } = new List<MetaInfo>();

    [JsonPropertyName("meta")] public Meta Meta { get; set; } = new Meta();

    [JsonPropertyName("giftInfo")] public GiftInfo GiftInfo { get; set; } = new GiftInfo();

    [JsonPropertyName("prices")] public List<Price> Prices { get; set; } = new List<Price>();

    [JsonPropertyName("bannerOverride")] public string? BannerOverride { get; set; }

    [JsonPropertyName("displayAssetPath")] public string? DisplayAssetPath { get; set; }

    [JsonPropertyName("newDisplayAssetPath")]
    public string? NewDisplayAssetPath { get; set; }

    [JsonPropertyName("refundable")] public bool Refundable { get; set; }

    [JsonPropertyName("title")] public string? Title { get; set; }

    [JsonPropertyName("description")] public string? Description { get; set; }

    [JsonPropertyName("shortDescription")] public string? ShortDescription { get; set; }

    [JsonPropertyName("appStoreId")] public List<string> AppStoreIds { get; set; } = new List<string>();

    [JsonPropertyName("fulfillmentIds")] public List<object> FulfillmentIds { get; set; } = new List<object>();

    [JsonPropertyName("dailyLimit")] public int DailyLimit { get; set; }

    [JsonPropertyName("weeklyLimit")] public int WeeklyLimit { get; set; }

    [JsonPropertyName("monthlyLimit")] public int MonthlyLimit { get; set; }

    [JsonPropertyName("sortPriority")] public int SortPriority { get; set; }

    [JsonPropertyName("catalogGroupPriority")]
    public int CatalogGroupPriority { get; set; }

    [JsonPropertyName("filterWeight")] public double FilterWeight { get; set; }

    [JsonPropertyName("matchFilter")] public string? MatchFilter { get; set; }

    [JsonPropertyName("additionalGrants")]
    public List<ItemGrant> AdditionalGrants { get; set; } = new List<ItemGrant>();
}
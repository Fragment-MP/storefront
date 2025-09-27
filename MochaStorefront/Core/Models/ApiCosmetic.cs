using System.Text.Json.Serialization;

namespace MochaStorefront.Core.Models;

public class ApiCosmetic
{
    [JsonPropertyName("id")]
    public string ID { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public ItemDetails Type { get; set; } = new ItemDetails();

    [JsonPropertyName("rarity")]
    public ItemDetails Rarity { get; set; } = new ItemDetails();

    [JsonPropertyName("series")]
    public SeriesInfo? Series { get; set; }

    [JsonPropertyName("set")]
    public ItemSet? Set { get; set; }

    [JsonPropertyName("introduction")]
    public IntroductionInfo? Introduction { get; set; }

    [JsonPropertyName("images")]
    public ItemImages Images { get; set; } = new ItemImages();

    [JsonPropertyName("variants")]
    public List<VariantInfo>? Variants { get; set; }

    [JsonPropertyName("searchTags")]
    public List<string>? SearchTags { get; set; }

    [JsonPropertyName("gameplayTags")]
    public List<string>? GameplayTags { get; set; }

    [JsonPropertyName("metaTags")]
    public List<string>? MetaTags { get; set; }

    [JsonPropertyName("showcaseVideo")]
    public string? ShowcaseVideo { get; set; }

    [JsonPropertyName("dynamicPakId")]
    public string? DynamicPakId { get; set; }

    [JsonPropertyName("displayAssetPath")]
    public string? DisplayAssetPath { get; set; }

    [JsonPropertyName("definitionPath")]
    public string? DefinitionPath { get; set; }

    [JsonPropertyName("path")]
    public string Path { get; set; } = string.Empty;

    [JsonPropertyName("added")]
    public string Added { get; set; } = string.Empty;

    [JsonPropertyName("shopHistory")]
    public List<string>? ShopHistory { get; set; }
    [JsonPropertyName("itemPreviewHeroPath")]
    public string? ItemPreviewHeroPath { get; set; }
}

public class SeriesInfo
{
    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;

    [JsonPropertyName("image")]
    public string Image { get; set; } = string.Empty;

    [JsonPropertyName("backendValue")]
    public string BackendValue { get; set; } = string.Empty;
}

public class IntroductionInfo
{
    [JsonPropertyName("chapter")]
    public string Chapter { get; set; } = string.Empty;

    [JsonPropertyName("season")]
    public string Season { get; set; } = string.Empty;

    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;

    [JsonPropertyName("backendValue")]
    public int BackendValue { get; set; }
}

public class VariantInfo
{
    [JsonPropertyName("channel")]
    public string Channel { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("options")]
    public List<VariantOption> Options { get; set; } = new List<VariantOption>();
}

public class VariantOption
{
    [JsonPropertyName("tag")]
    public string Tag { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("image")]
    public string Image { get; set; } = string.Empty;
}
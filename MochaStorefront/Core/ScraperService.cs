using System.Text.RegularExpressions;
using HtmlAgilityPack;
using MochaStorefront.Core.Models;

namespace MochaStorefront.Core;

public static class ScraperService
{
    private static readonly HttpClient _httpClient = new() { Timeout = TimeSpan.FromSeconds(30) };

    static ScraperService()
    {
        _httpClient.DefaultRequestHeaders.Add("User-Agent",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:137.0) Gecko/20100101 Firefox/137.0");
    }

    public static async Task<Dictionary<string, List<Item>>> GetStorefrontAsync(string url, Dictionary<string, KnownItem> knownItems)
    {
        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var html = await response.Content.ReadAsStringAsync();
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var shopData = new Dictionary<string, List<Item>>();
        var seenItems = new HashSet<string>();
        var position = 0;

        GenerateShopData(doc, shopData, knownItems, seenItems, ref position);

        foreach (var section in shopData.Keys.ToList())
            shopData[section] = shopData[section].OrderBy(item => item.Position).ToList();

        return shopData;
    }

    private static void GenerateShopData(HtmlDocument doc, Dictionary<string, List<Item>> shopData,
        Dictionary<string, KnownItem> knownItems, HashSet<string> seen, ref int position)
    {
        var sections = doc.DocumentNode.SelectNodes("//h2[contains(@class, 'shop-section-title')]");

        if (sections != null)
        {
            foreach (var sectionTitle in sections)
            {
                var category = ParseSectionTitle(sectionTitle.InnerText);
                var itemsContainer = sectionTitle.SelectSingleNode("following-sibling::div[1]");

                if (itemsContainer == null) continue;

                shopData.TryAdd(category, new List<Item>());

                var itemElements = itemsContainer.SelectNodes(".//a[contains(@class, 'item-display')]");
                if (itemElements == null) continue;

                foreach (var itemElement in itemElements)
                {
                    var item = GenerateItem(itemElement, category, knownItems, seen, ref position);
                    if (item != null) shopData[category].Add(item);
                }
            }
        }

        if (shopData.Count == 0) GenerateFallbackItems(doc, shopData, knownItems, seen, ref position);
    }

    private static void GenerateFallbackItems(HtmlDocument doc, Dictionary<string, List<Item>> shopData,
        Dictionary<string, KnownItem> knownItems, HashSet<string> seen, ref int position)
    {
        var currentSection = "Featured";
        shopData[currentSection] = new List<Item>();

        var allItemLinks = doc.DocumentNode.SelectNodes("//a[contains(@class, 'item-display')]");
        if (allItemLinks == null) return;

        foreach (var itemLink in allItemLinks)
        {
            var item = GenerateItem(itemLink, currentSection, knownItems, seen, ref position);
            if (item != null) shopData[currentSection].Add(item);
        }
    }

    private static Item? GenerateItem(HtmlNode itemElement, string section, Dictionary<string, KnownItem> knownItems, HashSet<string> seen, ref int position)
    {
        var nameElement = itemElement.SelectSingleNode(".//h4[contains(@class, 'item-name')]//span");
        var rawName = nameElement?.InnerText?.Trim() ?? "";

        if (string.IsNullOrEmpty(rawName) || rawName == "Unknown Item") return null;

        var isBundle = rawName.Contains("(Bundle)");
        var name = isBundle ? rawName.Replace("(Bundle)", "").Trim() : rawName;
        var itemKey = $"{section}-{name}";

        if (seen.Contains(itemKey) && !name.Contains("Battle Pass")) return null;
        seen.Add(itemKey);
        position++;

        var price = ExtractPrice(itemElement);
        var href = itemElement.GetAttributeValue("href", "");
        var parts = href.Trim('/').Split('/');
        var itemType = parts.Length >= 2 ? parts[0] : "";

        if (price <= 0) price = CalculateFallbackPrice(itemElement, name, knownItems);

        var item = BuildItem(itemType, name, rawName, isBundle, section, price, position, knownItems);
        return item ?? (name == "Battle Pass Tiers" ? CreateBattlePassItem(itemType, name, section, price, position) : null);
    }

    private static int CalculateFallbackPrice(HtmlNode itemElement, string name, Dictionary<string, KnownItem> knownItems)
    {
        if (!knownItems.TryGetValue(name, out var knownItem)) return 0;

        var backendType = knownItem.Item?.BackendValue ?? "";
        if (string.IsNullOrEmpty(backendType)) return 0;

        var classAttribute = itemElement.GetAttributeValue("class", "");
        var rarityMatch = Regex.Match(classAttribute, @"rarity-(\w+)");
        return rarityMatch.Success ? GetPriceByRarity(backendType, rarityMatch.Groups[1].Value) : 0;
    }

    private static Item? BuildItem(string itemType, string name, string fullName, bool isBundle, string section, int price, int position, Dictionary<string, KnownItem> knownItems)
    {
        if (!knownItems.TryGetValue(name, out var knownItemData)) return null;

        return new Item
        {
            Type = itemType,
            ID = knownItemData.ID ?? "",
            Name = name,
            FullName = fullName,
            IsBundle = isBundle,
            Price = price,
            Position = position,
            Category = knownItemData.Set?.Value ?? "",
            Set = knownItemData.Set != null ? new ItemSet
            {
                Value = knownItemData.Set.Value ?? "",
                Text = knownItemData.Set.Text ?? "",
                BackendValue = knownItemData.Set.BackendValue ?? ""
            } : null,
            ItemDetail = knownItemData.Item != null ? new ItemDetails
            {
                Value = knownItemData.Item.Value ?? "",
                Text = knownItemData.Item.Text ?? "",
                BackendValue = knownItemData.Item.BackendValue ?? ""
            } : new ItemDetails(),
            Images = knownItemData.Images != null ? new ItemImages
            {
                Icon = knownItemData.Images.Icon ?? "",
                SmallIcon = knownItemData.Images.SmallIcon ?? ""
            } : new ItemImages()
        };
    }

    private static Item CreateBattlePassItem(string itemType, string name, string section, int price, int position) => new()
    {
        Type = itemType,
        ID = "AthenaBattlePassTier",
        Name = name,
        FullName = name,
        IsBundle = false,
        Price = price,
        Position = position,
        Category = section,
        ItemDetail = new ItemDetails(),
        Images = new ItemImages()
    };

    private static string ParseSectionTitle(string raw)
    {
        var section = raw.Trim().Replace(" Items", "").Replace(" ITEMS", "");
        return string.IsNullOrEmpty(section) ? "Featured" : section;
    }

    private static int ExtractPrice(HtmlNode element)
    {
        var priceElement = element.SelectSingleNode(".//p[contains(@class, 'item-price')]");
        var priceText = priceElement?.InnerText?.Trim() ?? "";
        var match = Regex.Match(priceText, @"[0-9,]+");
        return match.Success && int.TryParse(match.Value.Replace(",", ""), out var price) ? price : 0;
    }

    private static int GetPriceByRarity(string backendType, string rarity)
    {
        var lowerRarity = rarity.ToLower();
        return backendType switch
        {
            "AthenaCharacter" => lowerRarity switch
            {
                "uncommon" => 800,
                "rare" => 1200,
                "epic" => 1500,
                "legendary" => 2000,
                _ => 0
            },
            "AthenaBackpack" => 500,
            "AthenaPickaxe" => lowerRarity switch
            {
                "uncommon" => 500,
                "rare" => 800,
                "epic" => 1200,
                _ => 0
            },
            "AthenaGlider" => lowerRarity switch
            {
                "uncommon" => 500,
                "rare" => 800,
                "epic" => 1200,
                "legendary" => 1500,
                _ => 0
            },
            "AthenaItemWrap" => lowerRarity switch
            {
                "uncommon" => 300,
                "rare" => 500,
                "epic" => 500,
                _ => 0
            },
            "AthenaSkyDiveContrail" => lowerRarity switch
            {
                "uncommon" => 200,
                "rare" => 500,
                "epic" => 800,
                _ => 0
            },
            "AthenaDance" => lowerRarity switch
            {
                "uncommon" => 200,
                "rare" => 500,
                "epic" => 800,
                "legendary" => 1000,
                _ => 0
            },
            _ => 0
        };
    }
}
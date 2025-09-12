using System.Text.RegularExpressions;
using HtmlAgilityPack;
using MochaStorefront.Core.Models;

namespace MochaStorefront.Core;

public static class ScraperService
{
    private static readonly HttpClient _httpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(30)
    };

    static ScraperService()
    {
        _httpClient.DefaultRequestHeaders.Add("User-Agent",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:137.0) Gecko/20100101 Firefox/137.0");
    }

    public static async Task<Dictionary<string, List<Item>>> GetStorefrontAsync(string url, Dictionary<string, KnownItem> knownItems)
    {
        try
        {
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var html = await response.Content.ReadAsStringAsync();
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var shopData = new Dictionary<string, List<Item>>();
            var seenItems = new HashSet<string>();
            var position = 0;

            var sectionTitleElements = doc.DocumentNode.SelectNodes("//h2[contains(@class, 'shop-section-title')]");
            
            if (sectionTitleElements != null)
            {
                foreach (var titleElement in sectionTitleElements)
                {
                    var sectionName = ParseSectionTitle(titleElement.InnerText);
                    
                    var itemsContainer = titleElement.SelectSingleNode("following-sibling::div[contains(@class, 'items-row')][1]");
                    
                    if (itemsContainer != null)
                    {
                        if (!shopData.ContainsKey(sectionName))
                        {
                            shopData[sectionName] = new List<Item>();
                        }

                        var itemLinks = itemsContainer.SelectNodes(".//a[contains(@class, 'item-display')]");
                        if (itemLinks != null)
                        {
                            foreach (var itemLink in itemLinks)
                            {
                                var item = ExtractItem(itemLink, sectionName, knownItems, seenItems, ref position);
                                if (item != null)
                                {
                                    shopData[sectionName].Add(item);
                                }
                            }
                        }
                    }
                }
            }

            if (shopData.Count == 0)
            {
                var currentSection = "Featured";
                shopData[currentSection] = new List<Item>();
                
                var allItemLinks = doc.DocumentNode.SelectNodes("//a[contains(@class, 'item-display')]");
                if (allItemLinks != null)
                {
                    foreach (var itemLink in allItemLinks)
                    {
                        var item = ExtractItem(itemLink, currentSection, knownItems, seenItems, ref position);
                        if (item != null)
                        {
                            shopData[currentSection].Add(item);
                        }
                    }
                }
            }

            foreach (var section in shopData.Keys.ToList())
            {
                shopData[section] = shopData[section].OrderBy(item => item.Position).ToList();
            }

            return shopData;
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to scrape storefront: {ex.Message}", ex);
        }
    }

    private static string ParseSectionTitle(string raw)
    {
        var section = raw.Trim();
        section = section.Replace(" Items", "").Replace(" ITEMS", "");
        return string.IsNullOrEmpty(section) ? "Featured" : section;
    }

    private static Item? ExtractItem(HtmlNode itemElement, string section, Dictionary<string, KnownItem> knownItems, HashSet<string> seen, ref int position)
    {
        var href = itemElement.GetAttributeValue("href", "");
        if (string.IsNullOrEmpty(href) || !href.StartsWith("/"))
            return null;

        var parts = href.Trim('/').Split('/');
        if (parts.Length < 2)
            return null;

        var itemType = parts[0];
        var nameElement = itemElement.SelectSingleNode(".//h4[contains(@class, 'item-name')]//span");
        var name = nameElement?.InnerText?.Trim() ?? "";

        if (string.IsNullOrEmpty(name) || name == "Unknown Item")
            return null;

        var itemKey = $"{section}-{name}";
        if (seen.Contains(itemKey) && !name.Contains("Battle Pass"))
            return null;
        
        seen.Add(itemKey);
        position++;

        var price = ExtractPrice(itemElement);
        var backendType = "";
        var rarity = "";

        var classAttribute = itemElement.GetAttributeValue("class", "");
        var rarityMatch = Regex.Match(classAttribute, @"rarity-(\w+)");
        if (rarityMatch.Success)
        {
            rarity = rarityMatch.Groups[1].Value;
        }

        if (price <= 0 && knownItems.TryGetValue(name, out var knownItem))
        {
            backendType = knownItem.Item?.BackendValue ?? "";
            
            if (price <= 0 && !string.IsNullOrEmpty(backendType) && !string.IsNullOrEmpty(rarity))
            {
                // Fall back if the price is 0, Ehem speaking to you codename elf
                price = GetPriceByRarity(backendType, rarity);
            }
        }

        if (price <= 0)
            price = 0;

        var item = new Item
        {
            Type = itemType,
            ID = "",
            Name = name,
            Price = price,
            Position = position,
            Category = "",
            ItemDetail = new ItemDetails(),
            Images = new ItemImages()
        };

        if (knownItems.TryGetValue(name, out var knownItemData))
        {
            item.ID = knownItemData.ID;
            item.Category = knownItemData.Set?.Value ?? "";
            
            if (knownItemData.Set != null)
            {
                item.Set = new ItemSet
                {
                    Value = knownItemData.Set.Value ?? "",
                    Text = knownItemData.Set.Text ?? "",
                    BackendValue = knownItemData.Set.BackendValue ?? ""
                };
            }
            
            if (knownItemData.Item != null)
            {
                item.ItemDetail = new ItemDetails
                {
                    Value = knownItemData.Item.Value ?? "",
                    Text = knownItemData.Item.Text ?? "",
                    BackendValue = knownItemData.Item.BackendValue ?? ""
                };
            }
            
            if (knownItemData.Images != null)
            {
                item.Images = new ItemImages
                {
                    Icon = knownItemData.Images.Icon ?? "",
                    SmallIcon = knownItemData.Images.SmallIcon ?? ""
                };
            }
        }
        else if (name == "Battle Pass Tiers")
        {
            item.ID = "AthenaBattlePassTier";
        }
        else
        {
            return null;
        }

        return item;
    }

    private static int ExtractPrice(HtmlNode element)
    {
        var priceElement = element.SelectSingleNode(".//p[contains(@class, 'item-price')]");
        if (priceElement == null)
            return 0;

        var priceText = priceElement.InnerText?.Trim() ?? "";
        if (string.IsNullOrEmpty(priceText))
            return 0;

        var regex = new Regex(@"[0-9,]+");
        var match = regex.Match(priceText);
        
        if (!match.Success)
            return 0;

        var priceDigits = match.Value.Replace(",", "");
        
        if (int.TryParse(priceDigits, out var price))
            return price;

        return 0;
    }

    private static int GetPriceByRarity(string backendType, string rarity)
    {
        switch (backendType)
        {
            case "AthenaCharacter":
                switch (rarity.ToLower())
                {
                    case "uncommon": return 800;
                    case "rare": return 1200;
                    case "epic": return 1500;
                    case "legendary": return 2000;
                    default: return 0;
                }

            case "AthenaBackpack": 
                return 500; 

            case "AthenaPickaxe":
                switch (rarity.ToLower())
                {
                    case "uncommon": return 500;
                    case "rare": return 800;
                    case "epic": return 1200;
                    default: return 0;
                }

            case "AthenaGlider":
                switch (rarity.ToLower())
                {
                    case "uncommon": return 500;
                    case "rare": return 800;
                    case "epic": return 1200;
                    case "legendary": return 1500;
                    default: return 0;
                }

            case "AthenaItemWrap": 
                switch (rarity.ToLower())
                {
                    case "uncommon": return 300;
                    case "rare": return 500;
                    case "epic": return 500; 
                    default: return 0;
                }

            case "AthenaSkyDiveContrail": 
                switch (rarity.ToLower())
                {
                    case "uncommon": return 200;
                    case "rare": return 500;
                    case "epic": return 800;
                    default: return 0;
                }

            case "AthenaDance":
                switch (rarity.ToLower())
                {
                    case "uncommon": return 200;
                    case "rare": return 500;
                    case "epic": return 800;
                    case "legendary": return 1000; 
                    default: return 0;
                }

            default:
                return 0;
        }
    }
}
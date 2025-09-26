using FragmentBackend.Database.Tables.Fortnite;
using MochaStorefront.Core.Generation;
using MochaStorefront.Core.Models;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace MochaStorefront.Core.Generation;

public class StorefrontGenerator
{
    private static readonly DateTime TodayAtMidnightUTC = DateTime.UtcNow.Date;
    private static readonly DateTime TomorrowUTC = TodayAtMidnightUTC.AddDays(1);
    private static readonly string IsoDate = TomorrowUTC.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");

    private static readonly SemaphoreSlim _featuredStorefrontLock = new(1, 1);
    private static readonly SemaphoreSlim _dailyStorefrontLock = new(1, 1);
    private static readonly SemaphoreSlim _unknownStorefrontLock = new(1, 1);

    public static async Task GenerateDailyStorefront(Storefront storefront, int year, int month, int day)
    {
        await _dailyStorefrontLock.WaitAsync();
        try
        {
            Console.WriteLine($"[{DateTime.UtcNow:HH:mm:ss}] Generating daily storefront");
            var seenItems = new HashSet<string>();
            var categories = new HashSet<string>();
            var postgres = new PostgresService();
            int count = 0;
            int analyticOfferGroupId = 6;

            foreach (var data in Constants.Storefront)
            {
                if (data.Key != "Daily") continue;

                foreach (var item in data.Value.Take(6))
                {
                    if (seenItems.Contains(item.ID)) continue;

                    var entry = CreateShopEntry(item, "Daily", year, month, day);
                    if (entry != null)
                    {
                        if (!string.IsNullOrEmpty(item.Category) && !categories.Contains(item.Category))
                            categories.Add(item.Category);

                        await postgres.CreateCatalogAsync(new FragmentBackend.Database.Tables.Fortnite.Catalog
                        {
                            CreatedAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                            OfferId = entry.OfferId,
                            Name = item.Name,
                            Category = item.Category,
                            Storefront = storefront.Name,
                            Value = JsonSerializer.Serialize(entry),
                            TemplateId = item.ID
                        });

                        storefront.CatalogEntries.Add(entry);
                        count++;
                        seenItems.Add(item.ID);

                        Console.WriteLine($"[{DateTime.UtcNow:HH:mm:ss}] Created daily item: {item.Name} (ID: {item.ID}, Price: {item.Price}, AnalyticGroup: {entry.Meta.AnalyticOfferGroupId})");
                    }
                }
            }

            Console.WriteLine($"[{DateTime.UtcNow:HH:mm:ss}] Generated {count} daily items for the storefront");
        }
        finally
        {
            _dailyStorefrontLock.Release();
        }
    }

    public static async Task GenerateFeaturedStorefront(Storefront storefront, int year, int month, int day)
    {
        await _featuredStorefrontLock.WaitAsync();
        try
        {
            Console.WriteLine($"[{DateTime.UtcNow:HH:mm:ss}] Generating featured storefront");
            var seenItems = new HashSet<string>();
            var postgres = new PostgresService();

            int count = 0;

            foreach (var data in Constants.Storefront)
            {
                if (data.Key != "Featured") continue;

                foreach (var item in data.Value)
                {
                    if (seenItems.Contains(item.ID)) continue;

                    var entry = CreateShopEntry(item, "Featured", year, month, day);
                    if (entry != null)
                    {
                        await postgres.CreateCatalogAsync(new FragmentBackend.Database.Tables.Fortnite.Catalog
                        {
                            CreatedAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                            OfferId = entry.OfferId,
                            Name = item.Name,
                            Category = item.Category,
                            Storefront = storefront.Name,
                            Value = JsonSerializer.Serialize(entry),
                            TemplateId = item.ID
                        });

                        storefront.CatalogEntries.Add(entry);
                        count++;
                        seenItems.Add(item.ID);
                        Console.WriteLine($"[{DateTime.UtcNow:HH:mm:ss}] Created Featured item: {item.Name} (ID: {item.ID}, Price: {item.Price})");
                    }
                }
            }

            Console.WriteLine($"[{DateTime.UtcNow:HH:mm:ss}] Generated {count} featured items for the storefront");
        }
        finally
        {
            _featuredStorefrontLock.Release();
        }
    }

    public static async Task GenerateUnknownStorefront(Storefront storefront, string storefrontName, int year, int month, int day)
    {
        await _unknownStorefrontLock.WaitAsync();
        try
        {
            Console.WriteLine($"[{DateTime.UtcNow:HH:mm:ss}] Generating {storefrontName} storefront");
            var seenItems = new HashSet<string>();
            var postgres = new PostgresService();
            var categories = new HashSet<string>();
            int count = 0;

            foreach (var data in Constants.Storefront)
            {
                if (data.Key != storefrontName) continue;

                foreach (var item in data.Value)
                {
                    if (seenItems.Contains(item.ID)) continue;

                    var entry = CreateShopEntry(item, "Featured", year, month, day, storefrontName);
                    if (entry != null)
                    {
                        if (!string.IsNullOrEmpty(item.Category) && !categories.Contains(item.Category))
                            categories.Add(item.Category);

                        await postgres.CreateCatalogAsync(new FragmentBackend.Database.Tables.Fortnite.Catalog
                        {
                            CreatedAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                            OfferId = entry.OfferId,
                            Name = item.Name,
                            Category = item.Category,
                            Storefront = storefront.Name,
                            Value = JsonSerializer.Serialize(entry),
                            TemplateId = item.ID
                        });

                        storefront.CatalogEntries.Add(entry);
                        count++;
                        seenItems.Add(item.ID);

                        Console.WriteLine($"[{DateTime.UtcNow:HH:mm:ss}] Created {storefrontName} item: {item.Name} (ID: {item.ID}, Price: {item.Price})");
                    }
                }
            }

            if (categories.Any())
            {
                var now = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
                var sections = categories.Select(category => new ShopSections
                {
                    CreatedAt = now,
                    Section = category
                });

                await postgres.CreateShopSectionsAsync(sections.ToList());
            }

            Console.WriteLine($"[{DateTime.UtcNow:HH:mm:ss}] Generated {count} {storefrontName} items for the storefront");
        }
        finally
        {
            _unknownStorefrontLock.Release();
        }
    }

    private static CatalogEntry CreateShopEntry(Item item, string section, int year, int month, int day, string storefrontName = "")
    {
        if (!IsValidItem(item)) return null;

        var isJuly18 = month == 7 && day == 18;
        var price = isJuly18 ? 0 : item.Price;

        var entry = CreateBaseEntry(item, price);
        var itemGrants = new List<string> { item.ID };
        var uniqueIdentifier = GenerateOfferId(itemGrants, price);

        entry.OfferId = uniqueIdentifier;
        entry.DevName = uniqueIdentifier;
        entry.Prices[0].SaleExpiration = IsoDate;

        if (section == "Featured" && !string.IsNullOrEmpty(storefrontName))
        {
            SetupUnknownFeaturedEntry(entry, item, storefrontName);
        }
        else if (section == "Featured")
        {
            SetupFeaturedEntry(entry, item, year);
        }
        else
        {
            SetupDailyEntry(entry, item);
        }

        return entry;
    }

    private static CatalogEntry CreateBaseEntry(Item item, int price)
    {
        string backendValue = item.ID.Split(':')[0];
        if (string.IsNullOrEmpty(backendValue) || backendValue != item.Type)
        {
            backendValue = BackendResolver.DetermineItemBackendValue(item);
            item.ID = $"{backendValue}:{item.ID}";
        }

        return new CatalogEntry
        {
            OfferId = $"v2:/{Guid.NewGuid()}",
            OfferType = "StaticPrice",
            DevName = $"[VIRTUAL] 1x {item.ID} for {price} MtxCurrency",
            ItemGrants = new List<ItemGrant>
            {
                new ItemGrant { TemplateId = item.ID, Quantity = 1 }
            },
            Requirements = new List<Requirement>
            {
                new Requirement
                {
                    RequirementType = "DenyOnItemOwnership",
                    RequiredId = item.ID,
                    MinQuantity = 1
                }
            },
            Categories = new List<string>(),
            MetaInfo = new List<MetaInfo>
            {
                new MetaInfo { Key = "BannerOverride", Value = "" }
            },
            Prices = new List<Price>
            {
                new Price
                {
                    CurrencyType = "MtxCurrency",
                    CurrencySubType = "Currency",
                    RegularPrice = price,
                    DynamicRegularPrice = price,
                    FinalPrice = price,
                    SaleExpiration = IsoDate,
                    BasePrice = price
                }
            },
            GiftInfo = new GiftInfo
            {
                IsEnabled = true,
                ForcedGiftBoxTemplateId = "",
                PurchaseRequirements = new List<Requirement>(),
                GiftRecordIds = new List<string>()
            },
            Meta = new Meta
            {
                TemplateId = item.ID,
                NewDisplayAssetPath = "",
                DisplayAssetPath = "",
                SectionId = "Daily",
                TileSize = "Small",
                LayoutId = "Daily.99",
                AnalyticOfferGroupId = "Daily"
            },
            DisplayAssetPath = "",
            NewDisplayAssetPath = "",
            Refundable = true,
            Title = "",
            Description = "",
            ShortDescription = "",
            AppStoreIds = new List<string>(),
            FulfillmentIds = new List<object>(),
            DailyLimit = -1,
            WeeklyLimit = -1,
            MonthlyLimit = -1,
            SortPriority = 0,
            CatalogGroupPriority = 0,
            FilterWeight = 0,
            BannerOverride = "",
            MatchFilter = "",
            AdditionalGrants = new List<ItemGrant>()
        };
    }

    private static void SetupFeaturedEntry(CatalogEntry entry, Item item, int year)
    {
        entry.Meta.SectionId = "Featured";
        entry.Meta.TileSize = "Normal";
        entry.Meta.LayoutId = "Featured.99";
        entry.Meta.AnalyticOfferGroupId = "Featured";
    }

    private static void SetupDailyEntry(CatalogEntry entry, Item item /* int analyticOfferGroupId */)
    {
        entry.Meta.SectionId = "Daily";
        entry.Meta.TileSize = item.ID.ToLower().Contains("cid") ? "Normal" : "Small";
        entry.Meta.LayoutId = "Daily.99";
        entry.Meta.AnalyticOfferGroupId = "Daily";
        entry.Categories.Clear(); 
    }

    private static void SetupUnknownFeaturedEntry(CatalogEntry entry, Item item, string storefrontName)
    {
        var sanitizedStorefrontName = storefrontName.Replace(" ", "");

        entry.Meta.SectionId = sanitizedStorefrontName;
        entry.Meta.TileSize = "Normal";
        entry.Meta.LayoutId = $"{sanitizedStorefrontName}.99";
        entry.Meta.AnalyticOfferGroupId = sanitizedStorefrontName;

        if (!string.IsNullOrEmpty(item.Set?.Value))
            entry.Categories.Add(item.Set.Value);
    }

    private static string GenerateOfferId(List<string> itemGrants, int price)
    {
        var input = $"{string.Join("", itemGrants)}_{price}";
        using var sha1 = SHA1.Create();
        var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(input));
        return BitConverter.ToString(hash).Replace("-", "").ToLower();
    }

    private static bool IsValidItem(Item item)
    {
        if (string.IsNullOrEmpty(item.Name) || string.IsNullOrEmpty(item.ID))
            return false;

        if (item.Price <= 0 && item.Name != "Heartspan" && item.ID != "MusicPack_LavaChicken")
            return false;

        return true;
    }

    public static async Task GenerateFutureUnknownStorefront(Storefront weeklySection, string key, int year, int month, int day, List<FutureShops> futureShops)
    {
        throw new NotImplementedException();
    }

    public static async Task GenerateFutureFeaturedStorefront(Storefront weeklySection, int year, int month, int day, List<FutureShops> futureShops)
    {
        throw new NotImplementedException();
    }

    public static async Task GenerateFutureDailyStorefront(Storefront dailySection, int year, int month, int day, List<FutureShops> futureShops)
    {
        throw new NotImplementedException();
    }
}
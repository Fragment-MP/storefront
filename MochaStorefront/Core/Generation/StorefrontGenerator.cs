using FragmentBackend.Database.Tables.Fortnite;
using FragmentBackend.Database.Tables.Profiles;
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

    private static readonly HashSet<string> _processedOfferIds = new();
    private static readonly object _processedLock = new object();

    public static async Task GenerateDailyStorefront(Storefront storefront, int year, int month, int day)
    {
        await _dailyStorefrontLock.WaitAsync();
        try
        {
            Console.WriteLine($"[{DateTime.UtcNow:HH:mm:ss}] Generating daily storefront");
            var postgres = new PostgresService();
            int count = 0;
            int analyticOfferGroupId = 0;

            storefront.CatalogEntries.RemoveAll(entry => entry.Meta.SectionId == "Daily");

            foreach (var data in Constants.Storefront)
            {
                if (data.Key != "Daily") continue;

                var items = data.Value.Where(IsValidItem).GroupBy(item => item.ID).Select(group => group.First()).ToList();
                foreach (var item in items)
                {
                    var entry = CreateShopEntry(item, "Daily", year, month, day, "", analyticOfferGroupId);
                    if (entry != null && !IsDuplicateOffer(entry.OfferId))
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
                        MarkOfferAsProcessed(entry.OfferId);
                        count++;
                        analyticOfferGroupId++;
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
            var postgres = new PostgresService();
            int count = 0;
            int analyticOfferGroupId = 0;

            storefront.CatalogEntries.RemoveAll(entry => entry.Meta.SectionId == "Featured");

            foreach (var data in Constants.Storefront)
            {
                if (data.Key != "Featured") continue;

                var items = data.Value.Where(IsValidItem).GroupBy(item => item.ID).Select(group => group.First()).ToList();

                foreach (var item in items)
                {
                    var entry = CreateShopEntry(item, "Featured", year, month, day, "", analyticOfferGroupId);
                    if (entry != null && !IsDuplicateOffer(entry.OfferId))
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
                        MarkOfferAsProcessed(entry.OfferId);
                        count++;
                        analyticOfferGroupId++;
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
            var postgres = new PostgresService();
            int count = 0;
            int analyticOfferGroupId = 0;

            storefront.CatalogEntries.RemoveAll(entry => entry.Meta.SectionId == storefrontName);

            var categories = new HashSet<string>();
            var test = new HashSet<string>();

            var itemsList = new List<Item>();

            foreach (var data in Constants.Storefront)
            {
                if (data.Key != storefrontName) continue;

                itemsList = data.Value.Where(IsValidItem).GroupBy(item => item.ID).Select(group => group.First()).ToList();

                foreach (var item in itemsList)
                {
                    var entry = CreateShopEntry(item, "Featured", year, month, day, storefrontName, analyticOfferGroupId);
                    if (entry != null && !IsDuplicateOffer(entry.OfferId))
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
                        MarkOfferAsProcessed(entry.OfferId);
                        count++;
                        analyticOfferGroupId++;

                        if (!string.IsNullOrEmpty(item.Category))
                            categories.Add(item.Category);

                        if (!string.IsNullOrEmpty(storefrontName))
                            test.Add(storefrontName);
                    }
                }
            }

            if (categories.Count > 0)
            {
                var now = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
                var sections = test.Select(category => new ShopSections
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

    private static CatalogEntry CreateShopEntry(Item item, string section, int year, int month, int day, string storefrontName = "", int analyticOfferGroupId = 0)
    {
        if (!IsValidItem(item)) return null;

        var isJuly18 = month == 7 && day == 18;
        var price = isJuly18 ? 0 : item.Price;

        var entry = CreateBaseEntry(item, price);
        var uniqueIdentifier = GenerateOfferId(item, section, storefrontName, year, month, day);

        entry.OfferId = uniqueIdentifier;
        entry.Prices[0].SaleExpiration = IsoDate;

        if (section == "Featured" && !string.IsNullOrEmpty(storefrontName))
        {
            SetupUnknownFeaturedEntry(entry, item, storefrontName, analyticOfferGroupId);
        }
        else if (section == "Featured")
        {
            SetupFeaturedEntry(entry, item, year, analyticOfferGroupId);
        }
        else
        {
            SetupDailyEntry(entry, item, analyticOfferGroupId);
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
            MetaInfo = new List<MetaInfo>(),
            Prices = new List<Price>
            {
                new Price
                {
                    CurrencyType = "MtxCurrency",
                    CurrencySubType = "",
                    RegularPrice = price,
                    DynamicRegularPrice = price,
                    FinalPrice = price,
                    SaleExpiration = "9999-12-31T23:59:59.999Z",
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
                NewDisplayAssetPath = "",
                SectionId = "Daily",
                TileSize = "Small",
                AnalyticOfferGroupId = "0"
            },
            DisplayAssetPath = "",
            Refundable = true,
            AppStoreIds = new List<string>(),
            FulfillmentIds = new List<object>(),
            DailyLimit = -1,
            WeeklyLimit = -1,
            MonthlyLimit = -1,
            SortPriority = -1,
            CatalogGroupPriority = 0,
            FilterWeight = 0.0,
            MatchFilter = "",
            AdditionalGrants = new List<ItemGrant>()
        };
    }

    private static void SetupFeaturedEntry(CatalogEntry entry, Item item, int year, int featuredAnalyticOfferGroupId)
    {
        string cleanItemId = item.ID.Contains(':') ? item.ID.Split(':')[1] : item.ID;

        entry.Meta.NewDisplayAssetPath = DisplayAssetPathGenerator.GetNewDisplayAssetPath(cleanItemId);
        entry.DisplayAssetPath = DisplayAssetPathGenerator.GetDisplayAssetPath(cleanItemId);

        entry.Meta.SectionId = "Featured";
        entry.Meta.TileSize = "Normal";
        entry.Meta.AnalyticOfferGroupId = featuredAnalyticOfferGroupId.ToString();

        AddMetaInfo(entry, "Featured", item.ID, item.Price.ToString(), featuredAnalyticOfferGroupId);

        if (item.Backpack != null)
        {
            entry.ItemGrants.Add(new ItemGrant { TemplateId = item.ID, Quantity = 1 });
            entry.Requirements.Add(new Requirement
            {
                RequirementType = "DenyOnItemOwnership",
                RequiredId = item.ID,
                MinQuantity = 1
            });
        }
    }

    private static void SetupDailyEntry(CatalogEntry entry, Item item, int analyticOfferGroupId)
    {
        string cleanItemId = item.ID.Contains(':') ? item.ID.Split(':')[1] : item.ID;

        entry.Meta.NewDisplayAssetPath = DisplayAssetPathGenerator.GetNewDisplayAssetPath(cleanItemId);
        entry.DisplayAssetPath = DisplayAssetPathGenerator.GetDisplayAssetPath(cleanItemId);

        entry.Meta.SectionId = "Daily";
        entry.Meta.TileSize = item.ID.ToLower().Contains("cid") ? "Normal" : "Small";
        entry.Meta.AnalyticOfferGroupId = analyticOfferGroupId.ToString();
        entry.Categories.Clear();

        AddMetaInfo(entry, "Daily", item.ID, item.Price.ToString(), analyticOfferGroupId);
    }

    private static void SetupUnknownFeaturedEntry(CatalogEntry entry, Item item, string storefrontName, int analyticOfferGroupId)
    {
        string cleanItemId = item.ID.Contains(':') ? item.ID.Split(':')[1] : item.ID;

        entry.Meta.NewDisplayAssetPath = DisplayAssetPathGenerator.GetNewDisplayAssetPath(cleanItemId);
        entry.DisplayAssetPath = DisplayAssetPathGenerator.GetDisplayAssetPath(cleanItemId);

        entry.Meta.SectionId = storefrontName;
        entry.Meta.TileSize = "Normal";
        entry.Meta.AnalyticOfferGroupId = analyticOfferGroupId.ToString();

        entry.MetaInfo.Add(new MetaInfo { Key = "NewDisplayAssetPath", Value = entry.Meta.NewDisplayAssetPath });
        entry.MetaInfo.Add(new MetaInfo { Key = "SectionId", Value = storefrontName });
        entry.MetaInfo.Add(new MetaInfo { Key = "TileSize", Value = "Normal" });
        entry.MetaInfo.Add(new MetaInfo { Key = "AnalyticOfferGroupId", Value = analyticOfferGroupId.ToString() });

        entry.GiftInfo.PurchaseRequirements.AddRange(entry.Requirements);

        if (!string.IsNullOrEmpty(item.Set?.Value))
            entry.Categories.Add(item.Set.Value);

        if (item.Backpack != null)
        {
            entry.ItemGrants.Add(new ItemGrant { TemplateId = item.ID, Quantity = 1 });
            entry.Requirements.Add(new Requirement
            {
                RequirementType = "DenyOnItemOwnership",
                RequiredId = item.ID,
                MinQuantity = 1
            });
        }
    }

    private static void AddMetaInfo(CatalogEntry entry, string section, string itemId, string price, int analyticOfferGroupId)
    {
        string tileSize = "Small";
        if (section == "Featured" || itemId.ToLower().Contains("cid"))
            tileSize = "Normal";

        entry.MetaInfo.Add(new MetaInfo { Key = "NewDisplayAssetPath", Value = entry.Meta.NewDisplayAssetPath });
        entry.MetaInfo.Add(new MetaInfo { Key = "SectionId", Value = section });
        entry.MetaInfo.Add(new MetaInfo { Key = "TileSize", Value = tileSize });
        entry.MetaInfo.Add(new MetaInfo { Key = "AnalyticOfferGroupId", Value = analyticOfferGroupId.ToString() });

        entry.GiftInfo.PurchaseRequirements.AddRange(entry.Requirements);
    }

    private static string GenerateOfferId(Item item, string section, string storefrontName, int year, int month, int day)
    {
        var input = $"{item.ID}_{section}_{storefrontName}_{year}_{month}_{day}";
        using var sha1 = SHA1.Create();
        var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(input));
        return $"v2:/{BitConverter.ToString(hash).Replace("-", "").ToLower()}";
    }

    private static bool IsValidItem(Item item)
    {
        if (string.IsNullOrEmpty(item.ID))
            return false;

        return true;
    }

    private static bool IsDuplicateOffer(string offerId)
    {
        lock (_processedLock)
        {
            return _processedOfferIds.Contains(offerId);
        }
    }

    private static void MarkOfferAsProcessed(string offerId)
    {
        lock (_processedLock)
        {
            _processedOfferIds.Add(offerId);
        }
    }

    public static void ClearProcessedOffers()
    {
        lock (_processedLock)
        {
            _processedOfferIds.Clear();
        }
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
using MochaStorefront.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MochaStorefront.Core.Generation
{
    public class StorefrontEntry
    {
        private static readonly CategoryTracker _categoryTracker = new();

        private static readonly HashSet<string> UnsupportedTypes = new()
        {
            "AthenaEmoji", "AthenaMusicPack"
        };

        private static readonly Dictionary<string, string> CategoryOverrides = new()
        {
            { "Crystal", "Dino Guard" }
        };

        public static CatalogEntry CreateShopEntry(Item item, string section, bool s14Above = false)
        {
            if (UnsupportedTypes.Contains(item.ItemDetail.BackendValue))
                throw new ArgumentException("This backendValue is not supported");


            if ((item.Price <= 0 || string.IsNullOrEmpty(item.Name)) &&
                item.Name != "Heartspan" && item.ID != "MusicPack_LavaChicken")
                throw new ArgumentException("Invalid item data");

            var entry = CreateBaseEntry(item);

            if (section == "Featured" && !s14Above)
            {
                SetupFeaturedEntry(entry, item);
            }
            else
            {
                SetupRegularEntry(entry, item, section);
            }

            GenerateCategories(entry, item, section);
            AddMetaInfo(entry, section, item.ID);

            return entry;
        }

        private static void GenerateCategories(CatalogEntry entry, Item item, string section)
        {
            if (CategoryOverrides.TryGetValue(item.Name, out string overrideCategory))
            {
                entry.Categories = new List<string> { overrideCategory };
            }
            else if (section == "Daily")
            {
                entry.Categories = new List<string>();
            }
            else
            {
                entry.Categories = new List<string> { item.Category };
            }

            bool isPickaxe = item.ID.ToLower().Contains("pickaxe_");
            bool isCharacter = item.ID.ToLower().Contains("cid_");

            if (isCharacter)
            {
                _categoryTracker.TrackCharacterCategories(item.ID, entry.Categories);
            }

            if (isPickaxe)
            {
                entry.Categories = _categoryTracker.GeneratePickaxeCategories(entry.Categories, item.Set?.Value);
            }
        }

        private static void AddMetaInfo(CatalogEntry entry, string section, string itemId)
        {
            string tileSize = "Small";
            if (section == "Featured" || itemId.ToLower().Contains("cid"))
                tileSize = "Normal";

            entry.MetaInfo.Add(new MetaInfo { Key = "TileSize", Value = tileSize });
            entry.MetaInfo.Add(new MetaInfo { Key = "SectionId", Value = section });

            entry.GiftInfo.PurchaseRequirements.AddRange(entry.Requirements);
        }

        private static void SetupFeaturedEntry(CatalogEntry entry, Item item)
        {
            entry.DisplayAssetPath = DisplayAssetPathGenerator.GetDisplayAssetPath($"DA_Featured_{item.ID}");
            entry.NewDisplayAssetPath = DisplayAssetPathGenerator.GetNewDisplayAssetPath(item.ID);
            entry.Meta.DisplayAssetPath = entry.DisplayAssetPath;
            entry.Meta.NewDisplayAssetPath = entry.NewDisplayAssetPath;
            entry.Meta.SectionId = "Featured";
            entry.Meta.TileSize = "Normal";
            entry.Meta.LayoutId = "Featured.99";
            entry.Meta.AnalyticOfferGroupId = "Featured";
        }

        private static void SetupRegularEntry(CatalogEntry entry, Item item, string section)
        {
            entry.DisplayAssetPath = DisplayAssetPathGenerator.GetDisplayAssetPath($"DA_Featured_{item.ID}");
            entry.NewDisplayAssetPath = DisplayAssetPathGenerator.GetNewDisplayAssetPath(item.ID);
            entry.Meta.DisplayAssetPath = entry.DisplayAssetPath;
            entry.Meta.NewDisplayAssetPath = entry.NewDisplayAssetPath;
            entry.Meta.SectionId = section;
            entry.Meta.TileSize = item.ID.ToLower().Contains("cid") ? "Normal" : "Small";
            entry.Meta.LayoutId = $"{section}.99";
            entry.Meta.AnalyticOfferGroupId = section;
        }

        private static CatalogEntry CreateBaseEntry(Item item)
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
                DevName = $"[VIRTUAL] 1x {item.ID} for {item.Price} MtxCurrency",
                ItemGrants = new List<ItemGrant>
                {
                    new ItemGrant
                    {
                        TemplateId = item.ID,
                        Quantity = 1
                    }
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
                    new MetaInfo
                    {
                        Key = "BannerOverride",
                        Value = ""
                    }
                },
                Prices = new List<Price>
                {
                    new Price
                    {
                        CurrencyType = "MtxCurrency",
                        CurrencySubType = "Currency",
                        RegularPrice = item.Price,
                        DynamicRegularPrice = item.Price,
                        FinalPrice = item.Price,
                        SaleExpiration = "9999-12-31T23:59:59.999Z",
                        BasePrice = item.Price
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
    }
}

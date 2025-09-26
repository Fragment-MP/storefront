using MochaStorefront.Core.Config;
using MochaStorefront.Core.Models;
using System.Text.Json;

namespace MochaStorefront.Core.Generation;

public class StorefrontGenerator
{
    private static readonly object _lock = new();

    public static void GenerateDailyStorefront(Storefront storefront)
    {
        lock (_lock)
        {
            Console.WriteLine($"[{DateTime.UtcNow:HH:mm:ss}] Generating daily storefront");
            var seenItems = new HashSet<string>();
            int count = 0;
            
            foreach (var data in Constants.Storefront)
            {
                if (data.Key != "Daily")
                {
                    count -= 1;
                    continue;
                }

                foreach (var item in data.Value.Take(6))
                {
                    if (seenItems.Contains(item.ID)) 
                        continue;

                    var backendValue = BackendResolver.DetermineItemBackendValue(item);
                    var templateId = $"{backendValue}:{item.ID}";
                    
                    Console.WriteLine($"[{DateTime.UtcNow:HH:mm:ss}] Created daily item: {item.Name} (ID: {item.ID}, Price: {item.Price})");
                    var entry = StorefrontEntry.CreateShopEntry(item, "Daily", ShopConfiguration.CurrentVersion >= 14);

                    storefront.CatalogEntries.Add(entry);
                    count++;
                    seenItems.Add(item.ID);
                }
            }
            
            Console.WriteLine($"[{DateTime.UtcNow:HH:mm:ss}] Generated {count} daily items for the storefront");
        }
    }

    public static void GenerateFeaturedStorefront(Storefront storefront)
    {
        lock (_lock)
        {
            Console.WriteLine($"[{DateTime.UtcNow:HH:mm:ss}] Generating featured storefront");
            var seenItems = new HashSet<string>();
            int count = 0;

            foreach (var data in Constants.Storefront)
            {
                if (data.Key != "Featured")
                {
                    count -= 1;
                    continue;
                }

                foreach (var item in data.Value.Take(6))
                {
                    if (seenItems.Contains(item.ID))
                        continue;

                    var backendValue = BackendResolver.DetermineItemBackendValue(item);
                    var templateId = $"{backendValue}:{item.ID}";

                    Console.WriteLine($"[{DateTime.UtcNow:HH:mm:ss}] Created Featured item: {item.Name} (ID: {item.ID}, Price: {item.Price})");
                    var entry = StorefrontEntry.CreateShopEntry(item, "Featured", ShopConfiguration.CurrentVersion >= 14);

                    storefront.CatalogEntries.Add(entry);
                    count++;
                    seenItems.Add(item.ID);
                }
            }

            Console.WriteLine($"[{DateTime.UtcNow:HH:mm:ss}] Generated {count} featured items for the storefront");
        }
    }
}
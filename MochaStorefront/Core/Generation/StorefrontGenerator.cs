using MochaStorefront.Core.Models;

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
                    count++;
                    seenItems.Add(item.ID);
                }
            }
            
            Console.WriteLine($"[{DateTime.UtcNow:HH:mm:ss}] Generated {count} daily items for the storefront");
        }
    }
}
using MochaStorefront;
using MochaStorefront.Core;
using MochaStorefront.Core.Models;

var knownItems = await Utils.LoadItemsAsync("https://fortnite-api.com/v2/cosmetics/br?responseFlags=4");
if (knownItems == null)
{
    Console.WriteLine("Failed to load items from API. Exiting...");
    return;
}

var result = await ScraperService.GetStorefrontAsync("https://fnbr.co/shop/December-17-2017", knownItems);
foreach (var section in result)
{
    Console.WriteLine($"{section.Key} ({section.Value.Count} items)");
    
    // foreach (var item in section.Value)
    // {
    //     Console.WriteLine($"{item.Name}");
    //     Console.WriteLine($"{item.Price:N0} V-Bucks");
    //     Console.WriteLine($"{item.Type}");
    //     if (!string.IsNullOrEmpty(item.Category))
    //     {
    //         Console.WriteLine($"{item.Category}");
    //     }
    // }
}

Constants.Storefront = result;

var dailySection = StorefrontService.CreateSection("BRDailyStorefront");
var weeklySection = StorefrontService.CreateSection("BRWeeklyStorefront");

var formattedDate = DateTime.Now.ToString("yyyy-MM-dd");
var filename = $"scraped-data-{formattedDate}.json";
await Utils.SaveJsonAsync(filename, result);
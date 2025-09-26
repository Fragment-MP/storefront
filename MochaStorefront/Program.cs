using MochaStorefront;
using MochaStorefront.Core;
using MochaStorefront.Core.Generation;
using MochaStorefront.Core.Models;

var knownItems = await Utils.LoadItemsAsync("https://fortnite-api.com/v2/cosmetics/br?responseFlags=4");
if (knownItems == null)
{
    Console.WriteLine("Failed to load items from API. Exiting...");
    return;
}

var result = await ScraperService.GetStorefrontAsync("https://fnbr.co/shop/October-1-2021", knownItems);


Constants.Storefront = result;

var dailySection = StorefrontService.CreateSection("BRDailyStorefront");
var weeklySection = StorefrontService.CreateSection("BRWeeklyStorefront");

foreach (var section in result)
{
    Console.WriteLine($"{section.Key} ({section.Value.Count} items)");
    var sectionName = section.Key.Contains("Daily") ? "BRDailyStorefront" : section.Key.Contains("Featured") ? "BRWeeklyStorefront" : section.Key;

    switch (sectionName)
    {
        case "BRDailyStorefront":
            StorefrontGenerator.GenerateDailyStorefront(dailySection);
            break;
        case "BRWeeklyStorefront":
            StorefrontGenerator.GenerateFeaturedStorefront(weeklySection);
            break;
        default:
            var unknownSection = StorefrontService.CreateSection(sectionName);
            StorefrontGenerator.GenerateUnknownStorefront(unknownSection, sectionName);
            break;
    }
}

var formattedDate = DateTime.Now.ToString("yyyy-MM-dd");
var filename = $"scraped-data-{formattedDate}.json";
await Utils.SaveJsonAsync(filename, StorefrontService.GetAllSections());
using MochaStorefront.Core.Config;
using MochaStorefront.Core.Generation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MochaStorefront.Core
{
    public class StorefrontManager
    {
        private static DateTime _currentShopDate;
        private readonly PostgresService _database;

        public StorefrontManager(PostgresService database)
        {
            _database = database;
        }

        public async Task InitializeAsync(string[] args)
        {
            if (args.Length > 0 && args[0] == "now")
            {
                await GenerateStorefrontAsync();
                return;
            }

            _ = Task.Run(GenerateStorefrontCycleAsync);
        }

        private async Task GenerateStorefrontCycleAsync()
        {
            while (true)
            {
                var now = DateTime.UtcNow;
                var currentDay = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc);
                var endOfDay = currentDay.AddDays(1);

                var sleepDuration = endOfDay - now;
                if (sleepDuration < TimeSpan.Zero)
                {
                    continue;
                }

                Console.WriteLine($"Sleeping for {sleepDuration}");

                await Task.Delay(sleepDuration);
                await GenerateStorefrontAsync();

                Console.WriteLine("Storefront generated, waiting for the next cycle.");
            }
        }

        private async Task GenerateStorefrontAsync()
        {
            var lastShop = await _database.GetLastProcessedShopAsync();

            if (lastShop == null || string.IsNullOrEmpty(lastShop.Date))
            {
                Console.WriteLine("No last processed shop found. Using configured start date.");
                _currentShopDate = ShopConfiguration.DefaultStartDate;
            }
            else
            {
                Console.WriteLine($"Last processed shop date: {lastShop.Date}");

                if (DateTime.TryParseExact(lastShop.Date, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out var shopDate) &&
                    shopDate != DateTime.MinValue)
                {
                    _currentShopDate = shopDate.AddDays(1);
                }
                else
                {
                    _currentShopDate = new DateTime(2020, 7, 1, 23, 59, 59, DateTimeKind.Utc);
                }
            }

            var stopDate = ShopConfiguration.DefaultStopDate;
            var now = DateTime.Now;

            if (_currentShopDate < stopDate && _currentShopDate < now)
            {
                if (!await _database.IsShopProcessedAsync(_currentShopDate))
                {
                    var futureShops = await _database.GetFutureShopsAsync();
                    Console.WriteLine(futureShops.Count);

                    if (futureShops.Count == 0)
                    {
                        await _database.CreatePastShopAsync(new FragmentBackend.Database.Tables.Fortnite.PastShops
                        {
                            CreatedAt = _currentShopDate.ToString("yyyy-MM-dd"),
                            Date = _currentShopDate.ToString("yyyy-MM-dd")
                        });
                    }

                    await HandleStorefrontAsync(_currentShopDate);

                    Console.WriteLine($"Generated storefront for {_currentShopDate:MMMM-d-yyyy}");
                }
                else
                {
                    Console.WriteLine($"Shop for {_currentShopDate:MMMM-d-yyyy} already processed, skipping");
                }
            }
        }

        private async Task HandleStorefrontAsync(DateTime date)
        {
            await _database.DeleteAllStorefrontsAsync();
            await _database.DeleteAllShopSectionsAsync();

            var formattedDate = date.ToString("MMMM-d-yyyy").ToLower();
            var url = $"https://fnbr.co/shop/{formattedDate}";

            Utils.LogWithTimestamp(ConsoleColor.Cyan, $"Generating storefront for {formattedDate}");

            var items = await Utils.LoadItemsAsync("https://fortnite-api.com/v2/cosmetics/br?responseFlags=5");
            if (items == null)
            {
                Utils.LogWithTimestamp(ConsoleColor.Red, $"Failed to load items for {formattedDate}");
                return;
            }

            var result = await ScraperService.GetStorefrontAsync(url, items);
            if (result == null)
            {
                Utils.LogWithTimestamp(ConsoleColor.Red, $"Failed to scrape data for {formattedDate}");
                return;
            }

            lock (Constants.StorefrontLock)
            {
                Constants.Storefront = result;
            }

            var dailySection = StorefrontService.CreateSection("BRDailyStorefront");
            var weeklySection = StorefrontService.CreateSection("BRWeeklyStorefront");

            int year = date.Year;
            int month = date.Month;
            int day = date.Day;
            var futureShops = await _database.GetFutureShopsAsync();
            bool hasFuture = futureShops.Count > 0;

            if (hasFuture)
            {
                Utils.LogWithTimestamp(ConsoleColor.Yellow,
                    $"found {futureShops.Count} future shop items, generating them");
            }

            var tasks = new List<Task>();

            foreach (var section in result)
            {
                Console.WriteLine($"{section.Key} ({section.Value.Count} items)");
                foreach (var test in section.Value)
                {
                    Console.WriteLine($" - {test.Name} ({test.Type})");
                }

                Task task = section.Key switch
                {
                    "Daily" => hasFuture
                        ? StorefrontGenerator.GenerateFutureDailyStorefront(dailySection, year, month, day, futureShops)
                        : StorefrontGenerator.GenerateDailyStorefront(dailySection, year, month, day),

                    "Featured" => hasFuture
                        ? StorefrontGenerator.GenerateFutureFeaturedStorefront(weeklySection, year, month, day, futureShops)
                        : StorefrontGenerator.GenerateFeaturedStorefront(weeklySection, year, month, day),

                    _ => hasFuture
                        ? StorefrontGenerator.GenerateFutureUnknownStorefront(weeklySection, section.Key, year, month, day, futureShops)
                        : StorefrontGenerator.GenerateUnknownStorefront(weeklySection, section.Key, year, month, day)
                };

                tasks.Add(task);
            }

            await Task.WhenAll(tasks);

            Utils.LogWithTimestamp(ConsoleColor.Green, $"Finished generating storefront for {formattedDate}");
            var filename = $"scraped-data-{formattedDate}.json";
            await Utils.SaveJsonAsync(filename, StorefrontService.GetAllSections());

            await _database.DeleteAllFutureShopsAsync();
        }
    }
}

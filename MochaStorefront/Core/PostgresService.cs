using FragmentBackend.Database;
using FragmentBackend.Database.Tables.Account;
using FragmentBackend.Database.Tables.AntiCheat;
using FragmentBackend.Database.Tables.CloudStorage;
using FragmentBackend.Database.Tables.Fortnite;
using FragmentBackend.Database.Tables.Profiles;
using FragmentBackend.Database.Tables.Tournaments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MochaStorefront.Core
{
    public class PostgresService
    {
        private readonly Lazy<Repository<Catalog>> _lazyCatalogRepo;
        private readonly Lazy<Repository<ShopSections>> _lazyShopSectionsRepo;
        private readonly Lazy<Repository<PastShops>> _lazyPastShopsRepo;
        private readonly Lazy<Repository<FutureShops>> _lazyFutureShopsRepo;

        private static readonly object _initLock = new();
        private static bool _isRepositoryManagerInitialized;

        private Repository<Catalog> _catalogRepo => _lazyCatalogRepo.Value;
        private Repository<ShopSections> _shopSectionsRepo => _lazyShopSectionsRepo.Value;
        private Repository<PastShops> _pastShopsRepo => _lazyPastShopsRepo.Value;
        private Repository<FutureShops> _futureShopsRepo => _lazyFutureShopsRepo.Value;

        public PostgresService()
        {
            _lazyCatalogRepo = new Lazy<Repository<Catalog>>(() => RepositoryManager.Get<Catalog>());
            _lazyShopSectionsRepo = new Lazy<Repository<ShopSections>>(() => RepositoryManager.Get<ShopSections>());
            _lazyPastShopsRepo = new Lazy<Repository<PastShops>>(() => RepositoryManager.Get<PastShops>());
            _lazyFutureShopsRepo = new Lazy<Repository<FutureShops>>(() => RepositoryManager.Get<FutureShops>());
        }

        public async Task InitializeAsync()
        {
            if (!_isRepositoryManagerInitialized)
            {
                lock (_initLock)
                {
                    if (!_isRepositoryManagerInitialized)
                    {
                        RepositoryManager.Initialize(
                            "Host=localhost;Port=5432;Database=onedoteleven;Username=postgres;Password=a",
                            true);
                        _isRepositoryManagerInitialized = true;
                    }
                }
            }
        }

        public async Task CreateCatalogAsync(Catalog catalog)
            => await _catalogRepo.SaveAsync(catalog);

        public async Task CreateShopSectionsAsync(List<ShopSections> shopSections)
            => await _shopSectionsRepo.SaveManyAsync(shopSections);

        public async Task<PastShops?> GetLastProcessedShopAsync()
            => await _pastShopsRepo.FindLastByAsync("date");

        public async Task CreatePastShopAsync(PastShops pastShop)
            => await _pastShopsRepo.SaveAsync(pastShop);

        public async Task<bool> IsShopProcessedAsync(DateTime date)
        {
            var shopDate = date.ToString("yyyy-MM-dd");
            var currentDay = DateTime.UtcNow.ToString("yyyy-MM-dd");

            var shop = await _pastShopsRepo.FindByColumnsAsync(
                new[] { "date", "created_at" },
                new object[] { shopDate, currentDay }
            );

            return shop != null;
        }

        public async Task DeleteAllStorefrontsAsync() 
            => await _catalogRepo.DeleteAllAsync();

        public async Task DeleteAllShopSectionsAsync()
            => await _shopSectionsRepo.DeleteAllAsync();

        public async Task DeleteAllFutureShopsAsync()
            => await _futureShopsRepo.DeleteAllAsync();

        public async Task<List<FutureShops>> GetFutureShopsAsync()
        {
            var today = DateTime.UtcNow.ToString("yyyy-MM-dd");

            var allShops = await _futureShopsRepo.FindAllByAsync(new Dictionary<string, object>());
            return allShops
                .Where(s => string.Compare(s.Date, today, StringComparison.Ordinal) > 0)
                .ToList();
        }
    }
}

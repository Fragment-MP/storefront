using MochaStorefront.Core.Config;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MochaStorefront.Core.Generation
{
    public class CategoryTracker
    {
        private readonly ConcurrentDictionary<string, List<string>> _characterCategories;
        private readonly ReaderWriterLockSlim _lock;
        private DateTime _lastPickaxeRemoval;

        public CategoryTracker()
        {
            _characterCategories = new ConcurrentDictionary<string, List<string>>();
            _lock = new ReaderWriterLockSlim();
            _lastPickaxeRemoval = DateTime.MinValue;
        }

        public void TrackCharacterCategories(string itemId, List<string> categories)
        {
            if (IsCharacterItem(itemId))
            {
                _characterCategories.AddOrUpdate(itemId, categories, (_, _) => categories);
            }
        }

        public List<string> GeneratePickaxeCategories(List<string> originalCategories, string setValue)
        {
            _lock.EnterUpgradeableReadLock();
            try
            {
                var now = DateTime.UtcNow;
                if (now - _lastPickaxeRemoval < ShopConfiguration.PickaxeRemovalInterval)
                {
                    return originalCategories;
                }

                _lock.EnterWriteLock();
                try
                {
                    _lastPickaxeRemoval = now;

                    if (string.Equals(setValue, "Goalbound", StringComparison.OrdinalIgnoreCase))
                    {
                        return new List<string> { setValue };
                    }

                    if (_characterCategories.Values.Any(charCategories =>
                        charCategories.SequenceEqual(originalCategories)))
                    {
                        return new List<string>();
                    }

                    return originalCategories;
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }
            finally
            {
                _lock.ExitUpgradeableReadLock();
            }
        }

        private static bool IsCharacterItem(string itemId) =>
            itemId.Contains("cid_", StringComparison.OrdinalIgnoreCase);

        public void Dispose()
        {
            _lock?.Dispose();
        }
    }
}

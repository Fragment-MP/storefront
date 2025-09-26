using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MochaStorefront.Core.Generation
{
    public class DisplayAssetPathGenerator
    {
        private static readonly ConcurrentDictionary<string, string> _displayAssetCache = new();
        private static readonly ConcurrentDictionary<string, string> _newDisplayAssetCache = new();

        public static string GetDisplayAssetPath(string itemId)
        {
            return _displayAssetCache.GetOrAdd(itemId,
                id => $"/Game/Catalog/DisplayAssets/DA_Featured_{id}.DA_Featured_{id}");
        }

        public static string GetNewDisplayAssetPath(string itemId)
        {
            return _newDisplayAssetCache.GetOrAdd(itemId,
                id => $"/Game/Catalog/NewDisplayAssets/DAv2_Featured_{id}.DAv2_Featured_{id}");
        }
    }
}

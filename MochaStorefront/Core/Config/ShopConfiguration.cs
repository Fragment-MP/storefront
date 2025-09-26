using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MochaStorefront.Core.Config
{
    public class ShopConfiguration
    {
        public static readonly HashSet<string> UnsupportedBackendValues = new()
        {
            "AthenaEmoji",
            "AthenaMusicPack"
        };

        public static readonly HashSet<string> ValidFreeItems = new()
        {
            "Heartspan",
            "MusicPack_LavaChicken"
        };

        public static readonly Dictionary<string, string> CategoryOverrides = new()
        {
            { "Crystal", "Dino Guard" }
        };

        public static readonly TimeSpan PickaxeRemovalInterval = TimeSpan.FromDays(7);
        public static readonly int CurrentVersion = 18;
    }
}

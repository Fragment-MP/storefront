using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MochaStorefront.Core.Config
{
    public class ShopConfiguration
    {
        public static readonly DateTime DefaultStartDate = new DateTime(2021, 12, 5, 0, 0, 0, DateTimeKind.Utc);
        public static readonly DateTime DefaultStopDate = new DateTime(2022, 3, 19, 0, 0, 0, DateTimeKind.Utc);

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

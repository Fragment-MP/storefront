using MochaStorefront.Core.Models;

namespace MochaStorefront.Core.Generation;

public static class BackendResolver
{
    private static readonly Dictionary<string, string> TypeToBackend = new(StringComparer.OrdinalIgnoreCase)
    {
        { "emote", "AthenaDance" },
        { "emoji", "AthenaDance" },
        { "toy", "AthenaDance" },
        { "loading", "AthenaLoadingScreen" },
        { "trails", "AthenaSkyDiveContrail" },
        { "glider", "AthenaGlider" },
        { "backpack", "AthenaBackpack" },
        { "petcarrier", "AthenaBackpack" },
        { "musicpack", "AthenaMusicPack" },
        { "pickaxe", "AthenaPickaxe" },
        { "wrap", "AthenaItemWrap" },
        { "outfit", "AthenaCharacter" },
        { "EID", "AthenaDance" },
        { "SPID", "AthenaDance" },
        { "LSID", "AthenaLoadingScreen" },
        { "MtxGiveaway", "Currency" },
        { "BID", "AthenaBackpack" },
        { "AthenaSeasonXpBoost", "Token" },
        { "AthenaSeasonFriendXpBoost", "Token" },
        { "CID", "AthenaCharacter" },
        { "AthenaSeasonMergedXpBoosts", "Token" },
        { "AthenaSeasonalXP", "Token" },
        { "AthenaNextSeasonTierBoost", "Token" },
        { "AthenaNextSeasonXPBoost", "Token" },
        { "AthenaBattlePassTier", "Token" },
        { "VTID", "CosmeticVariantToken" }
    };

    public static string DetermineItemBackendValue(Item item)
    {
        if (string.IsNullOrEmpty(item?.Type) || string.IsNullOrEmpty(item?.ID))
            return string.Empty;

        if (TypeToBackend.TryGetValue(item.Type, out var backend))
        {
            return backend;
        }

        foreach (var kvp in TypeToBackend)
        {
            if (item.ID.Contains(kvp.Key, StringComparison.OrdinalIgnoreCase))
            {
                return kvp.Value;
            }
        }

        return string.Empty;
    }
}
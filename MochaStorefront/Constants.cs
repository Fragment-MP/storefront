using MochaStorefront.Core.Models;

namespace MochaStorefront;

public static class Constants
{
    public static object StorefrontLock { get; } = new object();
    public static Dictionary<string, List<Item>> Storefront { get; set; }
}
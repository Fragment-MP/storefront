using MochaStorefront.Core.Models;

namespace MochaStorefront.Core;

public static class StorefrontService
{
    public static Storefront CreateSection(string section)
    {
        return new Storefront
        {
            Name = section,
            CatalogEntries = new List<CatalogEntry>()
        };
    }
}
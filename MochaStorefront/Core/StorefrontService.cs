using MochaStorefront.Core.Models;

namespace MochaStorefront.Core;

public static class StorefrontService
{
    private static readonly List<Storefront> Sections = new();

    public static Storefront CreateSection(string section)
    {
        var storefront = new Storefront
        {
            Name = section,
            CatalogEntries = new List<CatalogEntry>()
        };

        Sections.Add(storefront);
        return storefront;
    }

    public static Storefront? GetSection(string sectionName)
    {
        return Sections.FirstOrDefault(s =>
            string.Equals(s.Name, sectionName, StringComparison.OrdinalIgnoreCase));
    }

    public static IEnumerable<Storefront> GetAllSections()
    {
        return Sections;
    }
}

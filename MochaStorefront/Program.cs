using MochaStorefront.Core;
using MochaStorefront.Core.Generation;

public class Program
{
    public static async Task Main(string[] args)
    {
        var database = new PostgresService();
        await database.InitializeAsync();

        var generator = new StorefrontGenerator();

        var storefrontManager = new StorefrontManager(database);
        await storefrontManager.InitializeAsync(args);

        Console.ReadKey();
    }
}
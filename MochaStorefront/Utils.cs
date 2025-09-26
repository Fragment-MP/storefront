using System.Text.Json;
using MochaStorefront.Core.Models;

namespace MochaStorefront;

public static class Utils
{
  private static readonly HttpClient _httpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(10)
    };

    public static async Task<Dictionary<string, KnownItem>?> LoadItemsAsync(string apiUrl)
    {
        try
        {
            LogWithTimestamp("Fetching items from API...", ConsoleColor.Yellow);
            
            var response = await _httpClient.GetAsync(apiUrl);
            
            if (!response.IsSuccessStatusCode)
            {
                LogWithTimestamp($"API request failed with status: {response.StatusCode}", ConsoleColor.Red);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ApiResponse>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (apiResponse == null || apiResponse.Data == null)
            {
                LogWithTimestamp("Failed to parse API response", ConsoleColor.Red);
                return null;
            }
            
            var itemMap = new Dictionary<string, KnownItem>();
            
            foreach (var item in apiResponse.Data)
            {
                if (item.ShopHistory != null && item.ShopHistory.Any())
                {
                    if (!string.IsNullOrEmpty(item.Name) && !string.IsNullOrEmpty(item.ID))
                    {
                        itemMap[item.Name] = new KnownItem
                        {
                            ID = item.ID,
                            Set = item.Set,
                            Item = item.Type,
                            Images = item.Images
                        };
                    }
                }
            }

            LogWithTimestamp($"Loaded {itemMap.Count} items from API", ConsoleColor.Green);
            return itemMap;
        }
        catch (Exception ex)
        {
            LogWithTimestamp($"Failed to fetch items from API: {ex.Message}", ConsoleColor.Red);
            return null;
        }
    }

    public static async Task SaveJsonAsync<T>(string filepath, T data)
    {
        try
        {
            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            
            await File.WriteAllTextAsync(filepath, json);
            LogWithTimestamp($"Saved data to: {filepath}", ConsoleColor.Green);
        }
        catch (Exception ex)
        {
            LogWithTimestamp($"Failed to save JSON: {ex.Message}", ConsoleColor.Red);
            throw;
        }
    }

    private static void LogWithTimestamp(string message, ConsoleColor color)
    {
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        var originalColor = Console.ForegroundColor;
        
        Console.Write($"[{timestamp}] ");
        Console.ForegroundColor = color;
        Console.WriteLine(message);
        Console.ForegroundColor = originalColor;
    }

    public static void LogWithTimestamp(ConsoleColor color, string message, params object[] args)
    {
        var formattedMessage = args.Length > 0 ? string.Format(message, args) : message;
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        Console.ForegroundColor = color;
        Console.WriteLine($"[{timestamp}] {formattedMessage}");
        Console.ResetColor();
    }
}
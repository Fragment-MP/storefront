using System.Text.Json.Serialization;

namespace MochaStorefront.Core.Models;

public class ApiResponse
{
    [JsonPropertyName("status")]
    public int Status { get; set; }
    [JsonPropertyName("data")]
    public List<ApiCosmetic> Data { get; set; }
}
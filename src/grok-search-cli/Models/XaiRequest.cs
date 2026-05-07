using System.Text.Json.Serialization;

namespace XaiSearchCli.Models;

public class XaiRequest
{
    [JsonPropertyName("model")]
    public required string Model { get; set; }

    [JsonPropertyName("input")]
    public required string Input { get; set; }

    [JsonPropertyName("tools")]
    public required List<XaiTool> Tools { get; set; }
}

public class XaiTool
{
    [JsonPropertyName("type")]
    public required string Type { get; set; }

    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("search_filters")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public WebSearchFilters? SearchFilters { get; set; }

    [JsonPropertyName("user_handle_filters")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public XSearchUserFilters? UserHandleFilters { get; set; }
}

public class WebSearchFilters
{
    [JsonPropertyName("allowed_domains")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? AllowedDomains { get; set; }

    [JsonPropertyName("excluded_domains")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? ExcludedDomains { get; set; }
}

public class XSearchUserFilters
{
    [JsonPropertyName("allowed")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? Allowed { get; set; }

    [JsonPropertyName("excluded")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? Excluded { get; set; }

    [JsonPropertyName("from_date")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? FromDate { get; set; }

    [JsonPropertyName("to_date")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ToDate { get; set; }
}

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

    [JsonPropertyName("allowed_domains")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? AllowedDomains { get; set; }

    [JsonPropertyName("excluded_domains")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? ExcludedDomains { get; set; }

    [JsonPropertyName("allowed_x_handles")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? AllowedXHandles { get; set; }

    [JsonPropertyName("excluded_x_handles")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? ExcludedXHandles { get; set; }

    [JsonPropertyName("from_date")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? FromDate { get; set; }

    [JsonPropertyName("to_date")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ToDate { get; set; }

    [JsonPropertyName("enable_image_understanding")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? EnableImageUnderstanding { get; set; }

    [JsonPropertyName("enable_video_understanding")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? EnableVideoUnderstanding { get; set; }
}

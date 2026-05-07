using System.Text.Json.Serialization;

namespace XaiSearchCli.Models;

public class XaiResponse
{
    [JsonPropertyName("id")]
    public required string Id { get; set; }

    [JsonPropertyName("output")]
    public required List<XaiOutputItem> Output { get; set; }
}

public class XaiOutputItem
{
    [JsonPropertyName("content")]
    public List<XaiContentBlock>? Content { get; set; }
}

public class XaiContentBlock
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("text")]
    public string? Text { get; set; }

    [JsonPropertyName("annotations")]
    public List<XaiAnnotation>? Annotations { get; set; }
}

public class XaiAnnotation
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }
}

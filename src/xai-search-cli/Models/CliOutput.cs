using System.Text.Json.Serialization;

namespace XaiSearchCli.Models;

public class CliOutput
{
    [JsonPropertyName("tool")]
    public required string Tool { get; set; }

    [JsonPropertyName("model")]
    public required string Model { get; set; }

    [JsonPropertyName("answer")]
    public required string Answer { get; set; }

    [JsonPropertyName("citations")]
    public required List<CliCitation> Citations { get; set; }

    [JsonPropertyName("id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Id { get; set; }
}

public class CliCitation
{
    [JsonPropertyName("url")]
    public required string Url { get; set; }

    [JsonPropertyName("title")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Title { get; set; }
}

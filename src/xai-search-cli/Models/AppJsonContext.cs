using System.Text.Json.Serialization;

namespace XaiSearchCli.Models;

[JsonSerializable(typeof(XaiRequest))]
[JsonSerializable(typeof(XaiResponse))]
[JsonSerializable(typeof(CliOutput))]
[JsonSerializable(typeof(XaiTool))]
[JsonSerializable(typeof(WebSearchFilters))]
[JsonSerializable(typeof(XSearchUserFilters))]
[JsonSerializable(typeof(XaiOutputItem))]
[JsonSerializable(typeof(XaiContentBlock))]
[JsonSerializable(typeof(XaiAnnotation))]
[JsonSerializable(typeof(CliCitation))]
internal partial class AppJsonContext : JsonSerializerContext
{
}

using System.Text.Json.Serialization;

namespace XaiSearchCli.Models;

[JsonSerializable(typeof(XaiRequest))]
[JsonSerializable(typeof(XaiResponse))]
[JsonSerializable(typeof(CliOutput))]
[JsonSerializable(typeof(XaiTool))]
[JsonSerializable(typeof(XaiOutputItem))]
[JsonSerializable(typeof(XaiContentBlock))]
[JsonSerializable(typeof(XaiAnnotation))]
[JsonSerializable(typeof(CliCitation))]
[JsonSerializable(typeof(CliDiscoveryDocument))]
[JsonSerializable(typeof(CommandGroup))]
[JsonSerializable(typeof(FlagInfo))]
[JsonSerializable(typeof(SubcommandInfo))]
[JsonSerializable(typeof(ExampleInfo))]
[JsonSerializable(typeof(CredentialInfo))]
internal partial class AppJsonContext : JsonSerializerContext
{
}

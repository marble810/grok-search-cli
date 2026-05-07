using System.Text.Json;
using XaiSearchCli.Models;

namespace XaiSearchCli;

/// <summary>
/// Handles discovery entrypoints: human-readable help and machine-readable describe output.
/// These entrypoints do not require credentials or network access.
/// </summary>
public static class DiscoveryCommand
{
    /// <summary>
    /// Handle the 'help' command. With no arguments or an unknown topic, prints root help.
    /// Known topics: "search", "auth".
    /// </summary>
    public static void HandleHelp(string[] args, TextWriter stderr)
    {
        if (args.Length == 0)
        {
            Console.Out.Write(CommandRegistry.GetRootHelp());
            return;
        }

        var topic = args[0].ToLowerInvariant();
        switch (topic)
        {
            case "search":
                Console.Out.Write(CommandRegistry.GetSearchHelp());
                break;
            case "auth":
                Console.Out.Write(CommandRegistry.GetAuthHelp());
                break;
            default:
                Console.Out.Write(CommandRegistry.GetRootHelp());
                break;
        }
    }

    /// <summary>
    /// Handle the 'describe' command. Emits a JSON discovery document to stdout.
    /// </summary>
    public static void HandleDescribe(TextWriter stderr)
    {
        var doc = CommandRegistry.BuildDiscoveryDocument();
        var json = JsonSerializer.Serialize(doc, AppJsonContext.Default.CliDiscoveryDocument);
        Console.Out.WriteLine(json);
    }
}

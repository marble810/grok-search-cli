using System.Text.Json;
using XaiSearchCli;
using XaiSearchCli.Models;

namespace XaiSearchCli.Tests;

public class DiscoveryCommandTests
{
    [Fact]
    public void Help_Root_ShowsUsageAndCommandGroups()
    {
        var stderr = new StringWriter();
        var stdout = new StringWriter();
        Console.SetOut(stdout);

        try
        {
            DiscoveryCommand.HandleHelp([], stderr);
            var output = stdout.ToString();

            Assert.Contains("grok-search-cli", output);
            Assert.Contains("USAGE", output);
            Assert.Contains("SEARCH", output);
            Assert.Contains("AUTH", output);
            Assert.Contains("DISCOVERY", output);
            Assert.Contains("CREDENTIALS", output);
            Assert.Contains("OUTPUT", output);
            Assert.Contains("--tool", output);
            Assert.Contains("--model", output);
            Assert.Contains("auth login", output);
            Assert.Contains("help [command]", output);
            Assert.Contains("describe", output);
        }
        finally
        {
            Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        }
    }

    [Fact]
    public void Help_Root_WhenCalledWithHelpArg()
    {
        var stderr = new StringWriter();
        var stdout = new StringWriter();
        Console.SetOut(stdout);

        try
        {
            DiscoveryCommand.HandleHelp(["help"], stderr);
            var output = stdout.ToString();

            Assert.Contains("grok-search-cli", output);
            Assert.Contains("USAGE", output);
        }
        finally
        {
            Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        }
    }

    [Fact]
    public void Help_Search_ShowsSearchSpecificHelp()
    {
        var stderr = new StringWriter();
        var stdout = new StringWriter();
        Console.SetOut(stdout);

        try
        {
            DiscoveryCommand.HandleHelp(["search"], stderr);
            var output = stdout.ToString();

            Assert.Contains("Execute a search query", output);
            Assert.Contains("FLAGS", output);
            Assert.Contains("QUERY RULES", output);
            Assert.Contains("EXAMPLES", output);
            Assert.Contains("OUTPUT", output);
            Assert.Contains("--tool", output);
            Assert.Contains("--model", output);
            Assert.DoesNotContain("AUTH", output);
            Assert.DoesNotContain("auth login", output);
        }
        finally
        {
            Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        }
    }

    [Fact]
    public void Help_Auth_ShowsAuthSpecificHelp()
    {
        var stderr = new StringWriter();
        var stdout = new StringWriter();
        Console.SetOut(stdout);

        try
        {
            DiscoveryCommand.HandleHelp(["auth"], stderr);
            var output = stdout.ToString();

            Assert.Contains("Manage API credentials", output);
            Assert.Contains("COMMANDS", output);
            Assert.Contains("CREDENTIAL SOURCES", output);
            Assert.Contains("auth login", output);
            Assert.Contains("auth status", output);
            Assert.Contains("auth logout", output);
            Assert.DoesNotContain("--tool", output);
        }
        finally
        {
            Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        }
    }

    [Fact]
    public void Describe_OutputsValidJson()
    {
        var stderr = new StringWriter();
        var stdout = new StringWriter();
        Console.SetOut(stdout);

        try
        {
            DiscoveryCommand.HandleDescribe(stderr);
            var json = stdout.ToString();

            Assert.False(string.IsNullOrWhiteSpace(json));
            var doc = JsonDocument.Parse(json);
            Assert.NotNull(doc);
        }
        finally
        {
            Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        }
    }

    [Fact]
    public void Describe_DocumentHasRequiredFields()
    {
        var doc = CommandRegistry.BuildDiscoveryDocument();

        Assert.Equal("grok-search-cli", doc.CliName);
        Assert.NotNull(doc.Documentation);
        Assert.NotEmpty(doc.Commands);

        var searchCmd = doc.Commands.Find(c => c.Name == "search")!;
        Assert.NotNull(searchCmd.Flags);
        Assert.NotEmpty(searchCmd.Flags!);
        Assert.NotNull(searchCmd.CredentialPrerequisites);
        Assert.Contains("XAI_API_KEY", searchCmd.CredentialPrerequisites);
        Assert.NotNull(searchCmd.Examples);
        Assert.NotEmpty(searchCmd.Examples!);

        var authCmd = doc.Commands.Find(c => c.Name == "auth")!;
        Assert.NotNull(authCmd.Subcommands);
        Assert.NotEmpty(authCmd.Subcommands!);
        Assert.NotNull(authCmd.Examples);
        Assert.NotEmpty(authCmd.Examples!);

        Assert.NotNull(doc.DiscoveryCommands);
        Assert.Contains(doc.DiscoveryCommands, dc => dc.Name == "help");
        Assert.Contains(doc.DiscoveryCommands, dc => dc.Name == "describe");

        Assert.NotNull(doc.Credentials);
        Assert.NotEmpty(doc.Credentials.Sources);
    }

    [Fact]
    public void Describe_SearchCommand_HasCorrectFlags()
    {
        var doc = CommandRegistry.BuildDiscoveryDocument();
        var searchCmd = doc.Commands.Find(c => c.Name == "search")!;

        var toolFlag = searchCmd.Flags!.Find(f => f.Name == "--tool");
        Assert.NotNull(toolFlag);
        Assert.True(toolFlag.Required);
        Assert.Equal(["web", "x", "both"], toolFlag.Values);

        var modelFlag = searchCmd.Flags.Find(f => f.Name == "--model");
        Assert.NotNull(modelFlag);
        Assert.False(modelFlag.Required);
        Assert.False(modelFlag.Repeatable);

        var allowDomainFlag = searchCmd.Flags.Find(f => f.Name == "--allow-domain");
        Assert.NotNull(allowDomainFlag);
        Assert.False(allowDomainFlag.Required);
        Assert.True(allowDomainFlag.Repeatable);
    }

    [Fact]
    public void Describe_AuthCommand_HasExpectedSubcommands()
    {
        var doc = CommandRegistry.BuildDiscoveryDocument();
        var authCmd = doc.Commands.Find(c => c.Name == "auth")!;

        var subNames = authCmd.Subcommands!.Select(s => s.Name).ToList();
        Assert.Contains("login", subNames);
        Assert.Contains("status", subNames);
        Assert.Contains("logout", subNames);
    }

    [Fact]
    public void Describe_SearchCommand_HasQueryRules()
    {
        var doc = CommandRegistry.BuildDiscoveryDocument();
        var searchCmd = doc.Commands.Find(c => c.Name == "search")!;

        Assert.NotNull(searchCmd.QueryRules);
        Assert.Contains(searchCmd.QueryRules, r => r.Contains("stdin"));
    }

    [Fact]
    public void Describe_CanSerializeToJsonWithSourceGen()
    {
        // Verify that the document can be serialized using the AOT-friendly source generator
        var doc = CommandRegistry.BuildDiscoveryDocument();
        var json = JsonSerializer.Serialize(doc, AppJsonContext.Default.CliDiscoveryDocument);

        var parsed = JsonSerializer.Deserialize(json, AppJsonContext.Default.CliDiscoveryDocument);
        Assert.NotNull(parsed);
        Assert.Equal("grok-search-cli", parsed!.CliName);
    }

    [Fact]
    public void HelpOutput_DoesNotRequireCredentials()
    {
        // This test verifies that the help entrypoint can be called without any credential setup
        var stderr = new StringWriter();
        var stdout = new StringWriter();
        Console.SetOut(stdout);

        try
        {
            // No credentials configured - help should still work
            DiscoveryCommand.HandleHelp([], stderr);
            var output = stdout.ToString();

            Assert.Contains("grok-search-cli", output);
            Assert.Empty(stderr.ToString()); // No errors on stderr
        }
        finally
        {
            Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        }
    }

    [Fact]
    public void DescribeOutput_DoesNotRequireCredentials()
    {
        var stderr = new StringWriter();
        var stdout = new StringWriter();
        Console.SetOut(stdout);

        try
        {
            // No credentials configured - describe should still work
            DiscoveryCommand.HandleDescribe(stderr);
            var json = stdout.ToString();
            var doc = JsonDocument.Parse(json);

            Assert.NotNull(doc);
            Assert.Empty(stderr.ToString()); // No errors on stderr
        }
        finally
        {
            Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        }
    }

    [Fact]
    public void Describe_JsonHasExpectedTopLevelProperties()
    {
        var doc = CommandRegistry.BuildDiscoveryDocument();
        var json = JsonSerializer.Serialize(doc, AppJsonContext.Default.CliDiscoveryDocument);
        var parsed = JsonDocument.Parse(json);

        Assert.True(parsed.RootElement.TryGetProperty("cli_name", out _));
        Assert.True(parsed.RootElement.TryGetProperty("documentation", out _));
        Assert.True(parsed.RootElement.TryGetProperty("commands", out var commands));
        Assert.Equal(JsonValueKind.Array, commands.ValueKind);
        Assert.True(parsed.RootElement.TryGetProperty("credentials", out _));
    }

    [Fact]
    public void HelpOutput_UnknownTopicFallsBackToRoot()
    {
        var stderr = new StringWriter();
        var stdout = new StringWriter();
        Console.SetOut(stdout);

        try
        {
            DiscoveryCommand.HandleHelp(["nonexistent"], stderr);
            var output = stdout.ToString();

            // Should fall back to root help
            Assert.Contains("USAGE", output);
            Assert.Contains("SEARCH", output);
            Assert.Contains("AUTH", output);
        }
        finally
        {
            Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        }
    }
}

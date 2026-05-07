using System.Text.Json.Serialization;

namespace XaiSearchCli.Models;

/// <summary>
/// Machine-readable discovery document describing the full CLI surface.
/// </summary>
public class CliDiscoveryDocument
{
    [JsonPropertyName("cli_name")]
    public required string CliName { get; set; }

    [JsonPropertyName("documentation")]
    public required string Documentation { get; set; }

    [JsonPropertyName("commands")]
    public required List<CommandGroup> Commands { get; set; }

    [JsonPropertyName("discovery_commands")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<CommandGroup>? DiscoveryCommands { get; set; }

    [JsonPropertyName("credentials")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public CredentialInfo? Credentials { get; set; }
}

/// <summary>
/// Describes a single command group (e.g. search, auth) or discovery command.
/// </summary>
public class CommandGroup
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("description")]
    public required string Description { get; set; }

    [JsonPropertyName("usage")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Usage { get; set; }

    [JsonPropertyName("flags")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<FlagInfo>? Flags { get; set; }

    [JsonPropertyName("subcommands")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<SubcommandInfo>? Subcommands { get; set; }

    [JsonPropertyName("query_rules")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? QueryRules { get; set; }

    [JsonPropertyName("credential_prerequisites")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? CredentialPrerequisites { get; set; }

    [JsonPropertyName("output_mode")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? OutputMode { get; set; }

    [JsonPropertyName("examples")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<ExampleInfo>? Examples { get; set; }
}

/// <summary>
/// Describes a CLI flag.
/// </summary>
public class FlagInfo
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("required")]
    public bool Required { get; set; }

    [JsonPropertyName("values")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? Values { get; set; }

    [JsonPropertyName("repeatable")]
    public bool Repeatable { get; set; }

    [JsonPropertyName("description")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Description { get; set; }
}

/// <summary>
/// Describes a subcommand (e.g. auth login, auth status).
/// </summary>
public class SubcommandInfo
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("description")]
    public required string Description { get; set; }

    [JsonPropertyName("usage")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Usage { get; set; }
}

/// <summary>
/// Describes an example invocation.
/// </summary>
public class ExampleInfo
{
    [JsonPropertyName("description")]
    public required string Description { get; set; }

    [JsonPropertyName("command")]
    public required string Command { get; set; }
}

/// <summary>
/// Describes credential sources and notes.
/// </summary>
public class CredentialInfo
{
    [JsonPropertyName("sources")]
    public required List<string> Sources { get; set; }

    [JsonPropertyName("note")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Note { get; set; }
}

/// <summary>
/// Central registry of command metadata shared by help and describe output.
/// </summary>
public static class CommandRegistry
{
    private const string CliName = "grok-search-cli";

    public static CliDiscoveryDocument BuildDiscoveryDocument()
    {
        return new CliDiscoveryDocument
        {
            CliName = CliName,
            Documentation = "Machine-readable usage data for grok-search-cli",
            Commands =
            [
                new CommandGroup
                {
                    Name = "search",
                    Description = "Execute a search query using xAI",
                    Usage = "grok-search-cli [options] <query>",
                    Flags =
                    [
                        new FlagInfo
                        {
                            Name = "--tool",
                            Required = true,
                            Values = ["web", "x", "both"],
                            Repeatable = false,
                            Description = "Search source: web, X/Twitter, or both combined"
                        },
                        new FlagInfo
                        {
                            Name = "--allow-domain",
                            Required = false,
                            Repeatable = true,
                            Description = "Web filter: include only this domain (repeatable)"
                        },
                        new FlagInfo
                        {
                            Name = "--exclude-domain",
                            Required = false,
                            Repeatable = true,
                            Description = "Web filter: exclude this domain (repeatable)"
                        },
                        new FlagInfo
                        {
                            Name = "--allow-handle",
                            Required = false,
                            Repeatable = true,
                            Description = "X filter: include only this handle (repeatable)"
                        },
                        new FlagInfo
                        {
                            Name = "--exclude-handle",
                            Required = false,
                            Repeatable = true,
                            Description = "X filter: exclude this handle (repeatable)"
                        },
                        new FlagInfo
                        {
                            Name = "--from-date",
                            Required = false,
                            Repeatable = false,
                            Description = "X filter: include results on or after this date (YYYY-MM-DD)"
                        },
                        new FlagInfo
                        {
                            Name = "--to-date",
                            Required = false,
                            Repeatable = false,
                            Description = "X filter: include results on or before this date (YYYY-MM-DD)"
                        }
                    ],
                    QueryRules =
                    [
                        "Provide the query as a positional argument or via stdin, not both",
                        "If both are provided, the CLI exits with a configuration error"
                    ],
                    CredentialPrerequisites = ["XAI_API_KEY"],
                    OutputMode = "One JSON document on stdout with fields: tool, model, answer, citations, id",
                    Examples =
                    [
                        new ExampleInfo { Description = "Web search", Command = "grok-search-cli --tool web \"latest AI news\"" },
                        new ExampleInfo { Description = "X search with handle filter", Command = "grok-search-cli --tool x \"product launch\" --allow-handle techcrunch --from-date 2026-01-01" },
                        new ExampleInfo { Description = "Combined search with domain filter", Command = "grok-search-cli --tool both --exclude-domain spam.com \"topic\"" },
                        new ExampleInfo { Description = "Query from stdin", Command = "printf \"query\" | grok-search-cli --tool web" }
                    ]
                },
                new CommandGroup
                {
                    Name = "auth",
                    Description = "Manage API credentials",
                    Usage = "grok-search-cli auth <command> [options]",
                    Subcommands =
                    [
                        new SubcommandInfo
                        {
                            Name = "login",
                            Description = "Set up API credentials (interactive or --api-key-stdin)",
                            Usage = "grok-search-cli auth login [--api-key-stdin]"
                        },
                        new SubcommandInfo
                        {
                            Name = "status",
                            Description = "Check credential configuration status",
                            Usage = "grok-search-cli auth status"
                        },
                        new SubcommandInfo
                        {
                            Name = "logout",
                            Description = "Remove stored credentials",
                            Usage = "grok-search-cli auth logout"
                        }
                    ],
                    OutputMode = "Human-readable messages on stderr",
                    Examples =
                    [
                        new ExampleInfo { Description = "Interactive login", Command = "grok-search-cli auth login" },
                        new ExampleInfo { Description = "Non-interactive login from stdin", Command = "echo \"my-api-key\" | grok-search-cli auth login --api-key-stdin" },
                        new ExampleInfo { Description = "Check credential status", Command = "grok-search-cli auth status" },
                        new ExampleInfo { Description = "Clear stored credentials", Command = "grok-search-cli auth logout" }
                    ]
                }
            ],
            DiscoveryCommands =
            [
                new CommandGroup
                {
                    Name = "help",
                    Description = "Show human-readable help",
                    Usage = "grok-search-cli help [command]",
                    Examples =
                    [
                        new ExampleInfo { Description = "Root help", Command = "grok-search-cli help" },
                        new ExampleInfo { Description = "Search command help", Command = "grok-search-cli help search" },
                        new ExampleInfo { Description = "Auth command help", Command = "grok-search-cli help auth" }
                    ]
                },
                new CommandGroup
                {
                    Name = "describe",
                    Description = "Show machine-readable usage data (JSON)",
                    Usage = "grok-search-cli describe",
                    Examples =
                    [
                        new ExampleInfo { Description = "Show discovery document", Command = "grok-search-cli describe" }
                    ]
                }
            ],
            Credentials = new CredentialInfo
            {
                Sources =
                [
                    "XAI_API_KEY environment variable",
                    ".env file (searched up from current directory)",
                    "Managed credential store (~/.config/grok-search-cli/credentials.env)"
                ],
                Note = "Credentials are required for search but not for discovery commands (help, describe)"
            }
        };
    }

    /// <summary>
    /// Root help text shown for 'grok-search-cli help' or 'grok-search-cli help search'.
    /// </summary>
    public static string GetRootHelp()
    {
        return $$"""
grok-search-cli - Search the web and X (Twitter) using xAI

USAGE
  grok-search-cli [options] <query>
  grok-search-cli auth <command> [options]
  grok-search-cli help [command]
  grok-search-cli describe

SEARCH
  --tool (web|x|both)          Required. Select web, X, or both.
  <query>                      Search query (positional arg or stdin, not both)
  --allow-domain <domain>      Allowed web domain (repeatable)
  --exclude-domain <domain>    Excluded web domain (repeatable)
  --allow-handle <handle>      Allowed X handle (repeatable)
  --exclude-handle <handle>    Excluded X handle (repeatable)
  --from-date <yyyy-mm-dd>     X search start date
  --to-date <yyyy-mm-dd>       X search end date

AUTH
  auth login                   Set up API credentials (interactive or --api-key-stdin)
  auth status                  Check credential configuration
  auth logout                  Remove stored credentials

DISCOVERY
  help [command]               Show this help or command-specific help
  describe                     Show machine-readable usage data (JSON)

CREDENTIALS
  XAI_API_KEY via environment variable, .env file, or '{{CliName}} auth login'

OUTPUT
  Search results: JSON on stdout
  Errors and logs: stderr
""";
    }

    /// <summary>
    /// Search-specific help text.
    /// </summary>
    public static string GetSearchHelp()
    {
        return $$"""
grok-search-cli - Execute a search query

USAGE
  grok-search-cli [options] <query>

FLAGS
  --tool (web|x|both)          Required. Select web, X, or both.
  --allow-domain <domain>      Allowed web domain (repeatable)
  --exclude-domain <domain>    Excluded web domain (repeatable)
  --allow-handle <handle>      Allowed X handle (repeatable)
  --exclude-handle <handle>    Excluded X handle (repeatable)
  --from-date <yyyy-mm-dd>     X search start date
  --to-date <yyyy-mm-dd>       X search end date

QUERY RULES
  Provide the query as a positional argument or via stdin, not both.
  If both are provided, the CLI exits with a configuration error.

EXAMPLES
  {{CliName}} --tool web "latest AI news"
  {{CliName}} --tool x "product launch" --allow-handle techcrunch
  {{CliName}} --tool both "topic" --exclude-domain spam.com
  echo "query" | {{CliName}} --tool web

OUTPUT
  One JSON document on stdout with fields: tool, model, answer, citations, id
""";
    }

    /// <summary>
    /// Auth-specific help text.
    /// </summary>
    public static string GetAuthHelp()
    {
        return $$"""
grok-search-cli auth - Manage API credentials

COMMANDS
  auth login                   Set up API credentials interactively
  auth login --api-key-stdin   Set up API credentials from piped stdin
  auth status                  Check credential configuration
  auth logout                  Remove stored credentials

CREDENTIAL SOURCES (precedence order)
  1. XAI_API_KEY environment variable
  2. .env file (searched up from current directory)
  3. Managed credential store (~/.config/grok-search-cli/credentials.env)

EXAMPLES
  {{CliName}} auth login
  echo "my-api-key" | {{CliName}} auth login --api-key-stdin
  {{CliName}} auth status
  {{CliName}} auth logout
""";
    }
}

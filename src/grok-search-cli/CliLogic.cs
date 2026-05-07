using System.Text.Json;
using XaiSearchCli.Models;

namespace XaiSearchCli;

public class CommandException : Exception
{
    public CommandException(string message, int exitCode = 1) : base(message)
    {
        ExitCode = exitCode;
    }

    public int ExitCode { get; }
}

public class ToolFilters
{
    public List<string>? WebAllowed { get; set; }
    public List<string>? WebExcluded { get; set; }
    public List<string>? XAllowed { get; set; }
    public List<string>? XExcluded { get; set; }
    public string? XFromDate { get; set; }
    public string? XToDate { get; set; }
    public bool EnableImageUnderstanding { get; set; }
    public bool EnableVideoUnderstanding { get; set; }
}

public static class CliLogic
{
    public const string DefaultModel = "grok-4.3";

    public static List<XaiTool> BuildTools(string tool, ToolFilters filters)
    {
        var tools = new List<XaiTool>();
        if (tool is "web" or "both")
            tools.Add(new XaiTool
            {
                Type = "web_search",
                Name = "web_search",
                AllowedDomains = filters.WebAllowed,
                ExcludedDomains = filters.WebExcluded,
                EnableImageUnderstanding = filters.EnableImageUnderstanding ? true : null
            });
        if (tool is "x" or "both")
            tools.Add(new XaiTool
            {
                Type = "x_search",
                Name = "x_search",
                AllowedXHandles = filters.XAllowed,
                ExcludedXHandles = filters.XExcluded,
                FromDate = filters.XFromDate,
                ToDate = filters.XToDate,
                EnableImageUnderstanding = filters.EnableImageUnderstanding ? true : null,
                EnableVideoUnderstanding = filters.EnableVideoUnderstanding ? true : null
            });
        return tools;
    }

    public static CliOutput BuildOutput(string tool, string model, XaiResponse response)
    {
        var answer = ExtractAnswer(response);
        var citations = ExtractCitations(response);

        return new CliOutput
        {
            Tool = tool,
            Model = model,
            Answer = answer,
            Citations = citations,
            Id = string.IsNullOrEmpty(response.Id) ? null : response.Id
        };
    }

    public static string ParseModel(string[] args)
    {
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "--model")
            {
                if (i + 1 >= args.Length || args[i + 1].StartsWith("--"))
                    throw new CommandException("--model requires a value");

                return args[i + 1];
            }
        }

        return DefaultModel;
    }

    public static string ExtractAnswer(XaiResponse response)
    {
        foreach (var item in response.Output)
        {
            if (item.Content is null) continue;
            foreach (var block in item.Content)
            {
                if (block.Type == "output_text" && !string.IsNullOrEmpty(block.Text))
                    return block.Text;
            }
        }
        return string.Empty;
    }

    public static List<CliCitation> ExtractCitations(XaiResponse response)
    {
        var citations = new List<CliCitation>();
        foreach (var item in response.Output)
        {
            if (item.Content is null) continue;
            foreach (var block in item.Content)
            {
                if (block.Annotations is null) continue;
                foreach (var ann in block.Annotations)
                {
                    if (!string.IsNullOrEmpty(ann.Url))
                        citations.Add(new CliCitation { Url = ann.Url, Title = ann.Title });
                }
            }
        }
        return citations;
    }

    public static string SerializeOutput(CliOutput output)
    {
        return JsonSerializer.Serialize(output, AppJsonContext.Default.CliOutput);
    }

    public static string ParseTool(string[] args)
    {
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] is "--tool" && i + 1 < args.Length)
                return args[i + 1] switch
                {
                    "web" or "x" or "both" => args[i + 1],
                    var v => throw new CommandException($"invalid tool '{v}': must be web, x, or both")
                };
        }
        throw new CommandException("--tool (web|x|both) is required");
    }

    public static string[] GetPositionalArgs(string[] args)
    {
        var positional = new List<string>();
        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--tool":
                case "--allow-domain":
                case "--exclude-domain":
                case "--allow-handle":
                case "--exclude-handle":
                case "--from-date":
                case "--to-date":
                case "--model":
                    i++;
                    break;
                case "--enable-image-understanding":
                case "--enable-video-understanding":
                    break;
                default:
                    if (!args[i].StartsWith("--"))
                        positional.Add(args[i]);
                    break;
            }
        }
        return [.. positional];
    }

    public static ToolFilters ParseFilters(string tool, string[] args)
    {
        var filters = new ToolFilters();
        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--allow-domain" when i + 1 < args.Length:
                    (filters.WebAllowed ??= []).Add(args[++i]);
                    break;
                case "--exclude-domain" when i + 1 < args.Length:
                    (filters.WebExcluded ??= []).Add(args[++i]);
                    break;
                case "--allow-handle" when i + 1 < args.Length:
                    (filters.XAllowed ??= []).Add(args[++i]);
                    break;
                case "--exclude-handle" when i + 1 < args.Length:
                    (filters.XExcluded ??= []).Add(args[++i]);
                    break;
                case "--from-date" when i + 1 < args.Length:
                    filters.XFromDate = args[++i];
                    break;
                case "--to-date" when i + 1 < args.Length:
                    filters.XToDate = args[++i];
                    break;
                case "--enable-image-understanding":
                    filters.EnableImageUnderstanding = true;
                    break;
                case "--enable-video-understanding":
                    filters.EnableVideoUnderstanding = true;
                    break;
            }
        }

        ValidateFilters(tool, filters);
        return filters;
    }

    private static void ValidateFilters(string tool, ToolFilters filters)
    {
        if (filters.WebAllowed is { Count: > 0 } && filters.WebExcluded is { Count: > 0 })
            throw new CommandException("--allow-domain and --exclude-domain cannot be used together");

        if (filters.XAllowed is { Count: > 0 } && filters.XExcluded is { Count: > 0 })
            throw new CommandException("--allow-handle and --exclude-handle cannot be used together");

        if (filters.WebAllowed is { Count: > 5 } || filters.WebExcluded is { Count: > 5 })
            throw new CommandException("web domain filters support at most 5 values");

        if (filters.XAllowed is { Count: > 10 } || filters.XExcluded is { Count: > 10 })
            throw new CommandException("X handle filters support at most 10 values");

        if (tool == "web" && (filters.XAllowed != null || filters.XExcluded != null || filters.XFromDate != null || filters.XToDate != null || filters.EnableVideoUnderstanding))
            throw new CommandException("X-specific filters require --tool x or --tool both");

        if (tool == "x" && (filters.WebAllowed != null || filters.WebExcluded != null))
            throw new CommandException("web-specific filters require --tool web or --tool both");
    }
}

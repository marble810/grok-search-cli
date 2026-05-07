using System.Text.Json;
using XaiSearchCli.Models;

namespace XaiSearchCli;

public class CommandException : Exception
{
    public CommandException(string message) : base(message) { }
}

public class ToolFilters
{
    public List<string>? WebAllowed { get; set; }
    public List<string>? WebExcluded { get; set; }
    public List<string>? XAllowed { get; set; }
    public List<string>? XExcluded { get; set; }
    public string? XFromDate { get; set; }
    public string? XToDate { get; set; }
}

public static class CliLogic
{
    public const string Model = "grok-4-1-fast-reasoning";

    public static List<XaiTool> BuildTools(string tool, ToolFilters filters)
    {
        var tools = new List<XaiTool>();
        if (tool is "web" or "both")
            tools.Add(new XaiTool
            {
                Type = "web_search",
                Name = "web_search",
                SearchFilters = filters.WebAllowed != null || filters.WebExcluded != null
                    ? new WebSearchFilters
                    {
                        AllowedDomains = filters.WebAllowed,
                        ExcludedDomains = filters.WebExcluded
                    }
                    : null,
                UserHandleFilters = null
            });
        if (tool is "x" or "both")
            tools.Add(new XaiTool
            {
                Type = "x_search",
                Name = "x_search",
                UserHandleFilters = filters.XAllowed != null || filters.XExcluded != null || filters.XFromDate != null || filters.XToDate != null
                    ? new XSearchUserFilters
                    {
                        Allowed = filters.XAllowed,
                        Excluded = filters.XExcluded,
                        FromDate = filters.XFromDate,
                        ToDate = filters.XToDate
                    }
                    : null,
                SearchFilters = null
            });
        return tools;
    }

    public static CliOutput BuildOutput(string tool, XaiResponse response)
    {
        var answer = ExtractAnswer(response);
        var citations = ExtractCitations(response);

        return new CliOutput
        {
            Tool = tool,
            Model = Model,
            Answer = answer,
            Citations = citations,
            Id = string.IsNullOrEmpty(response.Id) ? null : response.Id
        };
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
                    i++;
                    break;
                default:
                    if (!args[i].StartsWith("--"))
                        positional.Add(args[i]);
                    break;
            }
        }
        return [.. positional];
    }

    public static ToolFilters ParseFilters(string[] args)
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
            }
        }
        return filters;
    }
}

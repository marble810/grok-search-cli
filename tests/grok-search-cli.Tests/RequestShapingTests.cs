using XaiSearchCli;
using XaiSearchCli.Models;

namespace XaiSearchCli.Tests;

public class RequestShapingTests
{
    [Fact]
    public void WebTool_CreatesOnlyWebSearchTool()
    {
        var filters = new ToolFilters();
        var tools = CliLogic.BuildTools("web", filters);

        Assert.Single(tools);
        Assert.Equal("web_search", tools[0].Type);
        Assert.Null(tools[0].UserHandleFilters);
    }

    [Fact]
    public void XTool_CreatesOnlyXSearchTool()
    {
        var filters = new ToolFilters();
        var tools = CliLogic.BuildTools("x", filters);

        Assert.Single(tools);
        Assert.Equal("x_search", tools[0].Type);
        Assert.Null(tools[0].SearchFilters);
    }

    [Fact]
    public void BothTool_CreatesBothSearchTools()
    {
        var filters = new ToolFilters();
        var tools = CliLogic.BuildTools("both", filters);

        Assert.Equal(2, tools.Count);
        Assert.Equal("web_search", tools[0].Type);
        Assert.Equal("x_search", tools[1].Type);
    }

    [Fact]
    public void WebSearchWithDomainFilters_IncludesSearchFilters()
    {
        var filters = new ToolFilters { WebAllowed = ["example.com"] };
        var tools = CliLogic.BuildTools("web", filters);

        var webTool = tools[0];
        Assert.NotNull(webTool.SearchFilters);
        Assert.Equal(["example.com"], webTool.SearchFilters!.AllowedDomains);
    }

    [Fact]
    public void XSearchWithHandleFilters_IncludesUserFilters()
    {
        var filters = new ToolFilters { XExcluded = ["spam"] };
        var tools = CliLogic.BuildTools("x", filters);

        var xTool = tools[0];
        Assert.NotNull(xTool.UserHandleFilters);
        Assert.Equal(["spam"], xTool.UserHandleFilters!.Excluded);
    }

    [Fact]
    public void XSearchWithDateFilters_IncludesDateRange()
    {
        var filters = new ToolFilters { XFromDate = "2026-01-01", XToDate = "2026-01-31" };
        var tools = CliLogic.BuildTools("x", filters);

        var xTool = tools[0];
        Assert.NotNull(xTool.UserHandleFilters);
        Assert.Equal("2026-01-01", xTool.UserHandleFilters!.FromDate);
        Assert.Equal("2026-01-31", xTool.UserHandleFilters.ToDate);
    }

    [Fact]
    public void ParseTool_ReturnsWebForWebArg()
    {
        var tool = CliLogic.ParseTool(["--tool", "web"]);
        Assert.Equal("web", tool);
    }

    [Fact]
    public void ParseTool_ThrowsForUnknownTool()
    {
        Assert.Throws<CommandException>(() =>
            CliLogic.ParseTool(["--tool", "invalid"]));
    }

    [Fact]
    public void ParseTool_ThrowsWhenMissing()
    {
        Assert.Throws<CommandException>(() =>
            CliLogic.ParseTool([]));
    }

    [Fact]
    public void GetPositionalArgs_ExcludesFlagsAndValues()
    {
        var result = CliLogic.GetPositionalArgs(
            ["--tool", "web", "--allow-domain", "example.com", "my search query"]);

        Assert.Equal(["my search query"], result);
    }

    [Fact]
    public void GetPositionalArgs_ReturnsEmptyForNoPositional()
    {
        var result = CliLogic.GetPositionalArgs(
            ["--tool", "web"]);

        Assert.Empty(result);
    }

    [Fact]
    public void ParseFilters_WithDomainFilters()
    {
        var filters = CliLogic.ParseFilters(
            ["--tool", "web", "--allow-domain", "good.com", "--exclude-domain", "bad.com"]);

        Assert.Equal(["good.com"], filters.WebAllowed);
        Assert.Equal(["bad.com"], filters.WebExcluded);
    }

    [Fact]
    public void ParseFilters_WithHandleFilters()
    {
        var filters = CliLogic.ParseFilters(
            ["--tool", "x", "--allow-handle", "author", "--exclude-handle", "troll"]);

        Assert.Equal(["author"], filters.XAllowed);
        Assert.Equal(["troll"], filters.XExcluded);
    }

    [Fact]
    public void ParseFilters_WithDateFilters()
    {
        var filters = CliLogic.ParseFilters(
            ["--tool", "x", "--from-date", "2026-01-01", "--to-date", "2026-01-31"]);

        Assert.Equal("2026-01-01", filters.XFromDate);
        Assert.Equal("2026-01-31", filters.XToDate);
    }
}

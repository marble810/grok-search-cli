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
        Assert.Null(tools[0].AllowedXHandles);
        Assert.Null(tools[0].ExcludedXHandles);
    }

    [Fact]
    public void XTool_CreatesOnlyXSearchTool()
    {
        var filters = new ToolFilters();
        var tools = CliLogic.BuildTools("x", filters);

        Assert.Single(tools);
        Assert.Equal("x_search", tools[0].Type);
        Assert.Null(tools[0].AllowedDomains);
        Assert.Null(tools[0].ExcludedDomains);
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
        Assert.Equal(["example.com"], webTool.AllowedDomains);
    }

    [Fact]
    public void XSearchWithHandleFilters_IncludesUserFilters()
    {
        var filters = new ToolFilters { XExcluded = ["spam"] };
        var tools = CliLogic.BuildTools("x", filters);

        var xTool = tools[0];
        Assert.Equal(["spam"], xTool.ExcludedXHandles);
    }

    [Fact]
    public void XSearchWithDateFilters_IncludesDateRange()
    {
        var filters = new ToolFilters { XFromDate = "2026-01-01", XToDate = "2026-01-31" };
        var tools = CliLogic.BuildTools("x", filters);

        var xTool = tools[0];
        Assert.Equal("2026-01-01", xTool.FromDate);
        Assert.Equal("2026-01-31", xTool.ToDate);
    }

    [Fact]
    public void ParseModel_ReturnsDefaultWhenMissing()
    {
        var model = CliLogic.ParseModel(["--tool", "web"]);

        Assert.Equal(CliLogic.DefaultModel, model);
    }

    [Fact]
    public void ParseModel_ReturnsExplicitValue()
    {
        var model = CliLogic.ParseModel(["--tool", "web", "--model", "grok-4.1"]);

        Assert.Equal("grok-4.1", model);
    }

    [Fact]
    public void ParseModel_ThrowsWhenValueMissing()
    {
        Assert.Throws<CommandException>(() =>
            CliLogic.ParseModel(["--tool", "web", "--model"]));
    }

    [Fact]
    public void WebSearchWithImageUnderstanding_SetsFlag()
    {
        var filters = CliLogic.ParseFilters("web", ["--tool", "web", "--enable-image-understanding"]);
        var tools = CliLogic.BuildTools("web", filters);

        Assert.True(tools[0].EnableImageUnderstanding);
    }

    [Fact]
    public void XSearchWithMediaUnderstanding_SetsFlags()
    {
        var filters = CliLogic.ParseFilters("x", ["--tool", "x", "--enable-image-understanding", "--enable-video-understanding"]);
        var tools = CliLogic.BuildTools("x", filters);

        Assert.True(tools[0].EnableImageUnderstanding);
        Assert.True(tools[0].EnableVideoUnderstanding);
    }

    [Fact]
    public void ParseFilters_RejectsMutuallyExclusiveWebFilters()
    {
        Assert.Throws<CommandException>(() =>
            CliLogic.ParseFilters("web", ["--tool", "web", "--allow-domain", "good.com", "--exclude-domain", "bad.com"]));
    }

    [Fact]
    public void ParseFilters_RejectsMutuallyExclusiveXFilters()
    {
        Assert.Throws<CommandException>(() =>
            CliLogic.ParseFilters("x", ["--tool", "x", "--allow-handle", "good", "--exclude-handle", "bad"]));
    }

    [Fact]
    public void ParseFilters_RejectsXOnlyFlagsForWebTool()
    {
        Assert.Throws<CommandException>(() =>
            CliLogic.ParseFilters("web", ["--tool", "web", "--enable-video-understanding"]));
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
        var filters = CliLogic.ParseFilters("web",
            ["--tool", "web", "--allow-domain", "good.com"]);

        Assert.Equal(["good.com"], filters.WebAllowed);
        Assert.Null(filters.WebExcluded);
    }

    [Fact]
    public void ParseFilters_WithHandleFilters()
    {
        var filters = CliLogic.ParseFilters("x",
            ["--tool", "x", "--allow-handle", "author"]);

        Assert.Equal(["author"], filters.XAllowed);
        Assert.Null(filters.XExcluded);
    }

    [Fact]
    public void ParseFilters_WithDateFilters()
    {
        var filters = CliLogic.ParseFilters("x",
            ["--tool", "x", "--from-date", "2026-01-01", "--to-date", "2026-01-31"]);

        Assert.Equal("2026-01-01", filters.XFromDate);
        Assert.Equal("2026-01-31", filters.XToDate);
    }
}

using System.Text.Json;
using XaiSearchCli;
using XaiSearchCli.Models;

namespace XaiSearchCli.Tests;

public class OutputContractTests
{
    [Fact]
    public void BuildOutput_IncludesToolModelAnswerAndCitations()
    {
        var response = new XaiResponse
        {
            Id = "resp_123",
            Output =
            [
                new XaiOutputItem
                {
                    Content =
                    [
                        new XaiContentBlock
                        {
                            Type = "output_text",
                            Text = "the answer",
                            Annotations =
                            [
                                new XaiAnnotation { Type = "url_citation", Url = "https://example.com", Title = "Example" }
                            ]
                        }
                    ]
                }
            ]
        };

        var output = CliLogic.BuildOutput("web", response);

        Assert.Equal("web", output.Tool);
        Assert.Equal("grok-4-1-fast-reasoning", output.Model);
        Assert.Equal("the answer", output.Answer);
        Assert.Single(output.Citations);
        Assert.Equal("https://example.com", output.Citations[0].Url);
        Assert.Equal("Example", output.Citations[0].Title);
        Assert.Equal("resp_123", output.Id);
    }

    [Fact]
    public void BuildOutput_OmitsIdWhenNull()
    {
        var response = new XaiResponse
        {
            Id = "",
            Output =
            [
                new XaiOutputItem
                {
                    Content =
                    [
                        new XaiContentBlock { Type = "output_text", Text = "answer" }
                    ]
                }
            ]
        };

        var output = CliLogic.BuildOutput("x", response);
        Assert.Null(output.Id);
    }

    [Fact]
    public void SerializeOutput_ValidJson()
    {
        var output = new CliOutput
        {
            Tool = "web",
            Model = "grok-4-1-fast-reasoning",
            Answer = "test answer",
            Citations = [new CliCitation { Url = "https://x.ai", Title = "xAI" }],
            Id = "resp_1"
        };

        var json = CliLogic.SerializeOutput(output);
        var doc = JsonDocument.Parse(json);

        Assert.Equal("web", doc.RootElement.GetProperty("tool").GetString());
        Assert.Equal("test answer", doc.RootElement.GetProperty("answer").GetString());
        Assert.Single(doc.RootElement.GetProperty("citations").EnumerateArray());
        Assert.Equal("resp_1", doc.RootElement.GetProperty("id").GetString());
    }

    [Fact]
    public void ExtractAnswer_ReturnsEmptyForNoText()
    {
        var response = new XaiResponse
        {
            Id = "resp_1",
            Output =
            [
                new XaiOutputItem
                {
                    Content =
                    [
                        new XaiContentBlock { Type = "thinking", Text = "thinking..." }
                    ]
                }
            ]
        };

        var answer = CliLogic.ExtractAnswer(response);
        Assert.Empty(answer);
    }

    [Fact]
    public void ExtractCitations_ReturnsOnlyUrlCitations()
    {
        var response = new XaiResponse
        {
            Id = "resp_1",
            Output =
            [
                new XaiOutputItem
                {
                    Content =
                    [
                        new XaiContentBlock
                        {
                            Type = "output_text",
                            Text = "answer",
                            Annotations =
                            [
                                new XaiAnnotation { Type = "url_citation", Url = "https://a.com", Title = "A" },
                                new XaiAnnotation { Type = "other", Url = "", Title = null }
                            ]
                        }
                    ]
                }
            ]
        };

        var citations = CliLogic.ExtractCitations(response);
        Assert.Single(citations);
        Assert.Equal("https://a.com", citations[0].Url);
    }

    [Fact]
    public void BuildOutput_WithBothTools()
    {
        var response = new XaiResponse
        {
            Id = "resp_both",
            Output =
            [
                new XaiOutputItem
                {
                    Content =
                    [
                        new XaiContentBlock { Type = "output_text", Text = "combined result" }
                    ]
                }
            ]
        };

        var output = CliLogic.BuildOutput("both", response);
        Assert.Equal("both", output.Tool);
    }
}

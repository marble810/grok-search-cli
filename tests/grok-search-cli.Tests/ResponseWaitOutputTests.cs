using System.Net;
using System.Net.Http;
using System.Text;
using XaiSearchCli.Clients;
using XaiSearchCli.Models;

namespace XaiSearchCli.Tests;

public class ResponseWaitOutputTests
{
    [Fact]
    public async Task SearchAsync_EmitsWaitingMessageBeforeResponseCompletes()
    {
        var gate = new TaskCompletionSource<HttpResponseMessage>(TaskCreationOptions.RunContinuationsAsynchronously);
        var handler = new DelayedHandler(gate.Task);
        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api.x.ai/v1")
        };
        var client = new XaiClient(httpClient);
        var stderr = new StringWriter();

        var searchTask = client.SearchAsync(new XaiRequest
        {
            Model = CliLogic.DefaultModel,
            Input = "test query",
            Tools = [new XaiTool { Type = "web_search", Name = "web_search" }]
        }, stderr);

        await Task.Yield();
        Assert.Contains("waiting for model response...", stderr.ToString());

        gate.SetResult(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                """
                {"id":"resp_1","output":[{"content":[{"type":"output_text","text":"done"}]}]}
                """,
                Encoding.UTF8,
                "application/json")
        });

        var response = await searchTask;

        Assert.Equal("resp_1", response.Id);
        Assert.Equal("done", CliLogic.ExtractAnswer(response));
    }

    [Fact]
    public async Task SearchAsync_FailureAfterWaiting_ThrowsWithoutSkippingWaitingMessage()
    {
        var gate = new TaskCompletionSource<HttpResponseMessage>(TaskCreationOptions.RunContinuationsAsynchronously);
        var handler = new DelayedHandler(gate.Task);
        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api.x.ai/v1")
        };
        var client = new XaiClient(httpClient);
        var stderr = new StringWriter();

        var searchTask = client.SearchAsync(new XaiRequest
        {
            Model = CliLogic.DefaultModel,
            Input = "test query",
            Tools = [new XaiTool { Type = "web_search", Name = "web_search" }]
        }, stderr);

        await Task.Yield();
        Assert.Contains("waiting for model response...", stderr.ToString());

        gate.SetResult(new HttpResponseMessage(HttpStatusCode.BadGateway)
        {
            Content = new StringContent("upstream failed", Encoding.UTF8, "text/plain")
        });

        var ex = await Assert.ThrowsAsync<CommandException>(() => searchTask);

        Assert.Equal(2, ex.ExitCode);
        Assert.Contains("xAI API returned 502", ex.Message);
        Assert.Contains("upstream failed", stderr.ToString());
    }

    private sealed class DelayedHandler(Task<HttpResponseMessage> responseTask) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return responseTask;
        }
    }
}
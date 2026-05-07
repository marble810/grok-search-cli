using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using XaiSearchCli.Models;

namespace XaiSearchCli.Clients;

public class XaiClient
{
    private static readonly Uri BaseUri = new("https://api.x.ai/v1");
    private readonly HttpClient _http;

    public XaiClient(string apiKey)
        : this(CreateHttpClient(apiKey))
    {
    }

    public XaiClient(HttpClient httpClient)
    {
        _http = httpClient;
    }

    public async Task<XaiResponse> SearchAsync(XaiRequest request, TextWriter errorOutput)
    {
        var content = JsonContent.Create(request, AppJsonContext.Default.XaiRequest);
        var httpResponseTask = _http.PostAsync("/v1/responses", content);
        errorOutput.WriteLine("waiting for model response...");
        var httpResponse = await httpResponseTask;

        if (!httpResponse.IsSuccessStatusCode)
        {
            var body = await httpResponse.Content.ReadAsStringAsync();
            if (!string.IsNullOrEmpty(body))
                errorOutput.WriteLine(body);
            throw new CommandException($"xAI API returned {(int)httpResponse.StatusCode}", exitCode: 2);
        }

        var raw = await httpResponse.Content.ReadAsStringAsync();
        var response = JsonSerializer.Deserialize(raw, AppJsonContext.Default.XaiResponse);
        return response ?? throw new InvalidOperationException("xAI API returned empty response body.");
    }

    private static HttpClient CreateHttpClient(string apiKey)
    {
        return new HttpClient
        {
            BaseAddress = BaseUri,
            DefaultRequestHeaders =
            {
                Authorization = new AuthenticationHeaderValue("Bearer", apiKey)
            }
        };
    }
}

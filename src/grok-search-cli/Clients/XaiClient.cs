using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using XaiSearchCli.Models;

namespace XaiSearchCli.Clients;

public class XaiClient
{
    private static readonly Uri BaseUri = new("https://api.x.ai/v1");
    private readonly HttpClient _http;
    private readonly JsonSerializerOptions _json;

    public XaiClient(string apiKey)
    {
        _http = new HttpClient
        {
            BaseAddress = BaseUri,
            DefaultRequestHeaders =
            {
                Authorization = new AuthenticationHeaderValue("Bearer", apiKey)
            }
        };
        _json = new JsonSerializerOptions
        {
            TypeInfoResolver = AppJsonContext.Default
        };
    }

    public async Task<XaiResponse> SearchAsync(XaiRequest request, TextWriter errorOutput)
    {
        var content = JsonContent.Create(request, AppJsonContext.Default.XaiRequest);
        var httpResponse = await _http.PostAsync("/v1/responses", content);

        if (!httpResponse.IsSuccessStatusCode)
        {
            var body = await httpResponse.Content.ReadAsStringAsync();
            errorOutput.WriteLine($"error: xAI API returned {(int)httpResponse.StatusCode}");
            if (!string.IsNullOrEmpty(body))
                errorOutput.WriteLine(body);
            Environment.Exit(2);
        }

        var raw = await httpResponse.Content.ReadAsStringAsync();
        var response = JsonSerializer.Deserialize(raw, AppJsonContext.Default.XaiResponse);
        return response ?? throw new InvalidOperationException("xAI API returned empty response body.");
    }
}

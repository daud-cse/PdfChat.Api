using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace PdfChat.Api.Services;

public class OpenAiService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _apiKey;
    private readonly string _embeddingModel;
    private readonly string _chatModel;

    public OpenAiService(IHttpClientFactory httpClientFactory, IConfiguration config)
    {
        _httpClientFactory = httpClientFactory;
        _apiKey = config["OpenAI:ApiKey"] ?? throw new InvalidOperationException("OpenAI:ApiKey missing");
        _embeddingModel = config["OpenAI:EmbeddingModel"] ?? "text-embedding-3-small";
        _chatModel = config["OpenAI:ChatModel"] ?? "gpt-4o-mini";
    }

    public async Task<List<float[]>> EmbedAsync(List<string> inputs, CancellationToken ct)
    {
        var http = _httpClientFactory.CreateClient();
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

        var payload = new
        {
            model = _embeddingModel,
            input = inputs
        };

        var resp = await http.PostAsync(
            "https://api.openai.com/v1/embeddings",
            new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json"),
            ct);

        resp.EnsureSuccessStatusCode();

        using var stream = await resp.Content.ReadAsStreamAsync(ct);
        using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);

        var vectors = new List<float[]>();
        foreach (var item in doc.RootElement.GetProperty("data").EnumerateArray())
        {
            var arr = item.GetProperty("embedding").EnumerateArray().Select(e => (float)e.GetDouble()).ToArray();
            vectors.Add(arr);
        }
        return vectors;
    }

    public async Task<string> ChatAsync(string systemPrompt, string userPrompt, CancellationToken ct)
    {
        var http = _httpClientFactory.CreateClient();
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

        var payload = new
        {
            model = _chatModel,
            messages = new[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = userPrompt }
            },
            temperature = 0
        };

        var resp = await http.PostAsync(
            "https://api.openai.com/v1/chat/completions",
            new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json"),
            ct);

        resp.EnsureSuccessStatusCode();

        using var stream = await resp.Content.ReadAsStreamAsync(ct);
        using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);

        var content = doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        return content ?? string.Empty;
    }
}

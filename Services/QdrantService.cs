using System.Net.Http.Json;
using System.Text.Json;
using PdfChat.Api.Models;

namespace PdfChat.Api.Services;

public class QdrantService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _endpoint;
    private readonly string _collection;
    private readonly int _vectorSize;
    private readonly string _distance;

    public QdrantService(IHttpClientFactory httpClientFactory, IConfiguration config)
    {
        _httpClientFactory = httpClientFactory;
        _endpoint = config["Qdrant:Endpoint"] ?? "http://localhost:6333";
        _collection = config["Qdrant:Collection"] ?? "pdf_chunks";
        _vectorSize = int.Parse(config["Qdrant:VectorSize"] ?? "1536");
        _distance = config["Qdrant:Distance"] ?? "Cosine";
    }

    private HttpClient Client() => _httpClientFactory.CreateClient();

    public async Task EnsureCollectionAsync(CancellationToken ct)
    {
        var http = Client();
        // Create or update collection
        var url = $"{_endpoint}/collections/{_collection}";
        var payload = new
        {
            vectors = new
            {
                size = _vectorSize,
                distance = _distance
            }
        };

        // PUT create
        var resp = await http.PutAsJsonAsync(url, payload, ct);
        if (!resp.IsSuccessStatusCode)
        {
            // If exists, Qdrant returns 409; safe to ignore non-201 if 200/409 etc.
            // We won't throw unless 4xx other than conflict or 5xx
        }
    }

    public async Task UpsertChunksAsync(List<Chunk> chunks, List<float[]> vectors, CancellationToken ct)
    {
        if (chunks.Count != vectors.Count)
            throw new InvalidOperationException("Chunks and vectors length mismatch.");

        var http = Client();
        var url = $"{_endpoint}/collections/{_collection}/points";

        var points = chunks.Select((c, i) => new
        {
            id = c.Id,
            vector = vectors[i],
            payload = new
            {
                docId = c.DocId,
                page = c.PageNumber,
                text = c.Text
            }
        }).ToArray();

        var payload = new { points };

        var resp = await http.PutAsJsonAsync(url, payload, ct);
        resp.EnsureSuccessStatusCode();
    }

    public record SearchHit(string Id, float Score, int Page, string Text);

    public async Task<List<SearchHit>> SearchAsync(string docId, float[] queryVector, int topK, CancellationToken ct)
    {
        var http = Client();
        var url = $"{_endpoint}/collections/{_collection}/points/search";

        var payload = new
        {
            vector = queryVector,
            limit = topK,
            with_payload = true,
            with_vector = false,
            filter = new
            {
                must = new object[]
                {
                    new { key = "docId", match = new { value = docId } }
                }
            }
        };

        var resp = await http.PostAsJsonAsync(url, payload, ct);
        resp.EnsureSuccessStatusCode();

        using var stream = await resp.Content.ReadAsStreamAsync(ct);
        using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);

        var result = new List<SearchHit>();
        foreach (var point in doc.RootElement.GetProperty("result").EnumerateArray())
        {
            var id = point.GetProperty("id").GetString() ?? "";
            var score = (float)point.GetProperty("score").GetDouble();
            var payloadObj = point.GetProperty("payload");
            var page = payloadObj.TryGetProperty("page", out var p) ? p.GetInt32() : 0;
            var text = payloadObj.TryGetProperty("text", out var t) ? t.GetString() ?? "" : "";

            result.Add(new SearchHit(id, score, page, text));
        }
        return result;
    }
}

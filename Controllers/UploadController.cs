using Microsoft.AspNetCore.Mvc;
using PdfChat.Api.Models;
using PdfChat.Api.Services;

namespace PdfChat.Api.Controllers;

[ApiController]
[Route("api/upload")]
public class UploadController : ControllerBase
{
    private readonly PdfTextExtractor _extractor;
    private readonly TextChunker _chunker;
    private readonly OpenAiService _openAi;
    private readonly QdrantService _qdrant;

    public UploadController(PdfTextExtractor extractor, TextChunker chunker, OpenAiService openAi, QdrantService qdrant)
    {
        _extractor = extractor;
        _chunker = chunker;
        _openAi = openAi;
        _qdrant = qdrant;
    }

    [HttpPost]
    [RequestSizeLimit(100_000_000)] // ~100 MB
    public async Task<ActionResult<UploadResponse>> Upload(IFormFile file, CancellationToken ct)
    {
        if (file is null || file.Length == 0) return BadRequest("No file provided.");

        // Extract all pages text
        var pages = await _extractor.ExtractAsync(file, ct);
        var docId = Guid.NewGuid().ToString("N");

        // Build chunks with page metadata
        var chunks = _chunker.Split(pages, docId);

        // Ensure collection exists
        await _qdrant.EnsureCollectionAsync(ct);

        // Embed & upsert
        var texts = chunks.Select(c => c.Text).ToList();
        var vectors = await _openAi.EmbedAsync(texts, ct);

        await _qdrant.UpsertChunksAsync(chunks, vectors, ct);

        return Ok(new UploadResponse(docId, chunks.Count));
    }
}

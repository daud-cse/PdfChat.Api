using Microsoft.AspNetCore.Mvc;
using PdfChat.Api.Models;
using PdfChat.Api.Services;

namespace PdfChat.Api.Controllers;

[ApiController]
[Route("api/chat")]
public class ChatController : ControllerBase
{
    private readonly OpenAiService _openAi;
    private readonly QdrantService _qdrant;
    private readonly RagService _rag;

    public ChatController(OpenAiService openAi, QdrantService qdrant, RagService rag)
    {
        _openAi = openAi;
        _qdrant = qdrant;
        _rag = rag;
    }

    [HttpPost]
    public async Task<ActionResult<ChatResponse>> Ask([FromBody] ChatRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.DocId) || string.IsNullOrWhiteSpace(req.Question))
            return BadRequest("docId and question are required.");

        // Get top-K chunks for this docId
        var queryEmbedding = await _openAi.EmbedAsync(new List<string> { req.Question }, ct);
        var hits = await _qdrant.SearchAsync(req.DocId, queryEmbedding[0], topK: 4, ct);

        var response = await _rag.AnswerAsync(req.Question, hits, ct);
        return Ok(response);
    }
}

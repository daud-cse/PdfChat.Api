using System.Text;
using PdfChat.Api.Models;

namespace PdfChat.Api.Services;

public class RagService
{
    private readonly OpenAiService _openAi;

    public RagService(OpenAiService openAi)
    {
        _openAi = openAi;
    }

    public async Task<ChatResponse> AnswerAsync(string question, List<QdrantService.SearchHit> hits, CancellationToken ct)
    {
        // Compose context with chunk headers for citation
        var sb = new StringBuilder();
        for (int i = 0; i < hits.Count; i++)
        {
            var h = hits[i];
            sb.AppendLine($"[Chunk {i + 1} | Page {h.Page}]");
            sb.AppendLine(h.Text);
            sb.AppendLine();
        }

        var system = "You are a careful medical document assistant. Use ONLY the provided context to answer. " +
                     "If the answer is not present, say you don't know. Cite chunk numbers like [Chunk 2]. " +
                     "Keep explanations precise and non-clinical unless asked.";

        var user = $"Question: {question}\n\nContext:\n{sb}";

        var answer = await _openAi.ChatAsync(system, user, ct);

        var citations = hits.Select((h, i) => new Citation
        {
            Rank = i + 1,
            Page = h.Page.ToString(),
            Snippet = h.Text.Length > 240 ? h.Text[..240] + "…" : h.Text
        }).ToList();

        return new ChatResponse { Answer = answer, Citations = citations };
    }
}

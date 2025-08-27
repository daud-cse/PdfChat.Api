namespace PdfChat.Api.Models;

public record UploadResponse(string DocId, int ChunkCount);

public class ChatRequest
{
    public string DocId { get; set; } = default!;
    public string Question { get; set; } = default!;
}

public class ChatResponse
{
    public string Answer { get; set; } = default!;
    public List<Citation> Citations { get; set; } = new();
}

public class Citation
{
    public int Rank { get; set; }
    public string? Page { get; set; }
    public string Snippet { get; set; } = default!;
}

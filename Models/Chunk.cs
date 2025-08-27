namespace PdfChat.Api.Models;

public class Chunk
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string DocId { get; set; } = default!;
    public int PageNumber { get; set; }
    public string Text { get; set; } = default!;
}

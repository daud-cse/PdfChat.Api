using System.Text;
using PdfChat.Api.Models;

namespace PdfChat.Api.Services;

public class TextChunker
{
    public List<Chunk> Split(List<(int Page, string Text)> pages, string docId, int chunkSize = 1200, int overlap = 200)
    {
        var chunks = new List<Chunk>();

        foreach (var (page, raw) in pages)
        {
            var text = NormalizeWhitespace(raw);
            if (string.IsNullOrWhiteSpace(text)) continue;

            int start = 0;
            while (start < text.Length)
            {
                int len = Math.Min(chunkSize, text.Length - start);
                var segment = text.Substring(start, len);

                chunks.Add(new Chunk
                {
                    DocId = docId,
                    PageNumber = page,
                    Text = segment
                });

                if (start + len >= text.Length) break;
                start += (chunkSize - overlap);
                if (start < 0) break;
            }
        }

        return chunks;
    }

    private static string NormalizeWhitespace(string s)
    {
        var sb = new StringBuilder(s.Length);
        bool ws = false;
        foreach (var ch in s)
        {
            if (char.IsWhiteSpace(ch))
            {
                if (!ws) { sb.Append(' '); ws = true; }
            }
            else { sb.Append(ch); ws = false; }
        }
        return sb.ToString().Trim();
    }
}

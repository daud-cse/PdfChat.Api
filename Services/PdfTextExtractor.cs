using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace PdfChat.Api.Services;

public class PdfTextExtractor
{
    public async Task<List<(int Page, string Text)>> ExtractAsync(IFormFile file, CancellationToken ct)
    {
        // Save to temp
        var tmp = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.pdf");
        await using (var fs = new FileStream(tmp, FileMode.Create)) { await file.CopyToAsync(fs, ct); }

        var pages = new List<(int, string)>();
        using (var pdf = PdfDocument.Open(tmp))
        {
            foreach (var page in pdf.GetPages())
            {
                var text = page.Text ?? string.Empty;
                pages.Add((page.Number, text));
            }
        }

        try { File.Delete(tmp); } catch { /* ignore */ }
        return pages;
    }
}

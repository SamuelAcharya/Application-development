using System.Text;
using AppDevCoursework.Data;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;

namespace AppDevCoursework.Services
{
    public class PdfExportService
    {
        private readonly DatabaseService _databaseService;

        public PdfExportService(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        public async Task<byte[]> ExportJournalEntriesAsync(DateTime start, DateTime end)
        {
            var entries = await _databaseService.GetEntriesInRangeAsync(start, end);

            if (entries == null || entries.Count == 0)
            {
                return Array.Empty<byte>();
            }

            var document = new PdfDocument();
            document.Info.Title = "Journal Export";

            // Basic page/font setup
            var fontTitle = new XFont("Arial", 16, XFontStyle.Bold);
            var fontSubTitle = new XFont("Arial", 10, XFontStyle.Italic);
            var fontBody = new XFont("Arial", 11, XFontStyle.Regular);

            XGraphics gfx = null!;
            XRect rect;
            double margin = 40;
            double lineHeight = fontBody.GetHeight();
            double y = margin;

            void NewPage()
            {
                var page = document.AddPage();
                page.Size = PdfSharpCore.PageSize.A4;
                gfx = XGraphics.FromPdfPage(page);
                y = margin;

                // Header
                var header = $"Journal Export: {start:yyyy-MM-dd} to {end:yyyy-MM-dd}";
                rect = new XRect(margin, y, page.Width - 2 * margin, lineHeight * 2);
                gfx.DrawString(header, fontTitle, XBrushes.Black, rect, XStringFormats.TopLeft);
                y += lineHeight * 2.5;
            }

            void EnsureSpace(double needed)
            {
                if (gfx == null)
                {
                    NewPage();
                    return;
                }

                var page = gfx.PdfPage;
                if (y + needed > page.Height - margin)
                {
                    NewPage();
                }
            }

            NewPage();

            foreach (var entry in entries)
            {
                // Entry header
                var entryHeader = $"{entry.EntryDate:dddd, MMMM d, yyyy} - {entry.Title}";
                EnsureSpace(lineHeight * 4);
                rect = new XRect(margin, y, gfx.PdfPage.Width - 2 * margin, lineHeight * 2);
                gfx.DrawString(entryHeader, fontSubTitle, XBrushes.DarkBlue, rect, XStringFormats.TopLeft);
                y += lineHeight * 1.8;

                // Moods and tags
                var metaBuilder = new StringBuilder();
                if (!string.IsNullOrWhiteSpace(entry.PrimaryMood))
                {
                    metaBuilder.Append($"Mood: {entry.PrimaryMood}");
                }

                if (!string.IsNullOrWhiteSpace(entry.SecondaryMoods))
                {
                    if (metaBuilder.Length > 0) metaBuilder.Append(" | ");
                    metaBuilder.Append($"Secondary: {entry.SecondaryMoods}");
                }

                if (!string.IsNullOrWhiteSpace(entry.Tags))
                {
                    if (metaBuilder.Length > 0) metaBuilder.Append(" | ");
                    metaBuilder.Append($"Tags: {entry.Tags}");
                }

                if (metaBuilder.Length > 0)
                {
                    rect = new XRect(margin, y, gfx.PdfPage.Width - 2 * margin, lineHeight * 2);
                    gfx.DrawString(metaBuilder.ToString(), fontBody, XBrushes.Gray, rect, XStringFormats.TopLeft);
                    y += lineHeight * 1.5;
                }

                // Content (very simple word-wrapped text)
                var content = StripHtml(entry.Content);
                var wrappedLines = WrapText(gfx, content, fontBody, gfx.PdfPage.Width - 2 * margin);

                foreach (var line in wrappedLines)
                {
                    EnsureSpace(lineHeight * 1.2);
                    rect = new XRect(margin, y, gfx.PdfPage.Width - 2 * margin, lineHeight);
                    gfx.DrawString(line, fontBody, XBrushes.Black, rect, XStringFormats.TopLeft);
                    y += lineHeight * 1.1;
                }

                // Spacing between entries
                y += lineHeight * 1.5;
            }

            using var ms = new MemoryStream();
            document.Save(ms, false);
            return ms.ToArray();
        }

        private static string StripHtml(string htmlContent)
        {
            if (string.IsNullOrWhiteSpace(htmlContent)) return string.Empty;
            return System.Text.RegularExpressions.Regex.Replace(htmlContent, "<.*?>", string.Empty);
        }

        private static List<string> WrapText(XGraphics gfx, string text, XFont font, double maxWidth)
        {
            var lines = new List<string>();
            if (string.IsNullOrWhiteSpace(text))
            {
                return lines;
            }

            var words = text.Split(new[] { ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            var currentLine = new StringBuilder();

            foreach (var word in words)
            {
                var testLine = currentLine.Length == 0 ? word : currentLine + " " + word;
                var size = gfx.MeasureString(testLine, font);

                if (size.Width > maxWidth && currentLine.Length > 0)
                {
                    lines.Add(currentLine.ToString());
                    currentLine.Clear();
                    currentLine.Append(word);
                }
                else
                {
                    if (currentLine.Length > 0) currentLine.Append(' ');
                    currentLine.Append(word);
                }
            }

            if (currentLine.Length > 0)
            {
                lines.Add(currentLine.ToString());
            }

            return lines;
        }
    }
}


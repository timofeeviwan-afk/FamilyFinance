using UglyToad.PdfPig.Content;

namespace FamilyFinance.Import;

internal sealed record WordBox(string Text, double X, double Y, double Width, double Height)
{
    public double CenterX => X + Width / 2;
    public double CenterY => Y + Height / 2;
}

internal sealed record TextRow(IReadOnlyList<WordBox> Words)
{
    public string Text => string.Join(" ", Words.Select(w => w.Text));
}

internal static class PdfLayout
{
    public static List<WordBox> GetWords(Page page) =>
        page.GetWords()
            .Select(w => new WordBox(w.Text,
                w.BoundingBox.Left, w.BoundingBox.Bottom,
                w.BoundingBox.Width, w.BoundingBox.Height))
            .ToList();

    public static List<TextRow> GroupIntoRows(IEnumerable<WordBox> words, double yTolerance = 3.0)
    {
        var sorted = words.OrderByDescending(w => w.CenterY).ToList();
        var rows = new List<List<WordBox>>();
        foreach (var w in sorted)
        {
            var row = rows.LastOrDefault();
            if (row is null || Math.Abs(row[0].CenterY - w.CenterY) > yTolerance)
                rows.Add(new List<WordBox> { w });
            else
                row.Add(w);
        }
        return rows.Select(r => new TextRow(r.OrderBy(w => w.X).ToList())).ToList();
    }

    public static string Slice(TextRow row, double xMin, double xMax) =>
        string.Join(" ", row.Words
            .Where(w => w.CenterX >= xMin && w.CenterX < xMax)
            .Select(w => w.Text));
}
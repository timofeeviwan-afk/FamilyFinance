using System.Globalization;
using System.Text.RegularExpressions;
using UglyToad.PdfPig;

namespace FamilyFinance.Import.Parsers;

public sealed partial class AlfaPdfParser : IPdfStatementParser
{
    public string BankCode => "alfa";

    public bool CanParse(IReadOnlyList<string> pageTexts) =>
        pageTexts.Any(t => t.Contains("Операции по счету", StringComparison.OrdinalIgnoreCase)
                        || t.Contains("Выписка по счету", StringComparison.OrdinalIgnoreCase));

    public IReadOnlyList<ParsedTransaction> Parse(string filePath)
    {
        using var doc = PdfDocument.Open(filePath);
        var result = new List<ParsedTransaction>();

        foreach (var page in doc.GetPages())
        {
            var words = PdfLayout.GetWords(page);
            var rows = PdfLayout.GroupIntoRows(words);

            int headerIdx = rows.FindIndex(r =>
                r.Text.Contains("Дата проводки", StringComparison.OrdinalIgnoreCase) &&
                r.Text.Contains("Код операции", StringComparison.OrdinalIgnoreCase));
            if (headerIdx < 0) continue;

            var header = rows[headerIdx];
            double xDate = FindX(header, "Дата");
            double xCode = FindX(header, "Код");
            double xDesc = FindX(header, "Описание");
            double xAmt  = FindX(header, "Сумма");
            if (xDate < 0 || xCode < 0 || xDesc < 0 || xAmt < 0) continue;

            for (int i = headerIdx + 1; i < rows.Count; i++)
            {
                var row = rows[i];
                var first = row.Words.FirstOrDefault()?.Text ?? "";
                if (!DateRegex().IsMatch(first)) continue;

                if (!DateOnly.TryParseExact(first, "dd.MM.yyyy",
                        CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
                    continue;

                var code = PdfLayout.Slice(row, xCode, xDesc).Trim();
                var desc = PdfLayout.Slice(row, xDesc, xAmt).Trim();
                var amtStr = PdfLayout.Slice(row, xAmt, double.MaxValue).Trim();

                if (!TryParseAmount(amtStr, out var amount, out var currency)) continue;

                result.Add(new ParsedTransaction
                {
                    Date = date,
                    Amount = amount,
                    Currency = currency,
                    Description = desc,
                    ExternalId = code,
                    SourceBank = BankCode,
                    SourcePage = page.Number,
                    SourceRow = i,
                });
            }
        }
        return result;
    }

    private static double FindX(TextRow header, string keyword)
    {
        var w = header.Words.FirstOrDefault(w =>
            w.Text.StartsWith(keyword, StringComparison.OrdinalIgnoreCase));
        return w?.X ?? -1;
    }

    [GeneratedRegex(@"^\d{2}\.\d{2}\.\d{4}$")]
    private static partial Regex DateRegex();

    [GeneratedRegex(@"(?<sign>[+\-])?\s*(?<num>[\d\s\u00A0]+[.,]\d{2})\s*(?<cur>RUR|RUB|USD|EUR|₽|\$|€)")]
    private static partial Regex AmountRegex();

    private static bool TryParseAmount(string s, out decimal amount, out string currency)
    {
        amount = 0; currency = "RUB";
        var m = AmountRegex().Match(s);
        if (!m.Success) return false;

        var sign = m.Groups["sign"].Value == "-" ? -1m : 1m;
        var num = m.Groups["num"].Value
            .Replace(" ", "").Replace("\u00A0", "").Replace(",", ".");
        currency = m.Groups["cur"].Value.ToUpperInvariant() switch
        {
            "RUR" or "RUB" or "₽" => "RUB",
            "USD" or "$" => "USD",
            "EUR" or "€" => "EUR",
            var o => o
        };
        if (!decimal.TryParse(num, NumberStyles.Number, CultureInfo.InvariantCulture, out var v))
            return false;
        amount = sign * v;
        return true;
    }
}
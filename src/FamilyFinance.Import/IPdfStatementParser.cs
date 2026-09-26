namespace FamilyFinance.Import;

public interface IPdfStatementParser
{
    string BankCode { get; }
    bool CanParse(IReadOnlyList<string> pageTexts);
    IReadOnlyList<ParsedTransaction> Parse(string filePath);
}
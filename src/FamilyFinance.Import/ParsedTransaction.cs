namespace FamilyFinance.Import;

public sealed record ParsedTransaction
{
    public required DateOnly Date { get; init; }
    public required decimal Amount { get; init; }
    public string Currency { get; init; } = "RUB";
    public string? Counterparty { get; init; }
    public required string Description { get; init; }
    public string? ExternalId { get; init; }
    public string? SourceBank { get; init; }
    public int SourcePage { get; init; }
    public int SourceRow { get; init; }
}
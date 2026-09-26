namespace FamilyFinance.Domain;

public class Transaction
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public Account? Account { get; set; }
    public DateOnly Date { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "RUB";
    public string? Counterparty { get; set; }
    public string Description { get; set; } = "";
    public int? CategoryId { get; set; }
    public Category? Category { get; set; }
    public string? ExternalId { get; set; }
    public string? ExternalHash { get; set; }
    public string? SourceBank { get; set; }
    public DateTime ImportedAt { get; set; } = DateTime.UtcNow;
}
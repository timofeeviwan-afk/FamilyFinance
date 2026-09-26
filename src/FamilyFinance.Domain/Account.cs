namespace FamilyFinance.Domain;

public class Account
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string? Bank { get; set; }
    public AccountType Type { get; set; }
    public string Currency { get; set; } = "RUB";
    public decimal OpeningBalance { get; set; }
    public bool IsActive { get; set; } = true;
}
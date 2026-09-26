namespace FamilyFinance.Domain;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public TransactionKind Kind { get; set; }
    public bool IsSystem { get; set; }
}
using FamilyFinance.Domain;
using Microsoft.EntityFrameworkCore;

namespace FamilyFinance.Data;

public class FinanceDbContext : DbContext
{
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<Category> Categories => Set<Category>();

    public FinanceDbContext(DbContextOptions<FinanceDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<Transaction>().HasIndex(t => t.ExternalHash);
        mb.Entity<Transaction>().HasIndex(t => t.Date);
        mb.Entity<Account>().HasIndex(a => a.Name);
    }
}
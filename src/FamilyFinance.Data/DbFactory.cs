using Microsoft.EntityFrameworkCore;

namespace FamilyFinance.Data;

public static class DbFactory
{
    public static FinanceDbContext Create(string dbPath)
    {
        var opts = new DbContextOptionsBuilder<FinanceDbContext>()
            .UseSqlite($"Data Source={dbPath}")
            .Options;
        return new FinanceDbContext(opts);
    }
}
using System.Globalization;
using System.Windows;
using FamilyFinance.Data;
using FamilyFinance.Domain;

namespace FamilyFinance.App;

public partial class AddTransactionWindow : Window
{
    private readonly FinanceDbContext _ctx;

    public AddTransactionWindow(FinanceDbContext ctx)
    {
        InitializeComponent();
        _ctx = ctx;
        Dp.SelectedDate = DateTime.Today;

        var accounts = _ctx.Accounts.ToList();
        if (accounts.Count == 0)
        {
            var cash = new Account { Name = "Наличные", Type = AccountType.Cash };
            _ctx.Accounts.Add(cash);
            _ctx.SaveChanges();
            accounts.Add(cash);
        }
        CbAccount.ItemsSource = accounts;
        CbAccount.SelectedIndex = 0;

        var cats = _ctx.Categories.ToList();
        if (cats.Count == 0)
        {
            _ctx.Categories.AddRange(
                new Category { Name = "Продукты", Kind = TransactionKind.Expense },
                new Category { Name = "Транспорт", Kind = TransactionKind.Expense },
                new Category { Name = "Жильё", Kind = TransactionKind.Expense },
                new Category { Name = "Здоровье", Kind = TransactionKind.Expense },
                new Category { Name = "Развлечения", Kind = TransactionKind.Expense },
                new Category { Name = "Одежда", Kind = TransactionKind.Expense },
                new Category { Name = "Инвестиции", Kind = TransactionKind.Expense },
                new Category { Name = "Зарплата", Kind = TransactionKind.Income },
                new Category { Name = "Прочее", Kind = TransactionKind.Expense });
            _ctx.SaveChanges();
            cats = _ctx.Categories.ToList();
        }
        CbCategory.ItemsSource = cats;
    }

    private void OnSave(object sender, RoutedEventArgs e)
    {
        if (Dp.SelectedDate is null) { MessageBox.Show("Укажите дату."); return; }

        var s = TbAmount.Text.Replace(" ", "").Replace(",", ".");
        if (!decimal.TryParse(s, NumberStyles.Number, CultureInfo.InvariantCulture, out var amount))
        {
            MessageBox.Show("Некорректная сумма.");
            return;
        }

        var account = (Account)CbAccount.SelectedItem;
        var category = (Category?)CbCategory.SelectedItem;

        _ctx.Transactions.Add(new Transaction
        {
            AccountId = account.Id,
            Date = DateOnly.FromDateTime(Dp.SelectedDate.Value),
            Amount = amount,
            Currency = account.Currency,
            CategoryId = category?.Id,
            Counterparty = string.IsNullOrWhiteSpace(TbCounterparty.Text) ? null : TbCounterparty.Text,
            Description = TbDescription.Text
        });
        _ctx.SaveChanges();
        DialogResult = true;
    }
}
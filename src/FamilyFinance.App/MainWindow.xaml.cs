using System.Collections.ObjectModel;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using FamilyFinance.Data;
using FamilyFinance.Domain;
using FamilyFinance.Import;
using FamilyFinance.Import.Parsers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;

namespace FamilyFinance.App;

public partial class MainWindow : Window
{
    private readonly FinanceDbContext _ctx;
    private readonly ObservableCollection<TransactionVm> _items = new();
    private readonly string _dbPath;

    public MainWindow(FinanceDbContext ctx)
    {
        InitializeComponent();
        _ctx = ctx;
        _dbPath = _ctx.Database.GetDbConnection().DataSource;
        Grid.ItemsSource = _items;
        Loaded += (_, _) => LoadData();
    }

    private void LoadData()
    {
        _items.Clear();
        var rows = _ctx.Transactions
            .Include(t => t.Category)
            .OrderByDescending(t => t.Date)
            .ThenByDescending(t => t.Id)
            .Take(1000).ToList();
        foreach (var t in rows) _items.Add(new TransactionVm(t));
        StatusText.Text = $"Записей: {_items.Count}";
    }

    private void OnAdd(object sender, RoutedEventArgs e)
    {
        var dlg = new AddTransactionWindow(_ctx);
        if (dlg.ShowDialog() == true) LoadData();
    }

    private void OnImport(object sender, RoutedEventArgs e)
    {
        var dlg = new OpenFileDialog { Filter = "PDF (*.pdf)|*.pdf" };
        if (dlg.ShowDialog() != true) return;

        try
        {
            IPdfStatementParser parser = new AlfaPdfParser();
            var parsed = parser.Parse(dlg.FileName);

            if (parsed.Count == 0)
            {
                MessageBox.Show("Не удалось распознать операции.");
                return;
            }

            var account = _ctx.Accounts.FirstOrDefault(a => a.Bank == parser.BankCode)
                ?? CreateDefaultAccount(parser.BankCode);

            int added = 0, skipped = 0;
            foreach (var p in parsed)
            {
                var hash = MakeHash(p, account.Id);
                if (_ctx.Transactions.Any(t => t.ExternalHash == hash))
                {
                    skipped++;
                    continue;
                }
                _ctx.Transactions.Add(new Transaction
                {
                    AccountId = account.Id,
                    Date = p.Date,
                    Amount = p.Amount,
                    Currency = p.Currency,
                    Counterparty = p.Counterparty,
                    Description = p.Description,
                    ExternalId = p.ExternalId,
                    ExternalHash = hash,
                    SourceBank = p.SourceBank,
                });
                added++;
            }
            _ctx.SaveChanges();
            LoadData();
            MessageBox.Show($"Импортировано: {added}, дубликатов: {skipped}");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка: {ex.Message}");
        }
    }

    private Account CreateDefaultAccount(string bankCode)
    {
        var acc = new Account
        {
            Name = bankCode == "alfa" ? "Альфа-Банк" : bankCode,
            Bank = bankCode,
            Type = AccountType.BankCard,
            Currency = "RUB"
        };
        _ctx.Accounts.Add(acc);
        _ctx.SaveChanges();
        return acc;
    }

    private static string MakeHash(ParsedTransaction p, int accountId)
    {
        var s = $"{accountId}|{p.Date:yyyy-MM-dd}|{p.Amount:F2}|{p.ExternalId ?? p.Description}";
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(s)));
    }

    private void OnReport(object sender, RoutedEventArgs e)
    {
        var rows = _ctx.Transactions
            .Include(t => t.Category)
            .AsEnumerable()
            .Where(t => t.Amount < 0)
            .GroupBy(t => t.Category?.Name ?? "(без категории)")
            .Select(g => new { Cat = g.Key, Total = g.Sum(t => -t.Amount), Count = g.Count() })
            .OrderByDescending(x => x.Total)
            .ToList();

        if (rows.Count == 0) { MessageBox.Show("Нет данных"); return; }

        var text = string.Join("\n", rows.Select(r =>
            $"{r.Cat,-25} {r.Total,12:N2} руб.   ({r.Count})"));
        MessageBox.Show(text, "Расходы по категориям");
    }

    private void OnBackup(object sender, RoutedEventArgs e)
    {
        var dlg = new SaveFileDialog
        {
            Filter = "Backup (*.db)|*.db",
            FileName = $"finance_backup_{DateTime.Now:yyyy-MM-dd_HHmm}.db"
        };
        if (dlg.ShowDialog() != true) return;
        try
        {
            _ctx.Database.CloseConnection();
            File.Copy(_dbPath, dlg.FileName, overwrite: true);
            MessageBox.Show("Бэкап сохранён.");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка: {ex.Message}");
        }
    }
    private void OnDebugPdf(object sender, RoutedEventArgs e)
{
    var dlg = new OpenFileDialog { Filter = "PDF (*.pdf)|*.pdf" };
    if (dlg.ShowDialog() != true) return;

    try
    {
        using var doc = UglyToad.PdfPig.PdfDocument.Open(dlg.FileName);
        var sb = new System.Text.StringBuilder();

        sb.AppendLine($"=== Файл: {System.IO.Path.GetFileName(dlg.FileName)} ===");
        sb.AppendLine($"=== Страниц: {doc.NumberOfPages} ===");

        foreach (var page in doc.GetPages())
        {
            sb.AppendLine();
            sb.AppendLine($"--- Страница {page.Number} ---");
            var words = page.GetWords()
                .Select(w => new { w.Text, X = w.BoundingBox.Left, Y = w.BoundingBox.Bottom })
                .OrderByDescending(w => w.Y).ThenBy(w => w.X)
                .ToList();

            sb.AppendLine($"Всего слов: {words.Count}");
            foreach (var w in words)
                sb.AppendLine($"[X={w.X,7:F1} Y={w.Y,7:F1}] {w.Text}");
        }

        var outPath = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            "pdf_debug.txt");
        System.IO.File.WriteAllText(outPath, sb.ToString(), System.Text.Encoding.UTF8);
        MessageBox.Show($"Дамп сохранён в:\n{outPath}", "Отладка PDF");
    }
    catch (Exception ex)
    {
        MessageBox.Show($"Ошибка: {ex.Message}");
    }
}
}

public sealed class TransactionVm
{
    public string Date { get; }
    public decimal Amount { get; }
    public string Currency { get; }
    public string CategoryName { get; }
    public string? Counterparty { get; }
    public string Description { get; }

    public TransactionVm(Transaction t)
    {
        Date = t.Date.ToString("dd.MM.yyyy");
        Amount = t.Amount;
        Currency = t.Currency;
        CategoryName = t.Category?.Name ?? "";
        Counterparty = t.Counterparty;
        Description = t.Description;
    }
}

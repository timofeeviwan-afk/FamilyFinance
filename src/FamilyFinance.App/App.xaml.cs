using System.IO;
using System.Windows;
using FamilyFinance.App.Services;
using FamilyFinance.Data;

namespace FamilyFinance.App;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        ShutdownMode = ShutdownMode.OnExplicitShutdown;

        var dir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "FamilyFinance");
        Directory.CreateDirectory(dir);
        var dbPath = Path.Combine(dir, "finance.db");
        var pwdFile = Path.Combine(dir, "pwd.hash");

        var pwd = new PasswordService(pwdFile);

        if (!pwd.IsInitialized)
        {
            var setup = new PasswordSetupWindow();
            if (setup.ShowDialog() != true) { Shutdown(); return; }
            pwd.SetPassword(setup.Password);
        }
        else
        {
            var login = new LoginWindow(pwd);
            if (login.ShowDialog() != true) { Shutdown(); return; }
        }

        var ctx = DbFactory.Create(dbPath);
        ctx.Database.EnsureCreated();

        var main = new MainWindow(ctx);
        MainWindow = main;
        ShutdownMode = ShutdownMode.OnMainWindowClose;
        main.Show();
    }
}
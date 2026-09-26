using System.Windows;
using FamilyFinance.App.Services;

namespace FamilyFinance.App;

public partial class LoginWindow : Window
{
    private readonly PasswordService _pwd;
    public LoginWindow(PasswordService pwd)
    {
        InitializeComponent();
        _pwd = pwd;
    }

    private void OnLogin(object sender, RoutedEventArgs e)
    {
        if (!_pwd.Verify(Pwd.Password))
        {
            MessageBox.Show("Неверный пароль.");
            return;
        }
        DialogResult = true;
    }
}
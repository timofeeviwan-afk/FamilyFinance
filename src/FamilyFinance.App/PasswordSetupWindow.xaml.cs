using System.Windows;

namespace FamilyFinance.App;

public partial class PasswordSetupWindow : Window
{
    public string Password { get; private set; } = "";
    public PasswordSetupWindow() => InitializeComponent();

    private void OnCreate(object sender, RoutedEventArgs e)
    {
        if (Pwd1.Password.Length < 4)
        {
            MessageBox.Show("Пароль должен быть не короче 4 символов.");
            return;
        }
        if (Pwd1.Password != Pwd2.Password)
        {
            MessageBox.Show("Пароли не совпадают.");
            return;
        }
        Password = Pwd1.Password;
        DialogResult = true;
    }
}
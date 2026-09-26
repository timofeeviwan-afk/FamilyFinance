using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace FamilyFinance.App.Services;

public sealed class PasswordService
{
    private readonly string _file;
    public PasswordService(string file) => _file = file;

    public bool IsInitialized => File.Exists(_file);

    public void SetPassword(string password) =>
        File.WriteAllText(_file, Hash(password));

    public bool Verify(string password)
    {
        if (!File.Exists(_file)) return false;
        var stored = File.ReadAllText(_file).Trim();
        var calc = Hash(password);
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(stored), Encoding.UTF8.GetBytes(calc));
    }

    private static string Hash(string password)
    {
        const string salt = "FamilyFinance_v1";
        return Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(salt + password)));
    }
}
using ATGSaveGameManager.Configuration;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

public class WindowsSecureStorage : ISecureStorage
{
    public Task StoreAsync(string key, string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        var protectedBytes = ProtectedData.Protect(
            bytes,
            null,
            DataProtectionScope.CurrentUser);

        File.WriteAllBytes(GetPath(key), protectedBytes);
        return Task.CompletedTask;
    }

    public Task<string?> RetrieveAsync(string key)
    {
        var path = GetPath(key);

        if (!File.Exists(path))
            return Task.FromResult<string?>(null);

        var protectedBytes = File.ReadAllBytes(path);
        var bytes = ProtectedData.Unprotect(
            protectedBytes,
            null,
            DataProtectionScope.CurrentUser);

        return Task.FromResult<string?>(Encoding.UTF8.GetString(bytes));
    }

    private string GetPath(string key)
    {
        var directory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "ATGSavegameManager");

        Directory.CreateDirectory(directory);

        return Path.Combine(directory, key);
    }
}

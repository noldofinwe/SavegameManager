using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
namespace ATGSaveGameManager.Configuration;


#if WINDOWS

public class WindowsSecureStorage : ISecureStorage
{
    public Task StoreAsync(string key, string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        var protectedBytes = ProtectedData.Protect(bytes, null, DataProtectionScope.CurrentUser);
        File.WriteAllBytes(GetPath(key), protectedBytes);
        return Task.CompletedTask;
    }

    public Task<string?> RetrieveAsync(string key)
    {
        if (!File.Exists(GetPath(key)))
            return Task.FromResult<string?>(null);

        var protectedBytes = File.ReadAllBytes(GetPath(key));
        var bytes = ProtectedData.Unprotect(protectedBytes, null, DataProtectionScope.CurrentUser);
        return Task.FromResult<string?>(Encoding.UTF8.GetString(bytes));
    }

    private string GetPath(string key) =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MyApp", key);
}
#endif

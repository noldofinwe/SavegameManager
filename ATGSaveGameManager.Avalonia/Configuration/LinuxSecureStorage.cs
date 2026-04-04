using System.Diagnostics;
using System.Threading.Tasks;

namespace ATGSaveGameManager.Configuration;

public class LinuxSecureStorage : ISecureStorage
{
    public async Task StoreAsync(string key, string value)
    {
        var psi = new ProcessStartInfo("secret-tool", $"store --label=\"{key}\" app yourapp key {key}")
        {
            RedirectStandardInput = true
        };

        var p = Process.Start(psi);
        await p.StandardInput.WriteAsync(value);
        p.StandardInput.Close();
        await p.WaitForExitAsync();
    }

    public async Task<string?> RetrieveAsync(string key)
    {
        var psi = new ProcessStartInfo("secret-tool", $"lookup app yourapp key {key}")
        {
            RedirectStandardOutput = true
        };

        var p = Process.Start(psi);
        var output = await p.StandardOutput.ReadToEndAsync();
        await p.WaitForExitAsync();

        return string.IsNullOrWhiteSpace(output) ? null : output.Trim();
    }
    
}

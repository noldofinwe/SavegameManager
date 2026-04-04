using System.Threading.Tasks;

namespace ATGSaveGameManager.Configuration;

public interface ISecureStorage
{
    Task StoreAsync(string key, string value);
    Task<string?> RetrieveAsync(string key);
}

using System;

namespace ATGSaveGameManager.Configuration;

public static class SecureStorageFactory
{
    public static ISecureStorage Create()
    {
#if WINDOWS
        return new WindowsSecureStorage();
#endif
        return new LinuxSecureStorage();

        throw new PlatformNotSupportedException();
    }
}
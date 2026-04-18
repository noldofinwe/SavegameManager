using System;

namespace ATGSaveGameManager.Configuration;

public static class SecureStorageFactory
{
    public static ISecureStorage Create()
    {
        if (OperatingSystem.IsWindows())
        {
            return new WindowsSecureStorage();
        }
        return new LinuxSecureStorage();

        //throw new PlatformNotSupportedException();
    }
}
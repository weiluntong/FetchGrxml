using System;
using FileServerCoreSDK.Interfaces;

namespace FileServerCoreSDK.Configuration;

public class DependencyInjection
{
    public static void AddFileServerCoreSDK(Action<FileServerOptions> configure)
    {
        // Stub - does nothing
    }
}

public class FileServerOptions
{
    public void SetLogger(IFileServerLogHelper logger)
    {
        // Stub - does nothing
    }
}

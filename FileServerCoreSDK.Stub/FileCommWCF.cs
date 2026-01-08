using System;
using FileServerCoreSDK.Definitions;

namespace FileServerCoreSDK;

public class FileCommWCF : IDisposable
{
    public FileCommWCF(string server, string appName)
    {
        // Stub constructor
    }

    public ListResults? GetDirectoryListing(
        int busNo, 
        string path, 
        string pattern,
        bool includeDeleted,
        bool foldersOnly,
        bool filesOnly)
    {
        throw new NotImplementedException("This is a stub implementation for CI/CD builds only");
    }

    public FSResult GetFileFromServer(int busNo, string remotePath, string localPath)
    {
        throw new NotImplementedException("This is a stub implementation for CI/CD builds only");
    }

    public void Dispose()
    {
        // Stub dispose
    }
}

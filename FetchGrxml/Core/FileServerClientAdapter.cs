using FileServerCoreSDK;
using FileServerCoreSDK.Definitions;

namespace FetchGrxml;

/// <summary>
/// Adapter wrapping FileCommWCF to implement IFileServerClient
/// </summary>
public class FileServerClientAdapter : IFileServerClient
{
    private readonly FileCommWCF _client;

    public FileServerClientAdapter(FileCommWCF client)
    {
        _client = client;
    }

    public ListResults? GetDirectoryListing(int busNo, string path, string pattern,
        bool includeDeleted, bool foldersOnly, bool filesOnly)
    {
        return _client.GetDirectoryListing(busNo, path, pattern, includeDeleted, foldersOnly, filesOnly);
    }

    public FSResult GetFileFromServer(int busNo, string remotePath, string localPath)
    {
        return _client.GetFileFromServer(busNo, remotePath, localPath);
    }
}

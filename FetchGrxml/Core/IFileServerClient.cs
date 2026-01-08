using FileServerCoreSDK;
using FileServerCoreSDK.Definitions;

namespace FetchGrxml;

/// <summary>
/// Abstraction over FileServerCoreSDK for testing
/// </summary>
public interface IFileServerClient
{
    ListResults? GetDirectoryListing(int busNo, string path, string pattern,
        bool includeDeleted, bool foldersOnly, bool filesOnly);
    
    FSResult GetFileFromServer(int busNo, string remotePath, string localPath);
}

using FileServerCoreSDK;

namespace FetchGrxml;

/// <summary>
/// Recursively scans FileServer directories for .grxml files
/// </summary>
public class FileScanner
{
    private readonly IFileServerClient _fileClient;
    private readonly RequestThrottler _throttler;
    private readonly List<string> _exclusionPatterns;

    public FileScanner(IFileServerClient fileClient, RequestThrottler throttler, List<string> exclusionPatterns)
    {
        _fileClient = fileClient;
        _throttler = throttler;
        _exclusionPatterns = exclusionPatterns;
    }

    public async Task<List<string>> ScanForGrxmlFiles(int busNo, string startPath = "/")
    {
        var allFiles = new List<string>();
        await ScanDirectoryRecursive(busNo, startPath, allFiles);
        return allFiles;
    }

    private async Task ScanDirectoryRecursive(int busNo, string path, List<string> accumulator)
    {
        try
        {
            // Throttle and fetch directory listing
            var result = await _throttler.ExecuteAsync(() => 
                _fileClient.GetDirectoryListing(busNo, path, "*",
                    includeDeleted: false,
                    foldersOnly: false,
                    filesOnly: false));
            
            if (result?.Files == null)
                return;

            foreach (var item in result.Files)
            {
                string fullPath = BuildFullPath(path, item.FileName);
                
                if (item.IsFolder)
                {
                    await ProcessFolder(busNo, fullPath, accumulator);
                }
                else if (IsGrxmlFile(fullPath))
                {
                    accumulator.Add(fullPath);
                }
            }
        }
        catch (Exception ex)
        {
            ConsoleUI.ShowDirectoryError(busNo, path, ex.Message);
        }
    }

    private async Task ProcessFolder(int busNo, string folderPath, List<string> accumulator)
    {
        if (ExclusionMatcher.ShouldExclude(folderPath, _exclusionPatterns))
        {
            ConsoleUI.ShowDirectorySkipped(busNo, folderPath);
            return;
        }

        await ScanDirectoryRecursive(busNo, folderPath, accumulator);
    }

    private static string BuildFullPath(string parentPath, string fileName)
    {
        if (parentPath == "/")
        {
            return "/" + fileName.TrimStart('/');
        }
        else
        {
            return parentPath.TrimEnd('/') + "/" + fileName.TrimStart('/');
        }
    }

    private static bool IsGrxmlFile(string path)
    {
        return path.EndsWith(".grxml", StringComparison.OrdinalIgnoreCase);
    }
}

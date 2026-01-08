using FileServerCoreSDK;
using FileServerCoreSDK.Definitions;

namespace FetchGrxml;

/// <summary>
/// Downloads files from FileServer
/// </summary>
public class FileDownloader
{
    private readonly IFileServerClient _fileClient;
    private readonly RequestThrottler _throttler;
    private readonly string _outputDirectory;

    public FileDownloader(IFileServerClient fileClient, RequestThrottler throttler, string outputDirectory)
    {
        _fileClient = fileClient;
        _throttler = throttler;
        _outputDirectory = outputDirectory;
    }

    public async Task<int> DownloadFiles(int busNo, List<string> files)
    {
        string buOutputDir = Path.Combine(_outputDirectory, $"BUS{busNo}");
        
        var downloadTasks = files.Select(fileName => 
            DownloadSingleFile(busNo, fileName, buOutputDir));
        
        var results = await Task.WhenAll(downloadTasks);
        return results.Count(success => success);
    }

    private async Task<bool> DownloadSingleFile(int busNo, string fileName, string outputDir)
    {
        try
        {
            string localPath = BuildLocalPath(fileName, outputDir);
            EnsureDirectoryExists(localPath);

            var result = await _throttler.ExecuteAsync(() => 
                _fileClient.GetFileFromServer(busNo, fileName, localPath));

            if (result.ActionResult == FileServerCodes.Success)
            {
                return true;
            }
            else
            {
                ConsoleUI.ShowDownloadError(fileName, result.ActionResult.ToString());
                return false;
            }
        }
        catch (Exception ex)
        {
            ConsoleUI.ShowDownloadError(fileName, ex.Message);
            return false;
        }
    }

    private static string BuildLocalPath(string fileName, string outputDir)
    {
        return Path.Combine(outputDir, fileName.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
    }

    private static void EnsureDirectoryExists(string filePath)
    {
        string? directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }
}

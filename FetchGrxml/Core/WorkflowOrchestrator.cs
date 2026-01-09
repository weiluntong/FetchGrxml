using FileServerCoreSDK;

namespace FetchGrxml;

/// <summary>
/// Orchestrates the workflow for processing business units
/// </summary>
public class WorkflowOrchestrator
{
    private readonly RequestThrottler _throttler;
    private readonly IFileServerClient _fileClient;
    private readonly string _outputDirectory;
    private readonly List<string> _exclusionPatterns;

    public WorkflowOrchestrator(
        IFileServerClient fileClient, 
        string outputDirectory, 
        List<string> exclusionPatterns)
    {
        _fileClient = fileClient;
        _outputDirectory = outputDirectory;
        _exclusionPatterns = exclusionPatterns;
        _throttler = new RequestThrottler();
    }

    public async Task<int> ProcessBusinessUnits(IEnumerable<int> businessUnits)
    {
        var scanner = new FileScanner(_fileClient, _throttler, _exclusionPatterns);
        var downloader = new FileDownloader(_fileClient, _throttler, _outputDirectory);
        int totalFiles = 0;

        foreach (int busNo in businessUnits)
        {
            totalFiles += await ProcessSingleBU(busNo, scanner, downloader);
            ConsoleUI.ShowBlankLine();
        }

        return totalFiles;
    }

    private async Task<int> ProcessSingleBU(int busNo, FileScanner scanner, FileDownloader downloader)
    {
        ConsoleUI.ShowProcessingBU(busNo);

        try
        {
            var files = await scanner.ScanForGrxmlFiles(busNo);
            ConsoleUI.ShowFilesFound(busNo, files.Count);

            int downloaded = await downloader.DownloadFiles(busNo, files);
            ConsoleUI.ShowDownloadProgress(busNo, downloaded, files.Count);

            return downloaded;
        }
        catch (Exception ex)
        {
            ConsoleUI.ShowBUError(busNo, ex.Message);
            return 0;
        }
    }
}

namespace FetchGrxml;

/// <summary>
/// Handles all console output for the application
/// </summary>
public static class ConsoleUI
{
    private static readonly ConsoleLogger _logger = new();

    public static void ShowBanner(string fileServerVip, string outputDirectory, string busListPath)
    {
        _logger.LogInfo("FetchGrxml - Grammar File Extraction Tool");
        _logger.LogInfo($"FileServer: {fileServerVip}");
        _logger.LogInfo($"Output Directory: {outputDirectory}");
        _logger.LogInfo($"BU List: {busListPath}");
        _logger.LogInfo("");
    }

    public static void ShowUsage()
    {
        _logger.LogInfo("FetchGrxml - Grammar File Extraction Tool");
        _logger.LogInfo("");
        _logger.LogInfo("Usage: FetchGrxml.exe <business_units.csv> <fileserver-vip> [exclusions.txt] [output-directory]");
        _logger.LogInfo("");
        _logger.LogInfo("Arguments:");
        _logger.LogInfo("  business_units.csv  - Path to CSV file containing BU numbers");
        _logger.LogInfo("  fileserver-vip      - FileServer VIP address (e.g., fileserver.example.com)");
        _logger.LogInfo("  exclusions.txt      - (Optional) File with directory paths to exclude, one per line");
        _logger.LogInfo("  output-directory    - (Optional) Directory where files will be saved (default: .\\output)");
    }

    public static void ShowError(string message)
    {
        _logger.LogException(message, new Exception("Application error"));
    }

    public static void ShowInfo(string message)
    {
        _logger.LogInfo(message);
    }

    public static void ShowBlankLine()
    {
        _logger.LogInfo("");
    }

    public static void ShowBusinessUnitsLoaded(int count)
    {
        _logger.LogInfo($"Found {count} business units to process");
        _logger.LogInfo("");
    }

    public static void ShowExclusionPatternsLoaded(int count)
    {
        _logger.LogInfo($"Loaded {count} exclusion patterns");
        _logger.LogInfo("");
    }

    public static void ShowProcessingBU(int busNo)
    {
        _logger.LogInfo($"Processing BU{busNo}...");
    }

    public static void ShowFilesFound(int busNo, int count)
    {
        _logger.LogInfo($"  BU{busNo}: Found {count} .grxml files");
    }

    public static void ShowDownloadProgress(int busNo, int downloaded, int total)
    {
        _logger.LogInfo($"  BU{busNo}: Downloaded {downloaded}/{total} files");
    }

    public static void ShowCompletion(int totalFiles, int totalBUs)
    {
        _logger.LogInfo($"COMPLETE: Downloaded {totalFiles} .grxml files across {totalBUs} business units");
    }

    public static void ShowDownloadError(int busNo, string fileName, string error)
    {
        _logger.LogInfo($"    BU{busNo}: ✗ {fileName} - {error}");
    }

    public static void ShowDirectorySkipped(int busNo, string path)
    {
        _logger.LogInfo($"    BU{busNo}: Skipping excluded directory: {path}");
    }

    public static void ShowDirectoryError(int busNo, string path, string error)
    {
        _logger.LogInfo($"    BU{busNo}: Error listing directory {path}: {error}");
    }

    public static void ShowBUError(int busNo, string error)
    {
        _logger.LogInfo($"  BU{busNo}: ERROR: {error}");
    }
}

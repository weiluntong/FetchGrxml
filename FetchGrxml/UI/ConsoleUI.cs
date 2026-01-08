namespace FetchGrxml;

/// <summary>
/// Handles all console output for the application
/// </summary>
public static class ConsoleUI
{
    public static void ShowBanner(string fileServerVip, string outputDirectory, string busListPath)
    {
        Console.WriteLine($"FetchGrxml - Grammar File Extraction Tool");
        Console.WriteLine($"FileServer: {fileServerVip}");
        Console.WriteLine($"Output Directory: {outputDirectory}");
        Console.WriteLine($"BU List: {busListPath}");
        Console.WriteLine();
    }

    public static void ShowUsage()
    {
        Console.WriteLine("FetchGrxml - Grammar File Extraction Tool");
        Console.WriteLine();
        Console.WriteLine("Usage: FetchGrxml.exe <business_units.csv> <fileserver-vip> [exclusions.txt] [output-directory]");
        Console.WriteLine();
        Console.WriteLine("Arguments:");
        Console.WriteLine("  business_units.csv  - Path to CSV file containing BU numbers");
        Console.WriteLine("  fileserver-vip      - FileServer VIP address (e.g., fileserver.example.com)");
        Console.WriteLine("  exclusions.txt      - (Optional) File with directory paths to exclude, one per line");
        Console.WriteLine("  output-directory    - (Optional) Directory where files will be saved (default: .\\output)");
    }

    public static void ShowError(string message)
    {
        Console.WriteLine($"ERROR: {message}");
    }

    public static void ShowInfo(string message)
    {
        Console.WriteLine(message);
    }

    public static void ShowBlankLine()
    {
        Console.WriteLine();
    }

    public static void ShowBusinessUnitsLoaded(int count)
    {
        Console.WriteLine($"Found {count} business units to process");
        Console.WriteLine();
    }

    public static void ShowExclusionPatternsLoaded(int count)
    {
        Console.WriteLine($"Loaded {count} exclusion patterns");
        Console.WriteLine();
    }

    public static void ShowProcessingBU(int busNo)
    {
        Console.WriteLine($"Processing BU{busNo}...");
    }

    public static void ShowFilesFound(int count)
    {
        Console.WriteLine($"  Found {count} .grxml files");
    }

    public static void ShowDownloadProgress(int downloaded, int total)
    {
        Console.WriteLine($"  Downloaded {downloaded}/{total} files");
    }

    public static void ShowCompletion(int totalFiles, int totalBUs)
    {
        Console.WriteLine($"COMPLETE: Downloaded {totalFiles} .grxml files across {totalBUs} business units");
    }

    public static void ShowDownloadError(string fileName, string error)
    {
        Console.WriteLine($"    ✗ {fileName} - {error}");
    }

    public static void ShowDirectorySkipped(string path)
    {
        Console.WriteLine($"    Skipping excluded directory: {path}");
    }

    public static void ShowDirectoryError(string path, string error)
    {
        Console.WriteLine($"    Error listing directory {path}: {error}");
    }

    public static void ShowBUError(string error)
    {
        Console.WriteLine($"  ERROR: {error}");
    }
}

using FileServerCoreSDK.Interfaces;

namespace FetchGrxml;

/// <summary>
/// Simple console logger implementation for FileServerCoreSDK
/// </summary>
public class ConsoleLogger : IFileServerLogHelper
{
    public void LogException(string message, Exception exception)
    {
        Console.WriteLine($"[ERROR] {message}: {exception.Message}");
    }

    public void LogDebug(int level, string message)
    {
        // Suppress debug messages for cleaner output
    }

    public void LogInfo(string message)
    {
        Console.WriteLine($"[INFO] {message}");
    }
}

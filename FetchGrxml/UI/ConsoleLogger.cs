using FileServerCoreSDK.Interfaces;

namespace FetchGrxml;

/// <summary>
/// Simple console logger implementation for FileServerCoreSDK
/// </summary>
public class ConsoleLogger : IFileServerLogHelper
{
    private static readonly AsyncLocal<string?> _currentCluster = new();

    public static void SetClusterContext(string clusterName)
    {
        _currentCluster.Value = clusterName;
    }

    private string GetPrefix()
    {
        var cluster = _currentCluster.Value;
        return string.IsNullOrEmpty(cluster) ? "" : $"[{cluster}] ";
    }

    public void LogException(string message, Exception exception)
    {
        Console.WriteLine($"{GetPrefix()}[ERROR] {message}: {exception.Message}");
    }

    public void LogDebug(int level, string message)
    {
        // Suppress debug messages for cleaner output
    }

    public void LogInfo(string message)
    {
        Console.WriteLine($"{GetPrefix()}[INFO] {message}");
    }
}

namespace FetchGrxml;

/// <summary>
/// Configuration for the application
/// </summary>
public class AppConfig
{
    public string BusNumbersCsvPath { get; set; } = "";
    public string ClusterConfigPath { get; set; } = "";
    public string? ExclusionsPath { get; set; }
    public string OutputDirectory { get; set; } = "";

    public static AppConfig FromArgs(string[] args)
    {
        return new AppConfig
        {
            BusNumbersCsvPath = args[0],
            ClusterConfigPath = args[1],
            ExclusionsPath = args.Length > 2 ? args[2] : null,
            OutputDirectory = args.Length > 3 ? args[3] : Path.Combine(".", "output")
        };
    }
}

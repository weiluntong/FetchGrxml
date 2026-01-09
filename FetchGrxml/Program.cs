using FileServerCoreSDK;
using FileServerCoreSDK.Configuration;

namespace FetchGrxml;

class Program
{
    static async Task Main(string[] args)
    {
        if (args.Length < 2)
        {
            ConsoleUI.ShowUsage();
            return;
        }

        var config = AppConfig.FromArgs(args);
        ConsoleUI.ShowBanner(config.ClusterConfigPath, config.OutputDirectory, config.BusNumbersCsvPath);

        if (!File.Exists(config.BusNumbersCsvPath))
        {
            ConsoleUI.ShowError($"File not found: {config.BusNumbersCsvPath}");
            return;
        }

        if (!Directory.Exists(config.ClusterConfigPath))
        {
            ConsoleUI.ShowError($"Cluster config directory not found: {config.ClusterConfigPath}");
            return;
        }

        var clusterToBUs = ConfigHelper.ReadBusinessUnits(config.BusNumbersCsvPath);
        if (clusterToBUs.Count == 0)
        {
            ConsoleUI.ShowError("No valid business unit numbers found in CSV file.");
            ConsoleUI.ShowInfo("Expected format: BU,Cluster pairs per line (cluster can be quoted comma-separated list)");
            return;
        }
        int totalBUs = clusterToBUs.Values.SelectMany(x => x).Distinct().Count();
        ConsoleUI.ShowBusinessUnitsLoaded(totalBUs);

        var exclusionPatterns = ConfigHelper.ReadExclusionPatterns(config.ExclusionsPath);
        if (exclusionPatterns.Count > 0)
            ConsoleUI.ShowExclusionPatternsLoaded(exclusionPatterns.Count);

        DependencyInjection.AddFileServerCoreSDK(options =>
        {
            options.SetLogger(new ConsoleLogger());
        });

        var clusterTasks = clusterToBUs.Select(async kvp =>
        {
            string clusterName = kvp.Key;
            var busInCluster = kvp.Value;
            ConsoleLogger.SetClusterContext(clusterName);
            string endpoint = ConfigHelper.ReadClusterEndpoint(config.ClusterConfigPath, clusterName);
            
            using (var fileCommWcf = new FileCommWCF(endpoint, "FetchGrxml"))
            {
                var fileClient = new FileServerClientAdapter(fileCommWcf);
                string clusterOutputDir = Path.Combine(config.OutputDirectory, clusterName);
                var orchestrator = new WorkflowOrchestrator(fileClient, clusterOutputDir, exclusionPatterns);
                return await orchestrator.ProcessBusinessUnits(busInCluster);
            }
        });

        var results = await Task.WhenAll(clusterTasks);
        int totalFiles = results.Sum();
        
        ConsoleUI.ShowCompletion(totalFiles, totalBUs);
    }
}

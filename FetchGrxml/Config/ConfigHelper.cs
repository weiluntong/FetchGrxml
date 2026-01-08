namespace FetchGrxml;

public static class ConfigHelper
{
    public static Dictionary<string, HashSet<int>> ReadBusinessUnits(string csvPath)
    {
        var clusterToBUs = new Dictionary<string, HashSet<int>>();
        
        foreach (string line in File.ReadAllLines(csvPath))
        {
            string trimmedLine = line.Trim();
            
            // Skip empty lines and comments
            if (string.IsNullOrWhiteSpace(trimmedLine) || trimmedLine.StartsWith("#"))
            {
                continue;
            }
            
            // Split by comma, but respect quotes
            var parts = SplitCsvLine(trimmedLine);
            
            if (parts.Length >= 2)
            {
                if (int.TryParse(parts[0].Trim(), out int busNo))
                {
                    string clusterValue = parts[1].Trim().Trim('"');
                    
                    // Split multiple clusters by comma
                    var clusters = clusterValue.Split(',')
                        .Select(c => c.Trim())
                        .Where(c => !string.IsNullOrWhiteSpace(c))
                        .ToList();
                    
                    foreach (var cluster in clusters)
                    {
                        if (!clusterToBUs.ContainsKey(cluster))
                        {
                            clusterToBUs[cluster] = new HashSet<int>();
                        }
                        clusterToBUs[cluster].Add(busNo);
                    }
                }
            }
        }
        
        return clusterToBUs;
    }

    private static string[] SplitCsvLine(string line)
    {
        var result = new List<string>();
        var currentField = new System.Text.StringBuilder();
        bool inQuotes = false;
        
        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            
            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == ',' && !inQuotes)
            {
                result.Add(currentField.ToString());
                currentField.Clear();
            }
            else
            {
                currentField.Append(c);
            }
        }
        
        result.Add(currentField.ToString());
        return result.ToArray();
    }

    public static string ReadClusterEndpoint(string clusterConfigPath, string clusterName)
    {
        string propertiesFile = Path.Combine(clusterConfigPath, $"{clusterName}.cluster.properties");
        
        if (!File.Exists(propertiesFile))
        {
            throw new FileNotFoundException($"Cluster properties file not found: {propertiesFile}");
        }
        
        foreach (string line in File.ReadAllLines(propertiesFile))
        {
            string trimmedLine = line.Trim();
            
            if (trimmedLine.StartsWith("file_server_configuration.FsIpAddress"))
            {
                int equalsIndex = trimmedLine.IndexOf('=');
                if (equalsIndex > 0)
                {
                    return trimmedLine.Substring(equalsIndex + 1).Trim();
                }
            }
        }
        
        throw new InvalidDataException($"file_server_configuration.FsIpAddress not found in {propertiesFile}");
    }

    public static List<string> ReadExclusionPatterns(string? exclusionsPath)
    {
        var exclusionPatterns = new List<string>();
        
        if (!string.IsNullOrEmpty(exclusionsPath) && File.Exists(exclusionsPath))
        {
            exclusionPatterns = File.ReadAllLines(exclusionsPath)
                .Select(line => line.Trim())
                .Where(line => !string.IsNullOrWhiteSpace(line) && !line.StartsWith("#"))
                .ToList();
        }
        
        return exclusionPatterns;
    }
}

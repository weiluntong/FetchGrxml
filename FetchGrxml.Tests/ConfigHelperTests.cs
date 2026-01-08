using Xunit;

namespace FetchGrxml.Tests;

public class ConfigHelperTests : IDisposable
{
    private readonly string _testDir;

    public ConfigHelperTests()
    {
        _testDir = Path.Combine(Path.GetTempPath(), $"FetchGrxmlTests_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDir);
    }

    [Fact]
    public void ReadBusinessUnits_CommaSeparated_ParsesCorrectly()
    {
        var csvPath = CreateTestFile("bu.csv", "12345,cluster3\n67890,cluster1\n11111,cluster2");
        
        var result = ConfigHelper.ReadBusinessUnits(csvPath);
        
        Assert.Equal(3, result.Count);
        Assert.Contains("cluster3", result.Keys);
        Assert.Contains("cluster1", result.Keys);
        Assert.Contains("cluster2", result.Keys);
        Assert.Single(result["cluster3"]);
        Assert.Contains(12345, result["cluster3"]);
        Assert.Single(result["cluster1"]);
        Assert.Contains(67890, result["cluster1"]);
        Assert.Single(result["cluster2"]);
        Assert.Contains(11111, result["cluster2"]);
    }

    [Fact]
    public void ReadBusinessUnits_WithClusterColumn_ParsesBothColumns()
    {
        var csvPath = CreateTestFile("bu.csv", "12345,prod-west\n67890,prod-east\n11111,dev");
        
        var result = ConfigHelper.ReadBusinessUnits(csvPath);
        
        Assert.Equal(3, result.Count);
        Assert.Contains("prod-west", result.Keys);
        Assert.Single(result["prod-west"]);
        Assert.Contains(12345, result["prod-west"]);
        Assert.Contains("prod-east", result.Keys);
        Assert.Single(result["prod-east"]);
        Assert.Contains(67890, result["prod-east"]);
        Assert.Contains("dev", result.Keys);
        Assert.Single(result["dev"]);
        Assert.Contains(11111, result["dev"]);
    }

    [Fact]
    public void ReadBusinessUnits_WithMultipleClusters_ParsesQuotedList()
    {
        var csvPath = CreateTestFile("bu.csv", "12345,\"cluster1,cluster2\"\n67890,cluster3");
        
        var result = ConfigHelper.ReadBusinessUnits(csvPath);
        
        Assert.Equal(3, result.Count);
        Assert.Contains("cluster1", result.Keys);
        Assert.Single(result["cluster1"]);
        Assert.Contains(12345, result["cluster1"]);
        Assert.Contains("cluster2", result.Keys);
        Assert.Single(result["cluster2"]);
        Assert.Contains(12345, result["cluster2"]);
        Assert.Contains("cluster3", result.Keys);
        Assert.Single(result["cluster3"]);
        Assert.Contains(67890, result["cluster3"]);
    }

    [Fact]
    public void ReadBusinessUnits_WithWhitespace_TrimsCorrectly()
    {
        var csvPath = CreateTestFile("bu.csv", " 12345 , prod-west \n 67890 , \" cluster1 , cluster2 \" ");
        
        var result = ConfigHelper.ReadBusinessUnits(csvPath);
        
        Assert.Equal(3, result.Count);
        Assert.Contains("prod-west", result.Keys);
        Assert.Contains(12345, result["prod-west"]);
        Assert.Contains("cluster1", result.Keys);
        Assert.Contains(67890, result["cluster1"]);
        Assert.Contains("cluster2", result.Keys);
        Assert.Contains(67890, result["cluster2"]);
    }

    [Fact]
    public void ReadBusinessUnits_WithInvalidNumbers_SkipsThem()
    {
        var csvPath = CreateTestFile("bu.csv", "12345,prod\ninvalid,test\n67890,dev");
        
        var result = ConfigHelper.ReadBusinessUnits(csvPath);
        
        Assert.Equal(2, result.Count);
        Assert.Contains("prod", result.Keys);
        Assert.Contains(12345, result["prod"]);
        Assert.Contains("dev", result.Keys);
        Assert.Contains(67890, result["dev"]);
    }

    [Fact]
    public void ReadBusinessUnits_SingleClusterWithQuotes_ParsesCorrectly()
    {
        var csvPath = CreateTestFile("bu.csv", "12345,\"cluster1\"");
        
        var result = ConfigHelper.ReadBusinessUnits(csvPath);
        
        Assert.Single(result);
        Assert.Contains("cluster1", result.Keys);
        Assert.Single(result["cluster1"]);
        Assert.Contains(12345, result["cluster1"]);
    }

    [Fact]
    public void ReadBusinessUnits_WithDuplicateBUs_IgnoresDuplicates()
    {
        var csvPath = CreateTestFile("bu.csv", "12345,cluster1\n12345,cluster1\n67890,cluster1");
        
        var result = ConfigHelper.ReadBusinessUnits(csvPath);
        
        Assert.Single(result);
        Assert.Contains("cluster1", result.Keys);
        Assert.Equal(2, result["cluster1"].Count);
        Assert.Contains(12345, result["cluster1"]);
        Assert.Contains(67890, result["cluster1"]);
    }

    [Fact]
    public void ReadBusinessUnits_EmptyFile_ReturnsEmptyList()
    {
        var csvPath = CreateTestFile("bu.csv", "");
        
        var result = ConfigHelper.ReadBusinessUnits(csvPath);
        
        Assert.Empty(result);
    }

    [Fact]
    public void ReadExclusionPatterns_WithComments_IgnoresComments()
    {
        var content = @"# This is a comment
/temp
*/logs/*
# Another comment
/backup";
        var exPath = CreateTestFile("ex.txt", content);
        
        var result = ConfigHelper.ReadExclusionPatterns(exPath);
        
        Assert.Equal(3, result.Count);
        Assert.Contains("/temp", result);
        Assert.Contains("*/logs/*", result);
        Assert.Contains("/backup", result);
    }

    [Fact]
    public void ReadExclusionPatterns_WithEmptyLines_SkipsThem()
    {
        var content = "/temp\n\n*/logs/*\n   \n/backup";
        var exPath = CreateTestFile("ex.txt", content);
        
        var result = ConfigHelper.ReadExclusionPatterns(exPath);
        
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public void ReadExclusionPatterns_NonExistentFile_ReturnsEmptyList()
    {
        var result = ConfigHelper.ReadExclusionPatterns("nonexistent.txt");
        
        Assert.Empty(result);
    }

    [Fact]
    public void ReadClusterEndpoint_ValidPropertiesFile_ReturnsEndpoint()
    {
        var clusterDir = Path.Combine(_testDir, "clusters");
        Directory.CreateDirectory(clusterDir);
        var propsPath = Path.Combine(clusterDir, "prod-west.cluster.properties");
        File.WriteAllText(propsPath, "file_server_configuration.FsIpAddress = fs-prod.example.com\nother_setting = value");
        
        var result = ConfigHelper.ReadClusterEndpoint(clusterDir, "prod-west");
        
        Assert.Equal("fs-prod.example.com", result);
    }

    [Fact]
    public void ReadClusterEndpoint_WithWhitespace_TrimsValue()
    {
        var clusterDir = Path.Combine(_testDir, "clusters");
        Directory.CreateDirectory(clusterDir);
        var propsPath = Path.Combine(clusterDir, "dev.cluster.properties");
        File.WriteAllText(propsPath, "file_server_configuration.FsIpAddress =   fs-dev.example.com  ");
        
        var result = ConfigHelper.ReadClusterEndpoint(clusterDir, "dev");
        
        Assert.Equal("fs-dev.example.com", result);
    }

    [Fact]
    public void ReadClusterEndpoint_NonExistentFile_ThrowsException()
    {
        var clusterDir = Path.Combine(_testDir, "clusters");
        Directory.CreateDirectory(clusterDir);
        
        var exception = Assert.Throws<FileNotFoundException>(() => 
            ConfigHelper.ReadClusterEndpoint(clusterDir, "nonexistent"));
        
        Assert.Contains("nonexistent.cluster.properties", exception.Message);
    }

    [Fact]
    public void ReadClusterEndpoint_MissingFsIpAddress_ThrowsException()
    {
        var clusterDir = Path.Combine(_testDir, "clusters");
        Directory.CreateDirectory(clusterDir);
        var propsPath = Path.Combine(clusterDir, "bad.cluster.properties");
        File.WriteAllText(propsPath, "some_other_setting = value");
        
        var exception = Assert.Throws<InvalidDataException>(() => 
            ConfigHelper.ReadClusterEndpoint(clusterDir, "bad"));
        
        Assert.Contains("file_server_configuration.FsIpAddress", exception.Message);
    }

    private string CreateTestFile(string fileName, string content)
    {
        var path = Path.Combine(_testDir, fileName);
        File.WriteAllText(path, content);
        return path;
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDir))
        {
            Directory.Delete(_testDir, true);
        }
    }
}

using Xunit;

namespace FetchGrxml.Tests;

public class AppConfigTests
{
    [Fact]
    public void FromArgs_WithTwoArgs_SetsRequiredFields()
    {
        var args = new[] { "bu.csv", "clusters" };
        
        var config = AppConfig.FromArgs(args);
        
        Assert.Equal("bu.csv", config.BusNumbersCsvPath);
        Assert.Equal("clusters", config.ClusterConfigPath);
        Assert.Null(config.ExclusionsPath);
        Assert.Equal(Path.Combine(".", "output"), config.OutputDirectory);
    }

    [Fact]
    public void FromArgs_WithThreeArgs_SetsExclusionsPath()
    {
        var args = new[] { "bu.csv", "clusters", "ex.txt" };
        
        var config = AppConfig.FromArgs(args);
        
        Assert.Equal("ex.txt", config.ExclusionsPath);
        Assert.Equal(Path.Combine(".", "output"), config.OutputDirectory);
    }

    [Fact]
    public void FromArgs_WithFourArgs_SetsOutputDirectory()
    {
        var args = new[] { "bu.csv", "clusters", "ex.txt", "myoutput" };
        
        var config = AppConfig.FromArgs(args);
        
        Assert.Equal("ex.txt", config.ExclusionsPath);
        Assert.Equal("myoutput", config.OutputDirectory);
    }

    [Fact]
    public void FromArgs_WithMinimalArgs_UsesDefaults()
    {
        var args = new[] { "test.csv", "clusters" };
        
        var config = AppConfig.FromArgs(args);
        
        Assert.Null(config.ExclusionsPath);
        Assert.Equal(Path.Combine(".", "output"), config.OutputDirectory);
    }
}

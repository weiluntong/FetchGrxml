using Moq;
using Xunit;
using FileServerCoreSDK.Definitions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FetchGrxml.Tests;

public class FileScannerTests
{
    [Fact]
    public async Task ScanForGrxmlFiles_WithGrxmlFiles_ReturnsOnlyGrxmlFiles()
    {
        var mockClient = new Mock<IFileServerClient>();
        var throttler = new RequestThrottler();
        
        // Mock root directory listing
        mockClient.Setup(c => c.GetDirectoryListing(123, "/", "*", false, false, false))
            .Returns(new ListResults
            {
                Files = new List<FSFileInfo>
                {
                    new FSFileInfo { FileName = "test.grxml", IsFolder = false },
                    new FSFileInfo { FileName = "data.xml", IsFolder = false },
                    new FSFileInfo { FileName = "file.txt", IsFolder = false }
                }
            });

        var scanner = new FileScanner(mockClient.Object, throttler, new List<string>());
        
        var results = await scanner.ScanForGrxmlFiles(123, "/");
        
        Assert.Single(results);
        Assert.Contains("/test.grxml", results);
    }

    [Fact]
    public async Task ScanForGrxmlFiles_WithNestedDirectories_RecursesCorrectly()
    {
        var mockClient = new Mock<IFileServerClient>();
        var throttler = new RequestThrottler();
        
        // Mock root directory
        mockClient.Setup(c => c.GetDirectoryListing(123, "/", "*", false, false, false))
            .Returns(new ListResults
            {
                Files = new List<FSFileInfo>
                {
                    new FSFileInfo { FileName = "root.grxml", IsFolder = false },
                    new FSFileInfo { FileName = "subfolder", IsFolder = true }
                }
            });

        // Mock subfolder directory
        mockClient.Setup(c => c.GetDirectoryListing(123, "/subfolder", "*", false, false, false))
            .Returns(new ListResults
            {
                Files = new List<FSFileInfo>
                {
                    new FSFileInfo { FileName = "nested.grxml", IsFolder = false }
                }
            });

        var scanner = new FileScanner(mockClient.Object, throttler, new List<string>());
        
        var results = await scanner.ScanForGrxmlFiles(123, "/");
        
        Assert.Equal(2, results.Count);
        Assert.Contains("/root.grxml", results);
        Assert.Contains("/subfolder/nested.grxml", results);
    }

    [Fact]
    public async Task ScanForGrxmlFiles_WithExclusions_SkipsExcludedDirectories()
    {
        var mockClient = new Mock<IFileServerClient>();
        var throttler = new RequestThrottler();
        
        // Mock root directory
        mockClient.Setup(c => c.GetDirectoryListing(123, "/", "*", false, false, false))
            .Returns(new ListResults
            {
                Files = new List<FSFileInfo>
                {
                    new FSFileInfo { FileName = "temp", IsFolder = true },
                    new FSFileInfo { FileName = "data", IsFolder = true }
                }
            });

        // Mock data folder (should be called)
        mockClient.Setup(c => c.GetDirectoryListing(123, "/data", "*", false, false, false))
            .Returns(new ListResults
            {
                Files = new List<FSFileInfo>
                {
                    new FSFileInfo { FileName = "file.grxml", IsFolder = false }
                }
            });

        var exclusions = new List<string> { "/temp" };
        var scanner = new FileScanner(mockClient.Object, throttler, exclusions);
        
        var results = await scanner.ScanForGrxmlFiles(123, "/");
        
        Assert.Single(results);
        Assert.Contains("/data/file.grxml", results);
        
        // Verify /temp was never queried
        mockClient.Verify(c => c.GetDirectoryListing(123, "/temp", It.IsAny<string>(), 
            It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public async Task ScanForGrxmlFiles_WithNullResults_HandlesGracefully()
    {
        var mockClient = new Mock<IFileServerClient>();
        var throttler = new RequestThrottler();
        
        mockClient.Setup(c => c.GetDirectoryListing(It.IsAny<int>(), It.IsAny<string>(), 
            It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>()))
            .Returns((ListResults?)null);

        var scanner = new FileScanner(mockClient.Object, throttler, new List<string>());
        
        var results = await scanner.ScanForGrxmlFiles(123, "/");
        
        Assert.Empty(results);
    }
}

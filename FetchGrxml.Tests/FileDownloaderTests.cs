using Moq;
using Xunit;
using FileServerCoreSDK.Definitions;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace FetchGrxml.Tests;

public class FileDownloaderTests : IDisposable
{
    private readonly string _testOutputDir;

    public FileDownloaderTests()
    {
        _testOutputDir = Path.Combine(Path.GetTempPath(), $"DownloadTests_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testOutputDir);
    }

    [Fact]
    public async Task DownloadFiles_WithSuccessfulDownloads_ReturnsCorrectCount()
    {
        var mockClient = new Mock<IFileServerClient>();
        var throttler = new RequestThrottler();
        
        mockClient.Setup(c => c.GetFileFromServer(123, It.IsAny<string>(), It.IsAny<string>()))
            .Returns(new FSResult { ActionResult = FileServerCodes.Success });

        var downloader = new FileDownloader(mockClient.Object, throttler, _testOutputDir);
        var files = new List<string> { "/file1.grxml", "/file2.grxml", "/file3.grxml" };
        
        var count = await downloader.DownloadFiles(123, files);
        
        Assert.Equal(3, count);
        mockClient.Verify(c => c.GetFileFromServer(123, It.IsAny<string>(), It.IsAny<string>()), 
            Times.Exactly(3));
    }

    [Fact]
    public async Task DownloadFiles_WithSomeFailures_ReturnsSuccessCount()
    {
        var mockClient = new Mock<IFileServerClient>();
        var throttler = new RequestThrottler();
        
        mockClient.Setup(c => c.GetFileFromServer(123, "/file1.grxml", It.IsAny<string>()))
            .Returns(new FSResult { ActionResult = FileServerCodes.Success });
        
        mockClient.Setup(c => c.GetFileFromServer(123, "/file2.grxml", It.IsAny<string>()))
            .Returns(new FSResult { ActionResult = FileServerCodes.FileNotFound });
        
        mockClient.Setup(c => c.GetFileFromServer(123, "/file3.grxml", It.IsAny<string>()))
            .Returns(new FSResult { ActionResult = FileServerCodes.Success });

        var downloader = new FileDownloader(mockClient.Object, throttler, _testOutputDir);
        var files = new List<string> { "/file1.grxml", "/file2.grxml", "/file3.grxml" };
        
        var count = await downloader.DownloadFiles(123, files);
        
        Assert.Equal(2, count);
    }

    [Fact]
    public async Task DownloadFiles_CreatesNestedDirectoryStructure()
    {
        var mockClient = new Mock<IFileServerClient>();
        var throttler = new RequestThrottler();
        
        mockClient.Setup(c => c.GetFileFromServer(123, It.IsAny<string>(), It.IsAny<string>()))
            .Returns(new FSResult { ActionResult = FileServerCodes.Success });

        var downloader = new FileDownloader(mockClient.Object, throttler, _testOutputDir);
        var files = new List<string> { "/folder1/folder2/deep.grxml" };
        
        await downloader.DownloadFiles(123, files);
        
        var expectedDir = Path.Combine(_testOutputDir, "BUS123", "folder1", "folder2");
        Assert.True(Directory.Exists(expectedDir));
    }

    [Fact]
    public async Task DownloadFiles_WithEmptyList_ReturnsZero()
    {
        var mockClient = new Mock<IFileServerClient>();
        var throttler = new RequestThrottler();

        var downloader = new FileDownloader(mockClient.Object, throttler, _testOutputDir);
        
        var count = await downloader.DownloadFiles(123, new List<string>());
        
        Assert.Equal(0, count);
        mockClient.Verify(c => c.GetFileFromServer(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), 
            Times.Never);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testOutputDir))
        {
            Directory.Delete(_testOutputDir, true);
        }
    }
}

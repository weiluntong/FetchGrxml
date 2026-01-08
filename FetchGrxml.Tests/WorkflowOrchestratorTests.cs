using Moq;
using Xunit;
using FileServerCoreSDK.Definitions;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace FetchGrxml.Tests;

public class WorkflowOrchestratorTests : IDisposable
{
    private readonly string _testOutputDir;

    public WorkflowOrchestratorTests()
    {
        _testOutputDir = Path.Combine(Path.GetTempPath(), $"WorkflowTest_{System.Guid.NewGuid()}");
        Directory.CreateDirectory(_testOutputDir);
    }

    [Fact]
    public async Task ProcessBusinessUnits_ScansAndDownloadsForMultipleBUs()
    {
        var mockClient = new Mock<IFileServerClient>();

        // Mock BU 123
        mockClient.Setup(c => c.GetDirectoryListing(123, "/", "*", false, false, false))
            .Returns(new ListResults
            {
                Files = new List<FSFileInfo>
                {
                    new FSFileInfo { FileName = "file1.grxml", IsFolder = false }
                }
            });

        mockClient.Setup(c => c.GetFileFromServer(123, "/file1.grxml", It.IsAny<string>()))
            .Returns(new FSResult { ActionResult = FileServerCodes.Success });

        // Mock BU 456
        mockClient.Setup(c => c.GetDirectoryListing(456, "/", "*", false, false, false))
            .Returns(new ListResults
            {
                Files = new List<FSFileInfo>
                {
                    new FSFileInfo { FileName = "file2.grxml", IsFolder = false }
                }
            });

        mockClient.Setup(c => c.GetFileFromServer(456, "/file2.grxml", It.IsAny<string>()))
            .Returns(new FSResult { ActionResult = FileServerCodes.Success });

        var orchestrator = new WorkflowOrchestrator(mockClient.Object, _testOutputDir, new List<string>());
        var totalFiles = await orchestrator.ProcessBusinessUnits(new List<int> { 123, 456 });

        // Verify scanning was called for both BUs
        mockClient.Verify(c => c.GetDirectoryListing(123, "/", "*", false, false, false), Times.Once);
        mockClient.Verify(c => c.GetDirectoryListing(456, "/", "*", false, false, false), Times.Once);

        // Verify downloads were called
        mockClient.Verify(c => c.GetFileFromServer(123, "/file1.grxml", It.IsAny<string>()), Times.Once);
        mockClient.Verify(c => c.GetFileFromServer(456, "/file2.grxml", It.IsAny<string>()), Times.Once);

        // Verify output directories were created
        Assert.True(Directory.Exists(Path.Combine(_testOutputDir, "BUS123")));
        Assert.True(Directory.Exists(Path.Combine(_testOutputDir, "BUS456")));
        
        Assert.Equal(2, totalFiles);
    }

    [Fact]
    public async Task ProcessBusinessUnits_WithExclusions_PassesExclusionsToScanner()
    {
        var mockClient = new Mock<IFileServerClient>();

        // Mock root with excluded folder
        mockClient.Setup(c => c.GetDirectoryListing(123, "/", "*", false, false, false))
            .Returns(new ListResults
            {
                Files = new List<FSFileInfo>
                {
                    new FSFileInfo { FileName = "temp", IsFolder = true },
                    new FSFileInfo { FileName = "data", IsFolder = true }
                }
            });

        mockClient.Setup(c => c.GetDirectoryListing(123, "/data", "*", false, false, false))
            .Returns(new ListResults
            {
                Files = new List<FSFileInfo>
                {
                    new FSFileInfo { FileName = "file.grxml", IsFolder = false }
                }
            });

        mockClient.Setup(c => c.GetFileFromServer(123, "/data/file.grxml", It.IsAny<string>()))
            .Returns(new FSResult { ActionResult = FileServerCodes.Success });

        var orchestrator = new WorkflowOrchestrator(mockClient.Object, _testOutputDir, new List<string> { "/temp" });
        await orchestrator.ProcessBusinessUnits(new List<int> { 123 });

        // Verify /temp was never queried
        mockClient.Verify(c => c.GetDirectoryListing(123, "/temp", It.IsAny<string>(), 
            It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Never);

        // Verify /data was queried
        mockClient.Verify(c => c.GetDirectoryListing(123, "/data", "*", false, false, false), Times.Once);
    }

    [Fact]
    public async Task ProcessBusinessUnits_WithNoFiles_CompletesSuccessfully()
    {
        var mockClient = new Mock<IFileServerClient>();

        mockClient.Setup(c => c.GetDirectoryListing(123, "/", "*", false, false, false))
            .Returns(new ListResults
            {
                Files = new List<FSFileInfo>()
            });

        var orchestrator = new WorkflowOrchestrator(mockClient.Object, _testOutputDir, new List<string>());
        var totalFiles = await orchestrator.ProcessBusinessUnits(new List<int> { 123 });

        // Should complete without errors
        mockClient.Verify(c => c.GetDirectoryListing(123, "/", "*", false, false, false), Times.Once);
        Assert.Equal(0, totalFiles);
    }

    [Fact]
    public async Task ProcessBusinessUnits_ProcessesBUsSequentially()
    {
        var mockClient = new Mock<IFileServerClient>();
        var processingOrder = new List<int>();

        mockClient.Setup(c => c.GetDirectoryListing(It.IsAny<int>(), "/", "*", false, false, false))
            .Returns<int, string, string, bool, bool, bool>((busNo, path, pattern, deleted, folders, files) =>
            {
                processingOrder.Add(busNo);
                return new ListResults { Files = new List<FSFileInfo>() };
            });

        var orchestrator = new WorkflowOrchestrator(mockClient.Object, _testOutputDir, new List<string>());
        await orchestrator.ProcessBusinessUnits(new List<int> { 111, 222, 333 });

        Assert.Equal(new[] { 111, 222, 333 }, processingOrder);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testOutputDir))
        {
            Directory.Delete(_testOutputDir, true);
        }
    }
}

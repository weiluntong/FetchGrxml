using FileServerCoreSDK;
using FileServerCoreSDK.Definitions;

// This file is just to test what types are available
public class TypeTest
{
    public void Test()
    {
        var lr = new ListResults();
        var files = lr.Files; // What type is this?
        
        // Try to create FileListItem
        var item = new FileListItem();
    }
}

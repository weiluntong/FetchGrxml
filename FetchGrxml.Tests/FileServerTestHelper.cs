using FileServerCoreSDK;
using FileServerCoreSDK.Definitions;

namespace FetchGrxml.Tests;

/// <summary>
/// Helper to create test data for FileServer types
/// </summary>
public static class FileServerTestHelper
{
    public static ListResults CreateListResults(params (string fileName, bool isFolder)[] items)
    {
        // Get the type of Files property
        var listResults = new ListResults();
        var filesProperty = typeof(ListResults).GetProperty("Files");
        
        if (filesProperty == null)
            throw new InvalidOperationException("ListResults.Files property not found");
        
        var itemType = filesProperty.PropertyType.GetGenericArguments()[0];
        var filesList = (System.Collections.IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(itemType))!;
        
        foreach (var (fileName, isFolder) in items)
        {
            var item = Activator.CreateInstance(itemType)!;
            itemType.GetProperty("FileName")!.SetValue(item, fileName);
            itemType.GetProperty("IsFolder")!.SetValue(item, isFolder);
            filesList.Add(item);
        }
        
        filesProperty.SetValue(listResults, filesList);
        return listResults;
    }
}

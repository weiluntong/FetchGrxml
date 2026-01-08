using System.Collections.Generic;
using FileServerCoreSDK.Definitions;

namespace FileServerCoreSDK.Definitions;

public class ListResults
{
    public List<FSFileInfo>? Files;
}

public class FSFileInfo
{
    public string FileName { get; set; } = string.Empty;
    public bool IsFolder { get; set; }
}

public class FSResult
{
    public FileServerCodes ActionResult { get; set; }
}

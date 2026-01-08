using System;

namespace FileServerCoreSDK.Interfaces;

public interface IFileServerLogHelper
{
    void LogDebug(int level, string message);
    void LogInfo(string message);
    void LogException(string message, Exception exception);
}

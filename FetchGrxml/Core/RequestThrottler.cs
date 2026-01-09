namespace FetchGrxml;

/// <summary>
/// Limits FileServer API calls to max 2 concurrent requests
/// </summary>
public class RequestThrottler
{
    private readonly SemaphoreSlim _semaphore = new(2, 2);

    public async Task<T> ExecuteAsync<T>(Func<T> action)
    {
        await _semaphore.WaitAsync();
        try
        {
            return await Task.Run(action);
        }
        finally
        {
            _semaphore.Release();
        }
    }
}

namespace FetchGrxml;

/// <summary>
/// Limits FileServer API calls to max 2 requests per second
/// </summary>
public class RequestThrottler
{
    private readonly int _maxRequestsPerSecond = 2;
    private readonly Queue<DateTime> _requestTimes = new();
    private readonly object _lock = new object();

    public async Task<T> ExecuteAsync<T>(Func<T> action)
    {
        TimeSpan delay = TimeSpan.Zero;
        
        lock (_lock)
        {
            DateTime now = DateTime.UtcNow;
            
            // Remove request times older than 1 second
            while (_requestTimes.Count > 0 && (now - _requestTimes.Peek()) > TimeSpan.FromSeconds(1))
            {
                _requestTimes.Dequeue();
            }
            
            // If we've already made 2 requests in the last second, calculate delay
            if (_requestTimes.Count >= _maxRequestsPerSecond)
            {
                DateTime oldestRequest = _requestTimes.Peek();
                delay = (oldestRequest + TimeSpan.FromSeconds(1)) - now;
            }
            
            // Record this request's start time
            _requestTimes.Enqueue(now + delay);
        }
        
        if (delay > TimeSpan.Zero)
        {
            await Task.Delay(delay);
        }
        
        return await Task.Run(action);
    }
}

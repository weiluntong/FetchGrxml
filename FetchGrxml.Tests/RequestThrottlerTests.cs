using Xunit;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace FetchGrxml.Tests;

public class RequestThrottlerTests
{
    [Fact]
    public async Task ExecuteAsync_LimitsToTwoRequestsPerSecond()
    {
        var throttler = new RequestThrottler();
        var requestTimes = new List<DateTime>();
        var lockObj = new object();

        var tasks = new List<Task>();
        for (int i = 0; i < 5; i++)
        {
            tasks.Add(throttler.ExecuteAsync(() =>
            {
                lock (lockObj)
                {
                    requestTimes.Add(DateTime.UtcNow);
                }
                return 42;
            }));
        }

        await Task.WhenAll(tasks);

        Assert.Equal(5, requestTimes.Count);

        // First 2 should start immediately (within 100ms window)
        var firstTwo = requestTimes.OrderBy(t => t).Take(2).ToList();
        var timeBetweenFirstTwo = (firstTwo[1] - firstTwo[0]).TotalMilliseconds;
        Assert.True(timeBetweenFirstTwo < 100, $"First two requests should start immediately, but gap was {timeBetweenFirstTwo}ms");

        // Third request should wait ~1 second after the first
        var third = requestTimes.OrderBy(t => t).ElementAt(2);
        var delayToThird = (third - firstTwo[0]).TotalMilliseconds;
        Assert.True(delayToThird > 900 && delayToThird < 1100, $"Third request should wait ~1s, but waited {delayToThird}ms");
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsCorrectValues()
    {
        var throttler = new RequestThrottler();

        var tasks = new List<Task<int>>();
        for (int i = 0; i < 3; i++)
        {
            int value = i;
            tasks.Add(throttler.ExecuteAsync(() => value * 2));
        }

        var results = await Task.WhenAll(tasks);

        Assert.Equal(3, results.Length);
        Assert.Contains(0, results);
        Assert.Contains(2, results);
        Assert.Contains(4, results);
    }

    [Fact]
    public async Task ExecuteAsync_AllTasksComplete()
    {
        var throttler = new RequestThrottler();
        int completedCount = 0;

        var tasks = new List<Task>();
        for (int i = 0; i < 5; i++)
        {
            tasks.Add(throttler.ExecuteAsync(() =>
            {
                Interlocked.Increment(ref completedCount);
                return Task.CompletedTask;
            }));
        }

        await Task.WhenAll(tasks);

        Assert.Equal(5, completedCount);
    }
}

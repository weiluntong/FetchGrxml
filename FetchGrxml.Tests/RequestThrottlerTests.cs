using Xunit;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace FetchGrxml.Tests;

public class RequestThrottlerTests
{
    [Fact]
    public async Task ExecuteAsync_LimitsToTwoConcurrentOperations()
    {
        var throttler = new RequestThrottler();
        int currentConcurrent = 0;
        int maxConcurrent = 0;
        var lockObj = new object();

        var tasks = new List<Task>();
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(Task.Run(async () =>
            {
                await throttler.ExecuteAsync(() =>
                {
                    lock (lockObj)
                    {
                        currentConcurrent++;
                        if (currentConcurrent > maxConcurrent)
                            maxConcurrent = currentConcurrent;
                    }

                    Thread.Sleep(50); // Simulate work

                    lock (lockObj)
                    {
                        currentConcurrent--;
                    }

                    return Task.CompletedTask;
                });
            }));
        }

        await Task.WhenAll(tasks);

        Assert.Equal(2, maxConcurrent);
    }

    [Fact]
    public async Task ExecuteAsync_WithReturnValue_LimitsToTwoConcurrentOperations()
    {
        var throttler = new RequestThrottler();
        int currentConcurrent = 0;
        int maxConcurrent = 0;
        var lockObj = new object();

        var tasks = new List<Task<int>>();
        for (int i = 0; i < 10; i++)
        {
            int value = i;
            tasks.Add(Task.Run(async () =>
            {
                return await throttler.ExecuteAsync(() =>
                {
                    lock (lockObj)
                    {
                        currentConcurrent++;
                        if (currentConcurrent > maxConcurrent)
                            maxConcurrent = currentConcurrent;
                    }

                    Thread.Sleep(50); // Simulate work

                    lock (lockObj)
                    {
                        currentConcurrent--;
                    }

                    return value * 2;
                });
            }));
        }

        var results = await Task.WhenAll(tasks);

        Assert.Equal(2, maxConcurrent);
        Assert.Equal(10, results.Length);
        Assert.Equal(0, results[0]);
        Assert.Equal(18, results[9]);
    }

    [Fact]
    public async Task ExecuteAsync_AllTasksComplete()
    {
        var throttler = new RequestThrottler();
        int completedCount = 0;

        var tasks = new List<Task>();
        for (int i = 0; i < 20; i++)
        {
            tasks.Add(Task.Run(async () =>
            {
                await throttler.ExecuteAsync(() =>
                {
                    Thread.Sleep(10);
                    Interlocked.Increment(ref completedCount);
                    return Task.CompletedTask;
                });
            }));
        }

        await Task.WhenAll(tasks);

        Assert.Equal(20, completedCount);
    }
}

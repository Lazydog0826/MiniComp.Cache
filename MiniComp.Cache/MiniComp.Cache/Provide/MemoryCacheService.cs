using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;

namespace MiniComp.Cache.Provide;

public class MemoryCacheService : ICacheService
{
    protected readonly IMemoryCache MemoryCache;
    protected readonly ConcurrentDictionary<string, SemaphoreSlim> Locks = new();

    public MemoryCacheService(IMemoryCache memoryCache)
    {
        MemoryCache = memoryCache;
    }

    public Task AddCacheAsync(string key, object? val, TimeSpan? timeSpan = null)
    {
        MemoryCache.Set(key, val, GetCacheOptions(timeSpan));
        return Task.CompletedTask;
    }

    public Task<T?> GetCacheAsync<T>(string key, TimeSpan? timeSpan = null)
    {
        if (!MemoryCache.TryGetValue(key, out var cachedValue))
            return Task.FromResult(default(T?));
        if (cachedValue is T typedValue)
        {
            return Task.FromResult<T?>(typedValue);
        }
        return Task.FromResult(default(T?));
    }

    public Task DeleteCacheAsync(string key)
    {
        MemoryCache.Remove(key);
        return Task.CompletedTask;
    }

    public Task<bool> IsExistAsync(string key)
    {
        return Task.FromResult(MemoryCache.TryGetValue(key, out _));
    }

    public Task KeyExpireAsync(string key, TimeSpan timeSpan)
    {
        if (MemoryCache.TryGetValue(key, out var value))
        {
            MemoryCache.Set(key, value, GetCacheOptions(timeSpan));
        }
        return Task.CompletedTask;
    }

    public async Task LockAsync(
        string lockKey,
        Func<Task> func,
        long expireMillisecond = 180000,
        long timeoutMillisecond = 180000
    )
    {
        var semaphore = Locks.GetOrAdd(lockKey, _ => new SemaphoreSlim(1, 1));
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutMillisecond));
        try
        {
            await semaphore.WaitAsync(cts.Token);
            await func();
        }
        catch (OperationCanceledException)
        {
            throw new LockTimeoutException();
        }
        finally
        {
            semaphore.Release();
        }
    }

    public async Task<T> GetOrCreateCacheAsync<T>(
        string key,
        Func<Task<T>> func,
        TimeSpan? time = null,
        bool isReset = false
    )
    {
        if (isReset)
        {
            MemoryCache.Remove(key);
        }
        var result = await GetCacheAsync<T>(key);
        if (result != null)
            return result;
        result = await func();
        await AddCacheAsync(key, result, time);
        return result;
    }

    private static MemoryCacheEntryOptions GetCacheOptions(TimeSpan? expiration)
    {
        var options = new MemoryCacheEntryOptions();
        if (expiration.HasValue)
        {
            options.SetAbsoluteExpiration(expiration.Value);
        }
        return options;
    }
}

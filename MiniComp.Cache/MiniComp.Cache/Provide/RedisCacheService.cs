using Medallion.Threading.Redis;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace MiniComp.Cache.Provide;

public class RedisCacheService : ICacheService
{
    protected readonly ConnectionMultiplexer Multiplexer;
    protected readonly IDatabase Db;

    public RedisCacheService(IOptions<CacheConfiguration> options)
    {
        Multiplexer = ConnectionMultiplexer.Connect(options.Value.ConnectionString);
        Db = Multiplexer.GetDatabase(0);
    }

    public async Task AddCacheAsync(string key, object? val, TimeSpan? timeSpan = null)
    {
        await Db.StringSetAsync(key, JsonConvert.SerializeObject(val), timeSpan);
    }

    public async Task<T?> GetCacheAsync<T>(string key, TimeSpan? timeSpan = null)
    {
        var val = await Db.StringGetAsync(key);
        if (timeSpan != null && val.HasValue)
            await KeyExpireAsync(key, timeSpan.Value);
        return val.HasValue ? JsonConvert.DeserializeObject<T>(val.ToString()) : default;
    }

    public async Task DeleteCacheAsync(string key)
    {
        await Db.KeyDeleteAsync(key);
    }

    public async Task<bool> IsExistAsync(string key)
    {
        return await Db.KeyExistsAsync(key);
    }

    public async Task KeyExpireAsync(string key, TimeSpan timeSpan)
    {
        await Db.KeyExpireAsync(key, timeSpan);
    }

    public async Task LockAsync(
        string lockKey,
        Func<Task> func,
        long expireMillisecond = 180000,
        long timeoutMillisecond = 180000
    )
    {
        var timeout = TimeSpan.FromMilliseconds(timeoutMillisecond);
        var @lock = new RedisDistributedLock(
            lockKey,
            Db,
            opt =>
            {
                opt.Expiry(TimeSpan.FromMilliseconds(expireMillisecond));
            }
        );
        await using var handle = await @lock.TryAcquireAsync(timeout);
        if (handle != null)
        {
            await func.Invoke();
        }
        else
        {
            throw new LockTimeoutException();
        }
    }

    public async Task<T> GetOrCreateCacheAsync<T>(
        string key,
        Func<Task<T>> func,
        TimeSpan? time = null,
        bool isReset = false
    )
    {
        time ??= TimeSpan.FromDays(1);
        var data = await GetCacheAsync<T>(key);
        if (data != null && !isReset)
            return data;
        data = await func.Invoke();
        await AddCacheAsync(key, data, time);
        return data;
    }
}

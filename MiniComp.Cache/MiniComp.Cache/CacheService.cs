using Medallion.Threading.Redis;
using Microsoft.Extensions.Options;
using MiniComp.Core.App;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace MiniComp.Cache;

public class CacheService(IOptions<ConnectionConfiguration> options) : ICacheService
{
    private readonly ConnectionMultiplexer _multiplexer = ConnectionMultiplexer.Connect(
        options.Value.ConnectionString
    );

    public IDatabase GetDatabase(int val = 0)
    {
        return _multiplexer.GetDatabase(val);
    }

    public string JoinPlatformCode(string key)
    {
        return $"{HostApp.HostSettings.AppId}:{key}";
    }

    #region 对象

    public async Task AddCacheAsync(
        string key,
        object? val,
        TimeSpan? timeSpan = null,
        bool isJoinPrefix = true
    )
    {
        if (val != null)
        {
            if (isJoinPrefix)
                key = JoinPlatformCode(key);
            var db = GetDatabase();
            await db.StringSetAsync(key, JsonConvert.SerializeObject(val), timeSpan);
        }
    }

    public async Task<T> GetCacheAsync<T>(
        string key,
        TimeSpan? timeSpan = null,
        bool isJoinPrefix = true
    )
    {
        if (isJoinPrefix)
            key = JoinPlatformCode(key);
        var db = GetDatabase();
        var val = await db.StringGetAsync(key);
        if (timeSpan != null && val.HasValue)
            await db.KeyExpireAsync(key, timeSpan);
        return (val.HasValue ? JsonConvert.DeserializeObject<T>(val.ToString()) : default)!;
    }

    public async Task DeleteCacheAsync(string key, bool isJoinPrefix = true)
    {
        if (isJoinPrefix)
            key = JoinPlatformCode(key);
        var db = GetDatabase();
        await db.KeyDeleteAsync(key);
    }

    public async Task<bool> IsExistAsync(string key, bool isJoinPrefix = true)
    {
        if (isJoinPrefix)
            key = JoinPlatformCode(key);
        var db = GetDatabase();
        return await db.KeyExistsAsync(key);
    }

    #endregion 对象

    #region 分布式锁

    public async Task LockAsync(
        string lockKey,
        Func<Task> func,
        long expireSeconds = 180000,
        long timeoutSeconds = 180000
    )
    {
        lockKey = JoinPlatformCode(lockKey);

        var topLock = !AsyncLocalData.LockKeyExistence(lockKey);

        if (!topLock)
        {
            await func.Invoke();
        }
        else
        {
            var db = GetDatabase();
            var timeout = TimeSpan.FromMilliseconds(timeoutSeconds);
            var @lock = new RedisDistributedLock(
                lockKey,
                db,
                opt =>
                {
                    opt.Expiry(TimeSpan.FromMilliseconds(expireSeconds));
                }
            );
            await using var handle = await @lock.TryAcquireAsync(timeout);
            if (handle != null)
            {
                try
                {
                    AsyncLocalData.AddLockKey(lockKey);
                    await func.Invoke();
                }
                finally
                {
                    AsyncLocalData.RemoveLockKey(lockKey);
                }
            }
            else
                throw new LockTimeoutException();
        }
    }

    #endregion 分布式锁

    #region 获取或创建缓存

    public async Task<T> GetOrCreateCacheAsync<T>(
        string key,
        Func<Task<T>> func,
        TimeSpan? time = null,
        bool isReset = false
    )
    {
        time ??= TimeSpan.FromDays(1);
        var data = await GetCacheAsync<T>(key, null);
        if (data == null || isReset)
        {
            data = await func.Invoke();
            await AddCacheAsync(key, data!, time);
        }
        return data;
    }

    #endregion 获取或创建缓存
}

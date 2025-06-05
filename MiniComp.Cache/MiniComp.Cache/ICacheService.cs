using StackExchange.Redis;

namespace MiniComp.Cache;

public interface ICacheService
{
    /// <summary>
    /// 插入缓存
    /// </summary>
    /// <param name="key"></param>
    /// <param name="val"></param>
    /// <param name="timeSpan"></param>
    /// <returns></returns>
    public Task AddCacheAsync(string key, object? val, TimeSpan? timeSpan = null);

    /// <summary>
    /// 获取缓存
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="timeSpan"></param>
    /// <returns></returns>
    public Task<T?> GetCacheAsync<T>(string key, TimeSpan? timeSpan = null);

    /// <summary>
    /// 删除缓存
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public Task DeleteCacheAsync(string key);

    /// <summary>
    /// 是否存在
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public Task<bool> IsExistAsync(string key);

    /// <summary>
    /// 设置过期时间
    /// </summary>
    /// <param name="key"></param>
    /// <param name="timeSpan"></param>
    /// <returns></returns>
    public Task KeyExpireAsync(string key, TimeSpan timeSpan);

    /// <summary>
    /// 获取分布式锁
    /// </summary>
    /// <param name="lockKey">锁键</param>
    /// <param name="func">获取锁后执行的委托</param>
    /// <param name="expireMillisecond">锁过期时间，防止死锁</param>
    /// <param name="timeoutMillisecond">多少毫秒后超时</param>
    /// <returns></returns>
    public Task LockAsync(
        string lockKey,
        Func<Task> func,
        long expireMillisecond = 180000,
        long timeoutMillisecond = 180000
    );

    /// <summary>
    /// 获取或创建缓存
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="func"></param>
    /// <param name="time"></param>
    /// <param name="isReset"></param>
    /// <returns></returns>
    public Task<T> GetOrCreateCacheAsync<T>(
        string key,
        Func<Task<T>> func,
        TimeSpan? time = null,
        bool isReset = false
    );
}

using StackExchange.Redis;

namespace MiniComp.Cache;

public interface ICacheService
{
    /// <summary>
    /// 获取RedisDb
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public IDatabase GetDatabase(int index = 0);

    /// <summary>
    /// 拼接平台代码
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public string JoinPlatformCode(string key);

    #region 对象

    /// <summary>
    /// 插入缓存
    /// </summary>
    /// <param name="key"></param>
    /// <param name="val"></param>
    /// <param name="timeSpan"></param>
    /// <param name="isJoinPrefix"></param>
    /// <returns></returns>
    public Task AddCacheAsync(
        string key,
        object? val,
        TimeSpan? timeSpan = null,
        bool isJoinPrefix = true
    );

    /// <summary>
    /// 获取缓存
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="timeSpan"></param>
    /// <param name="isJoinPrefix"></param>
    /// <returns></returns>
    public Task<T> GetCacheAsync<T>(
        string key,
        TimeSpan? timeSpan = null,
        bool isJoinPrefix = true
    );

    /// <summary>
    /// 删除缓存
    /// </summary>
    /// <param name="key"></param>
    /// <param name="isJoinPrefix"></param>
    /// <returns></returns>
    public Task DeleteCacheAsync(string key, bool isJoinPrefix = true);

    /// <summary>
    /// 是否存在
    /// </summary>
    /// <param name="key"></param>
    /// <param name="isJoinPrefix"></param>
    /// <returns></returns>
    public Task<bool> IsExistAsync(string key, bool isJoinPrefix = true);

    #endregion 对象

    #region 分布式锁

    /// <summary>
    /// 获取分布式锁
    /// </summary>
    /// <param name="lockKey">锁键</param>
    /// <param name="func">获取锁后执行的委托</param>
    /// <param name="timeoutSeconds">多少毫秒后超时</param>
    /// <param name="expireSeconds">锁过期时间，防止死锁</param>
    /// <returns></returns>
    public Task LockAsync(
        string lockKey,
        Func<Task> func,
        long expireSeconds = 180000,
        long timeoutSeconds = 180000
    );

    #endregion 分布式锁

    #region 获取或创建缓存

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

    #endregion 获取或创建缓存
}

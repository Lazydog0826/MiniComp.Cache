namespace MiniComp.Cache;

internal static class AsyncLocalData
{
    /// <summary>
    /// 已获取的锁
    /// </summary>
    private static readonly AsyncLocal<List<string>> LockKeyList = new();

    /// <summary>
    /// 获取已获取的锁集合
    /// </summary>
    /// <returns></returns>
    private static List<string> GetLockKeyList() => LockKeyList.Value ??= [];

    /// <summary>
    /// 新增Key
    /// </summary>
    /// <param name="key"></param>
    internal static void AddLockKey(string key)
    {
        if (!LockKeyExistence(key))
            GetLockKeyList().Add(key);
    }

    /// <summary>
    /// Key是否存在
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    internal static bool LockKeyExistence(string key)
    {
        return GetLockKeyList().Any(x => x == key);
    }

    /// <summary>
    /// 移除Key
    /// </summary>
    /// <param name="key"></param>
    internal static void RemoveLockKey(string key)
    {
        if (LockKeyExistence(key))
            GetLockKeyList().RemoveAll(x => x == key);
    }
}

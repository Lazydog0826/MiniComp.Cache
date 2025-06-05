namespace MiniComp.Cache;

public class CacheConfiguration
{
    /// <summary>
    /// 连接字符串
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// 是否使用Redis
    /// </summary>
    public bool IsUseRedis { get; set; } = false;
}

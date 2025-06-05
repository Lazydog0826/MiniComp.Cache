using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MiniComp.Cache.Provide;
using MiniComp.Core.App;
using MiniComp.Core.Extension;

namespace MiniComp.Cache;

public static class Setup
{
    /// <summary>
    /// 注入缓存服务
    /// </summary>
    /// <param name="services"></param>
    /// <param name="serviceLifetime"></param>
    /// <returns></returns>
    public static IServiceCollection AddCacheService(
        this IServiceCollection services,
        ServiceLifetime serviceLifetime = ServiceLifetime.Singleton
    )
    {
        var configSection = HostApp.Configuration.GetSection(nameof(CacheConfiguration));
        var config =
            configSection.Get<CacheConfiguration>()
            ?? throw new Exception("未配置CacheConfiguration");
        services.Configure<CacheConfiguration>(configSection);
        if (config.IsUseRedis)
        {
            services.Inject(serviceLifetime, typeof(ICacheService), typeof(RedisCacheService));
        }
        else
        {
            services.AddMemoryCache();
            services.Inject(serviceLifetime, typeof(ICacheService), typeof(MemoryCacheService));
        }
        return services;
    }
}

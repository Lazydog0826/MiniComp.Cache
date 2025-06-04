using Microsoft.Extensions.DependencyInjection;
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
        services.Configure<ConnectionConfiguration>(HostApp.Configuration.GetSection("Redis"));
        services.Inject(serviceLifetime, typeof(ICacheService), typeof(CacheService));
        return services;
    }
}

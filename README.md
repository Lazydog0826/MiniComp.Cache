# MiniComp.Cache
```csharp
var builder = WebApplication.CreateBuilder(args);
HostApp.Configuration = builder.Configuration;
builder.Services.AddCacheService();

// ICacheService和sqlsugar冲突
using ICacheService = MiniComp.Cache.ICacheService;
```

### 配置文件

```json
{
    "CacheConfiguration": {
        "ConnectionString": "localhost:6379,password=yourpassword,abortConnect=true",
        "IsUseRedis": false
    }
}
```

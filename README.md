# CoreLogger 自研高性能日志插件
基于 .NET 原生日志接口实现的自定义日志组件，专为 ASP.NET Core 项目深度优化，
解决传统自定义日志 **启动阻塞 Swagger、进程 GC 闪退、文件 IO 冲突、高并发内存泄漏** 等问题。

## ✨ 功能特性
- ✅ 完全兼容 .NET 标准 `ILogger` / `ILoggerProvider` 接口，无缝对接原有业务代码
- ✅ 异步队列消费日志，非阻塞 HTTP 主线程，高并发场景性能优异
- ✅ 双输出终端：控制台输出 + 本地日志文件输出
- ✅ 智能文件管理：支持按大小/周期自动滚动、旧日志清理、GZip 压缩备份
- ✅ 内置敏感信息脱敏：默认支持手机号、身份证、邮箱，可自定义正则规则
- ✅ 多维度日志过滤：日志级别、模块黑白名单、关键字屏蔽
- ✅ 支持日志 `Scope` 作用域，实现 TraceId、用户ID、操作链路追踪
- ✅ 双配置模式：代码动态配置 + `appsettings.json` 配置文件配置
- ✅ DI 全局单例托管生命周期，彻底避免配置对象被 GC 回收导致进程闪退

## 🎯 运行环境与依赖
### 1. 适配框架 & 项目类型
- 运行框架：.NET 6 / .NET 7 / .NET 8
- 支持项目：ASP.NET Core WebAPI、MVC、控制台应用、Windows 服务

### 2. 项目依赖
组件基于 .NET 原生类库开发，**无需安装任何第三方 NuGet 包**，仅依赖框架内置库：
- `Microsoft.Extensions.Logging`
- `Microsoft.Extensions.DependencyInjection`
- `Microsoft.Extensions.Configuration`
- `System.IO.Compression`

### 3. 项目引入方式
1. **源码引用**：将 `CoreLogger` 类库添加到解决方案，主项目添加「项目引用」
2. **NuGet 引用**：组件打包后，在 NuGet 包管理器搜索 `CoreLogger` 完成安装

## ⚠️ 重要前置约束（必看）
> [!WARNING]
> **Web 项目强制禁止**调用 `builder.AddProvider()` 接入 .NET 全局日志管线。
> 该操作会在应用启动早期执行重型后台任务，抢占系统资源，直接造成 **Swagger 无法访问、路由卡死、项目启动阻塞**。
> 仅通过 DI 单例注册即可正常使用全部日志功能。

## 🚀 快速接入
### 1. 引入命名空间
在 `Program.cs` 文件头部引入以下命名空间：
```cs
using CoreLogger.Extensions;
using CoreLogger.Entiy;
using Microsoft.Extensions.Logging;
```

### 2. 方式一：代码动态配置（推荐）
灵活度高，适合需要差异化配置的场景，完整注册代码如下：
```cs
var builder = WebApplication.CreateBuilder(args);
// 可选：清空系统默认日志，如需保留框架原生日志可注释此行
builder.Logging.ClearProviders();
// 注册自定义日志组件并配置参数
builder.Logging.AddIndustrialLogger(opt =>
{
    // 全局基础配置
    opt.GlobalMinLevel = LogLevel.Information;
    opt.SampleRate = 1.0;
    opt.BatchSize = 50;
    opt.ChannelCapacity = 1024 * 1024;
    opt.FlushIntervalMs = 1000;
    opt.TimestampType = TimestampType.Beijing;

    // 控制台输出配置
    opt.Console.Enabled = true;
    opt.Console.MinLevel = LogLevel.Debug;
    opt.Console.Format = LogFormatType.Text;

    // 文件输出配置
    opt.File.Enabled = true;
    opt.File.MinLevel = LogLevel.Information;
    opt.File.Format = LogFormatType.Text;
    opt.File.RootPath = Path.Combine(AppContext.BaseDirectory, "MyLogs");
    opt.File.MaxFileSizeMB = 20;
    opt.File.MaxBackupCount = 20;
    opt.File.RollingInterval = RollingInterval.Day;
    opt.File.EnableCompress = true;
    opt.File.FileName = "app-log";

    // 日志过滤：屏蔽微软框架冗余日志
    opt.Filter.Enabled = true;
    opt.Filter.ExcludeModules.Add("Microsoft");

    // 敏感信息脱敏配置
    opt.SensitiveMask.Enabled = true;
    opt.SensitiveMask.Rules.Add(@"\d+@\w+\.\w+", "***@***");
});

// 注册系统默认服务
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
// 启用 Swagger 中间件
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseAuthorization();
app.MapControllers();
app.Run();
```

### 3. 方式二：配置文件配置
#### 3.1 在 appsettings.json 添加配置节点
```cs
json
{
  "Logging": {
    "IndustrialLogger": {
      "GlobalMinLevel": "Information",
      "SampleRate": 1.0,
      "BatchSize": 50,
      "ChannelCapacity": 1048576,
      "FlushIntervalMs": 1000,
      "TimestampType": "Beijing",
      "Console": {
        "Enabled": true,
        "MinLevel": "Debug",
        "Format": "Text"
      },
      "File": {
        "Enabled": true,
        "MinLevel": "Information",
        "Format": "Text",
        "RootPath": "MyLogs",
        "MaxFileSizeMB": 20,
        "MaxBackupCount": 20,
        "RollingInterval": "Day",
        "EnableCompress": true,
        "FileName": "app-log"
      },
      "Filter": {
        "Enabled": true,
        "ExcludeModules": [ "Microsoft" ]
      },
      "SensitiveMask": {
        "Enabled": true,
        "Rules": {
          "\\d+@\\w+\\.\\w+": "***@***"
        }
      }
    }
  }
}
```

#### 3.2 代码读取配置并注册
```cs
// 从配置文件加载日志配置并完成注册
builder.Logging.AddIndustrialLogger(builder.Configuration.GetSection("Logging:IndustrialLogger"));
📖 日志使用指南
方案一：DI 注入原生 ILogger（Web 项目首选 ✅）
全局复用一套日志引擎，资源开销最小、高并发友好，完全兼容 .NET 原生日志语法。

using Microsoft.AspNetCore.Mvc;
using CoreLogger.DI;
using Microsoft.Extensions.Logging;
namespace TestWebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController(NetLoggerProvider logProvider) : ControllerBase
    {
        // 根据控制器分类创建日志实例
        private readonly ILogger _appLogger = logProvider.CreateLogger(nameof(TestController));

        /// <summary>多级别日志输出测试</summary>
        [HttpGet("test")]
        public IActionResult TestLog()
        {
            // 全级别日志调用
            _appLogger.LogTrace("系统跟踪日志");
            _appLogger.LogDebug("系统调试日志");
            _appLogger.LogInformation("普通业务日志");
            _appLogger.LogWarning("系统警告日志");
            _appLogger.LogError(new Exception("测试异常"), "业务错误日志");
            _appLogger.LogCritical("系统致命错误日志");

            // 敏感信息自动脱敏测试
            _appLogger.LogInformation("用户信息：手机号13812345678，身份证110101199003074567，邮箱test@163.com");

            return Ok("日志写入完成");
        }

        /// <summary>日志作用域（全链路追踪）</summary>
        [HttpGet("scope")]
        public IActionResult TestScope()
        {
            using (_appLogger.BeginScope(new
            {
                TraceId = Guid.NewGuid(),
                UserId = 10001,
                Operation = "订单支付"
            }))
            {
                _appLogger.LogInformation("订单创建成功");
                _appLogger.LogInformation("支付流程执行完成");
            }
            return Ok("链路追踪日志写入完成");
        }
    }
}
方案二：独立日志实例（多模块 / 多文件隔离）
适用于多租户、多业务模块场景，实现不同模块日志物理文件隔离。
[!WARNING]
禁止在控制器内部直接 new 日志实例。控制器为「每请求实例」，高并发下会造成线程、文件句柄泄漏。
必须在 DI 中注册为全局单例。
1. 注册独立日志实例（Program.cs）
cs
// 为独立模块注册专属日志实例
builder.Services.AddSingleton<CoreLog>(sp =>
{
    var opt = new LoggerOptions
    {
        File =
        {
            RootPath = Path.Combine(AppContext.BaseDirectory, "ModuleA-Logs"),
            FileName = "module-a-log"
        }
    };
    return new CoreLog(opt);
});
2. 控制器注入并使用
cs
[ApiController]
[Route("api/modulea")]
public class ModuleAController(CoreLog moduleALogger) : ControllerBase
{
    [HttpGet("log")]
    public IActionResult ModuleALogTest()
    {
        moduleALogger.Enqueue(LogLevel.Information, "ModuleA", "模块A业务日志");
        moduleALogger.Enqueue(LogLevel.Error, "ModuleA", "模块A异常", new Exception("模块接口报错"));
        return Ok("模块独立日志写入完成");
    }
}
方案三：静态帮助类（仅控制台 / 桌面程序使用）
[!WARNING]
Web 项目禁止使用静态日志类，容易造成多实例资源冲突。
cs
// 全局初始化配置（项目启动时仅执行一次）
SysLogHelper.Configure(opt =>
{
    opt.File.RootPath = "ConsoleLogs";
});
```

# 静态方式调用日志
SysLogHelper.Info("控制台普通日志");
SysLogHelper.Error("控制台错误日志", new Exception("运行异常"));

# 完整配置项说明
1. 根配置 LoggerOptions
配置字段	类型	默认值	说明
GlobalMinLevel	LogLevel	Information	全局日志最低输出级别
SampleRate	double	1.0	日志采样率（0~1），用于高流量削峰
BatchSize	int	50	队列单次批量消费日志条数
ChannelCapacity	int	1048576	异步队列最大容量
FlushIntervalMs	int	1000	定时强制刷盘间隔（毫秒）
TimestampType	TimestampType	Beijing	日志时间戳时区

2. 控制台配置 ConsoleSinkOptions
配置字段	类型	默认值	说明
Enabled	bool	true	是否开启控制台输出
MinLevel	LogLevel	Debug	控制台独立日志级别
Format	LogFormatType	Text	输出格式：纯文本 / JSON

4. 文件配置 FileSinkOptions
配置字段	类型	默认值	说明
Enabled	bool	true	是否开启文件日志输出
MinLevel	LogLevel	Information	文件日志最低输出级别
RootPath	string	logs	日志文件根目录
MaxFileSizeMB	int	10	单日志文件最大容量 (MB)
MaxBackupCount	int	30	最大备份文件数量
RollingInterval	RollingInterval	Day	文件滚动周期：按分 / 小时 / 天
EnableCompress	bool	true	旧备份文件是否开启 GZip 压缩
FileName	string	log	日志主文件名

5. 脱敏配置 SensitiveMaskOptions
配置字段	类型	默认值	说明
Enabled	bool	true	是否开启敏感信息脱敏
Rules	Dictionary<string,string>	内置通用规则	自定义正则脱敏规则

6. 过滤配置 LogFilterOptions
配置字段	类型	默认值	说明
Enabled	bool	true	是否开启日志过滤
IncludeModules	List<string>	空	模块白名单，仅输出指定模块日志
ExcludeModules	List<string>	空	模块黑名单，屏蔽指定模块日志
BlockKeywords	List<string>	空	屏蔽包含指定关键字的日志

# ♻️ 资源释放说明
1. DI 托管实例（推荐方式）
通过 AddSingleton 注册的日志实例、NetLoggerProvider：
Web 宿主停止时，DI 容器会自动执行 DisposeAsync，自动关闭后台线程、定时器、文件流、异步队列，无需手动编写释放代码。
2. 手动创建实例释放
手动 new 生成的独立日志实例，需要自行实现资源释放：
```cs
public class BusinessService : IAsyncDisposable
{
    private readonly CoreLog _customLog = new(new LoggerOptions());

    public async ValueTask DisposeAsync()
    {
        if (_customLog is IAsyncDisposable disposable)
        {
            await disposable.DisposeAsync();
        }
    }
}
```

# ❓ 常见问题 & 排查方案
Q1：启动后 Swagger / 路由无法访问
原因：代码中使用了 builder.AddProvider() 接入全局日志管线，启动阶段阻塞 Web 管道。
解决方案：删除 builder.AddProvider(provider) 代码，仅保留 DI 单例注册。
Q2：程序运行过程中无故闪退
原因：日志配置对象为局部变量，被 GC 提前回收，后台线程访问无效内存。
解决方案：确保 NetLoggerProvider 已注册为 DI 全局单例。
Q3：日志文件被占用、内容错乱
原因：多个日志实例共用同一个日志目录 / 文件。
解决方案：为不同日志实例配置独立目录、独立文件名，物理隔离文件 IO。
Q4：高并发下内存、文件句柄持续上涨
原因：在控制器中频繁 new 日志实例，不断创建后台线程与文件流。
解决方案：统一使用 DI 单例日志，禁止在瞬时生命周期类中动态实例化日志。
Q5：敏感信息脱敏功能不生效
解决方案：检查脱敏总开关 SensitiveMask.Enabled 是否开启、校验正则表达式语法、核对日志输出级别。
Q6：日志完全无输出
解决方案：逐级检查全局日志级别、模块过滤规则、控制台 / 文件输出开关。

# 🏭 生产环境最佳实践
注册规范：仅使用 AddSingleton 注册日志，严禁使用 builder.AddProvider()。
使用规范：优先使用 DI 注入 ILogger 全局日志；模块隔离日志统一注册为 DI 单例。
文件规范：多日志实例必须隔离目录与文件；生产环境建议开启日志压缩与自动清理。
配置规范：生产环境调高全局日志级别为 Warning，减少无效日志输出，提升性能。
权限规范：Linux 服务器需保证日志目录拥有读写权限，避免文件创建失败。
禁用规范：Web 项目禁止使用 SysLogHelper 静态日志类。

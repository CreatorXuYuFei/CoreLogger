using Microsoft.AspNetCore.Mvc;
using CoreLogger.Core;
using CoreLogger.Extensions;
using CoreLogger.DI;

namespace TestWebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController(NetLoggerProvider logProvider) : ControllerBase
    {
        // 构造函数注入 ILogger<T>，框架自动解析到自研日志组件
        private readonly CoreLog _corelogger = new(new CoreLogger.Entiy.LoggerOptions(/*加配置项*/));
        //例子DI 手动注入官方日志总线
        /*
        控制器构造函数 → DI 注入 全局单例 NetLoggerProvider
        ↓
        调用 NetLoggerProvider.CreateLogger("TestController")
                ↓
        内部返回NetLogger（实现 ILogger）
                ↓
        赋值给 ILogger*/
        private readonly ILogger _logger = logProvider.CreateLogger(nameof(TestController));

        [HttpGet("test")]
        public IActionResult TestLog()
        {
            // 原生日志级别调用
            // 写日志
            _corelogger.Enqueue(LogLevel.Information, "日志已保存为：backend_service_log.log", "Use");
            _corelogger.Enqueue(LogLevel.Error, "测试错误日志", "Default", new Exception("测试"));

            // ==============================================
            // 🔥 2. 全级别日志测试
            // ==============================================
            _corelogger.Enqueue(LogLevel.Information, "调试日志：系统初始化完成", "User");
            // ==============================================
            // 🔥 3. 敏感信息脱敏测试（自动隐藏）
            // ==============================================
            _corelogger.Enqueue(LogLevel.Information, "用户信息：手机号13812345678，身份证110101199003074567，邮箱test@163.com","User");
            _corelogger.Enqueue(LogLevel.Information, "CSSSSSS", "User");

            // ==============================================
            // 🔥 4. 日志作用域（上下文追踪：TraceId/用户ID）
            // ==============================================
            using (LogScope.BeginScope(new { TraceId = Guid.NewGuid(), UserId = 1001, Operation = "支付" }))
            {
                _corelogger.Enqueue(LogLevel.Information, "订单创建成功", "Order"); // 日志自动携带上下文信息
            }

            return Ok("日志写入完成");
        }

        [HttpGet("test2")]
        public IActionResult Test2Log()
        {
            // 原生日志级别调用
            _logger.LogInformation("测试自定义日志 123456");

            return Ok("日志写入完成");
        }
    }
}

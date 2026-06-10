using CoreLogger.DI;
using CoreLogger.Entiy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CoreLogger.Extensions
{
    public static class LoggerExtensions
    {
        public static ILoggingBuilder AddIndustrialLogger(this ILoggingBuilder builder, Action<LoggerOptions> config)
        {
            var opt = new LoggerOptions(); 
            config(opt);
            var Provider = new NetLoggerProvider(opt);

            //注入到容器单例进行生命周期托管保证存活
            builder.Services.AddSingleton(Provider);

            //不能后台注人日志管线，日志后台为重型任务，会堵塞后续容器注入， 建议使用前一行注入的单例构造使用方式
            //builder.AddProvider(Provider);

            //例子DI 注入全局单例的日志提供者
            //public TestController(NetLoggerProvider logProvider)
            //{
            //    _logger = logProvider.CreateLogger(nameof(TestController));
            //}

            return builder;
        }
        public static ILoggingBuilder AddIndustrialLogger(this ILoggingBuilder builder, IConfiguration cfg)
        {
            var opt = cfg.GetSection("Logging:IndustrialLogger").Get<LoggerOptions>() ?? new LoggerOptions();
            builder.AddProvider(new NetLoggerProvider(opt)); return builder;
        }
    }
}

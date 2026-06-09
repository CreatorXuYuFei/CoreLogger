using CoreLogger.DI;
using CoreLogger.Entiy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CoreLogger.Extensions
{
    public static class LoggerExtensions
    {
        public static ILoggingBuilder AddIndustrialLogger(this ILoggingBuilder builder, Action<LoggerOptions> config)
        {
            var opt = new LoggerOptions(); config(opt); builder.AddProvider(new NetLoggerProvider(opt)); return builder;
        }
        public static ILoggingBuilder AddIndustrialLogger(this ILoggingBuilder builder, IConfiguration cfg)
        {
            var opt = cfg.GetSection("Logging:IndustrialLogger").Get<LoggerOptions>() ?? new LoggerOptions();
            builder.AddProvider(new NetLoggerProvider(opt)); return builder;
        }
    }
}

using CoreLogger.Abstractions;
using CoreLogger.Entiy;
using CoreLogger.Enum;
using CoreLogger.Extensions;
using CoreLogger.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using CoreLogger.Core;

namespace LogService
{
    #region 静态调用入口
    public static class SysLogHelper
    {
        private static readonly CoreLog _default = new(new LoggerOptions());
        ///<summary>
        ///直接创建静态日志插件调用方式
        ///在控制台程序使用时需注意在控制台关闭时调用手动释放方法Shutdown()
        ///</summary>
        ///<param name="cfg">配置项</param>
        public static void Configure(Action<LoggerOptions> cfg) => cfg(_default.Options);

        //全级别日志方法
        public static void Debug(string msg, string module = "Default") => _default.Enqueue(LogLevel.Debug, module, msg);
        public static void Info(string msg, string module = "Default") => _default.Enqueue(LogLevel.Information, module, msg);
        public static void Warn(string msg, string module = "Default") => _default.Enqueue(LogLevel.Warning, module, msg);
        public static void Error(string msg, Exception? ex = null, string module = "Default") => _default.Enqueue(LogLevel.Error, module, msg, ex);
        public static void Critical(string msg, Exception? ex = null, string module = "Default") => _default.Enqueue(LogLevel.Critical, module, msg, ex);

        //作用域
        public static IDisposable BeginScope<T>(T state) where T : notnull => LogScope.BeginScope(state);

        //手动关闭方法释放资源
        public static void Shutdown()
        {
            _default.Dispose();
        }
    }
    #endregion
}
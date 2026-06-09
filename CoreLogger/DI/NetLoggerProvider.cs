using CoreLogger.Core;
using CoreLogger.Entiy;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreLogger.DI
{
    [ProviderAlias("IndustrialLogger")]
    public class NetLoggerProvider(LoggerOptions options) : ILoggerProvider, IAsyncDisposable
    {
        private readonly CoreLog _core = new(options);
        private readonly ConcurrentDictionary<string, NetLogger> _loggers = new();
        //防止重复释放
        private bool _disposed = false;

        public ILogger CreateLogger(string categoryName)
        {
            return _loggers.GetOrAdd(categoryName, k => new NetLogger(_core, k));
        }

        #region 标准 Dispose 模式
        public void Dispose()
        {
            //调用释放逻辑 + 抑制终结器
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public ValueTask DisposeAsync()
        {
            //异步释放 + 抑制终结器
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
            return _core.DisposeAsync();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            //释放托管资源
            if (disposing)
            {
                _core.Dispose();
            }

            _disposed = true;
        }

        //终结器（防止未手动调用 Dispose）
        ~NetLoggerProvider()
        {
            Dispose(disposing: false);
        }
        #endregion
    }
}

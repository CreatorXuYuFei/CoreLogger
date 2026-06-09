using CoreLogger.Core;
using CoreLogger.Extensions;
using Microsoft.Extensions.Logging;

namespace CoreLogger.DI
{
    public class NetLogger : ILogger
    {
        private readonly CoreLog _core;
        private readonly string _category;

        internal NetLogger(CoreLog core, string category) => (_core, _category) = (core, category);
        public IDisposable BeginScope<TState>(TState state) where TState : notnull => LogScope.BeginScope(state);
        public bool IsEnabled(LogLevel logLevel) => logLevel >= _core.Options.GlobalMinLevel && logLevel != LogLevel.None;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel)) return;
            _core.Enqueue(logLevel, _category, formatter(state, exception), exception);
        }
    }
}

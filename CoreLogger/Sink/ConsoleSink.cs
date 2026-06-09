using CoreLogger.Abstractions;
using CoreLogger.Core;
using CoreLogger.Entiy;
using CoreLogger.Enum;
using CoreLogger.Utils;
using System.Collections.Concurrent;
using System.Text.Json;

namespace CoreLogger.Sink
{
    internal sealed class ConsoleSink(CoreLog logger) : ILogSink
    {
        private readonly CoreLog _logger = logger;
        private static readonly ConcurrentQueue<string> _consoleQueue = new();
        //开关：用来停止后台任务
        private static readonly CancellationTokenSource _cts = new();
        static ConsoleSink()
        {
            //直接启动后台任务，无需持有引用
            _ = RunConsoleWorker();
        }

        public Task WriteAsync(List<LogMessage> batch)
        {
            if (!_logger.Options.Console.Enabled) return Task.CompletedTask;
            foreach (var msg in batch)
            {
                if (msg.Level < _logger.Options.Console.MinLevel) continue;
                var content = _logger.Options.Console.Format == LogFormatType.Json
                    ? JsonSerializer.Serialize(msg, _logger.JsonOptions)
                    : TemplateEngine.Render(_logger.Options.TextTemplate, msg);
                _consoleQueue.Enqueue(content);
            }
            return Task.CompletedTask;
        }

        private static async Task RunConsoleWorker()
        {
            while (!_cts.IsCancellationRequested)
            {
                if (_consoleQueue.TryDequeue(out var line))
                    Console.WriteLine(line);
                else
                    await Task.Delay(5);
            }
        }

        public Task FlushAsync() => Task.CompletedTask;

        public Task CloseAsync()
        {
            _cts.Cancel();
            return Task.CompletedTask;
        }
    }
}

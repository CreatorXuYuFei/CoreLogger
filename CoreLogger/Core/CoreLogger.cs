using CoreLogger.Abstractions;
using CoreLogger.Entiy;
using CoreLogger.Extensions;
using CoreLogger.Sink;
using CoreLogger.Utils;
using LogService;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using System.Threading.Channels;

namespace CoreLogger.Core
{
    
    internal sealed class CoreLog : IAsyncDisposable
    {
        private readonly LoggerOptions _options;
        private readonly ObjectPool<StringBuilder> _sbPool = new DefaultObjectPool<StringBuilder>(new StringBuilderPooledObjectPolicy());
        private readonly Channel<LogMessage> _logChannel;
        private readonly List<ILogSink> _sinks = [];
        private readonly Timer _flushTimer;
        private readonly Task _consumerTask;
        private readonly CancellationTokenSource _cts = new();
        private readonly JsonSerializerOptions _jsonOptions;
        private long _dropCount;

        internal JsonSerializerOptions JsonOptions => _jsonOptions;
        internal LoggerOptions Options => _options;
        internal ObjectPool<StringBuilder> SbPool => _sbPool;

        public CoreLog(LoggerOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _logChannel = Channel.CreateBounded<LogMessage>(new BoundedChannelOptions(options.ChannelCapacity)
            {
                FullMode = BoundedChannelFullMode.DropOldest,
                SingleReader = true
            });

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
            };

            _sinks.Add(new ConsoleSink(this));
            _sinks.Add(new FileSink(this));
            _consumerTask = Task.Factory.StartNew(ConsumeLoopAsync, _cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            _flushTimer = new Timer(async _ => await FlushAsync(), null, options.FlushIntervalMs, options.FlushIntervalMs);
        }

        public void Enqueue(LogLevel level, string module, string message, Exception? ex = null)
        {
            try
            {
                if (!CheckFilter(module, message) || level < _options.GlobalMinLevel || Random.Shared.NextDouble() > _options.SampleRate) return;
                message = MaskEngine.Mask(message, _options.SensitiveMask);

                var now = TimeHelper.GetTimestamp(_options.TimestampType);
                var msg = new LogMessage { Level = level, Module = module, Message = message, Exception = ex, Scope = LogScope.Current, Time = now };
                if (!_logChannel.Writer.TryWrite(msg)) Interlocked.Increment(ref _dropCount);
            }
            catch (Exception exc)
            {
                Console.WriteLine($"日志记录失败！[LogCoreError] Enqueue失败: {exc}");
            }
        }

        private bool CheckFilter(string module, string msg)
        {
            var f = _options.Filter;
            if (!f.Enabled) return true;
            if (f.ExcludeModules.Any(m => module.Contains(m))) return false;
            if (f.IncludeModules.Count > 0 && !f.IncludeModules.Any(m => module.Contains(m))) return false;
            if (f.BlockKeywords.Any(k => msg.Contains(k))) return false;
            return true;
        }

        private async Task ConsumeLoopAsync()
        {
            var reader = _logChannel.Reader;
            var batch = new List<LogMessage>(_options.BatchSize);
            while (await reader.WaitToReadAsync(_cts.Token))
            {
                batch.Clear();
                while (reader.TryRead(out var msg) && batch.Count < _options.BatchSize) batch.Add(msg);
                if (batch.Count == 0) continue;
                try { await Task.WhenAll(_sinks.Select(s => s.WriteAsync(batch))); } catch { }
            }
        }

        private async Task FlushAsync() => await Task.WhenAll(_sinks.Select(s => s.FlushAsync()));
        public async ValueTask DisposeAsync()
        {
            _cts.Cancel(); _flushTimer.Dispose(); _logChannel.Writer.Complete();
            await _consumerTask.WaitAsync(TimeSpan.FromSeconds(10));
            await FlushAsync(); await Task.WhenAll(_sinks.Select(s => s.CloseAsync()));
        }
        public void Dispose() => DisposeAsync().AsTask().Wait(1000);
    }
}

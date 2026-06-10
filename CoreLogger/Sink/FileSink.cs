using CoreLogger.Abstractions;
using CoreLogger.Core;
using CoreLogger.Entiy;
using CoreLogger.Enum;
using CoreLogger.Utils;
using System.IO.Compression;
using System.Text;
using System.Text.Json;

namespace CoreLogger.Sink
{
    internal sealed class FileSink : ILogSink
    {
        private readonly CoreLog _logger;
        private readonly ConcurrentExpireCache<string, FileMeta> _fileCache;
        private static readonly Encoding _encoding = Encoding.UTF8;
        private long _sequence;

        public FileSink(CoreLog logger)
        {
            _logger = logger;
            _fileCache = new ConcurrentExpireCache<string, FileMeta>(f => DateTime.UtcNow - f.LastAccessTime > TimeSpan.FromMinutes(_logger.Options.FileExpireMinutes));
        }

        public async Task WriteAsync(List<LogMessage> batch)
        {
            var opt = _logger.Options.File;
            if (!opt.Enabled) return;

            foreach (var g in batch.GroupBy(x => (x.Module, Time: GetTimeFolder(x.Time))))
            {
                var dir = Path.Combine(opt.RootPath, g.Key.Module, g.Key.Time);
                Directory.CreateDirectory(dir);
                //使用配置的文件名
                var meta = GetOrCreateFile(Path.Combine(dir, _logger.Options.File.FileName));

                var sb = _logger.SbPool.Get();
                try
                {
                    foreach (var msg in g)
                    {
                        if (msg.Level < opt.MinLevel) continue;
                        sb.AppendLine(opt.Format == LogFormatType.Json
                            ? JsonSerializer.Serialize(msg, _logger.JsonOptions)
                            //传入时区配置
                            : TemplateEngine.Render(_logger.Options.TextTemplate, msg));
                    }
                    var content = sb.ToString();
                    var bytes = _encoding.GetByteCount(content);
                    if (meta.CurrentSize + bytes > opt.MaxFileSizeMB * 1024 * 1024)
                    {
                        await RollFile(meta);
                        //使用配置文件名
                        meta = GetOrCreateFile(Path.Combine(dir, _logger.Options.File.FileName));
                    }
                    await meta.Writer.WriteAsync(content);
                    meta.CurrentSize += bytes;
                    meta.LastAccessTime = DateTime.UtcNow;
                }
                finally { _logger.SbPool.Return(sb); }
            }
            _fileCache.CleanupExpired();
        }

        ///<summary>
        ///滚动文件 + 自动清理旧备份
        ///</summary>
        private async Task RollFile(FileMeta meta)
        {
            await meta.Writer.FlushAsync();
            meta.Writer.Dispose();
            var dest = $"{meta.FilePath}_{DateTime.UtcNow:HHmmss}_{Interlocked.Increment(ref _sequence)}_{Guid.NewGuid():N}";
            var opt = _logger.Options.File;

            if (opt.EnableCompress)
            {
                using var fs = new FileStream(meta.FilePath, FileMode.Open);
                using var gz = new GZipStream(new FileStream($"{dest}.gz", FileMode.Create), CompressionLevel.Optimal);
                await fs.CopyToAsync(gz);
                File.Delete(meta.FilePath);
            }
            else
                File.Move(meta.FilePath, $"{dest}.log");

            //自动删除超出备份数的旧文件
            CleanupOldBackups(Path.GetDirectoryName(meta.FilePath)!, opt.MaxBackupCount);
        }

        ///<summary>
        ///按创建时间排序，保留最新N个，删除旧文件
        ///</summary>
        private static void CleanupOldBackups(string logDir, int maxCount)
        {
            if (maxCount <= 0 || !Directory.Exists(logDir)) return;
            try
            {
                var files = Directory.GetFiles(logDir, "*log*")
                    .Where(f => f.Contains("log_") || f.Contains(".gz"))
                    .Select(f => new FileInfo(f))
                    .OrderBy(f => f.CreationTimeUtc)
                    .ToList();

                //超出数量则删除旧文件
                while (files.Count > maxCount)
                {
                    files[0].Delete();
                    files.RemoveAt(0);
                }
            }
            catch (Exception logerr)
            {
                //忽略清理异常，不影响主流程
                Console.WriteLine($"日志清理异常！err：{logerr}");
            }
        }

        private FileMeta GetOrCreateFile(string path)
        {
            if (_fileCache.TryGetValue(path, out var list) && list?.Count > 0) return list[0];
            var stream = new FileStream($"{path}.log", FileMode.Append, FileAccess.Write, FileShare.Read, 4096, true);
            var meta = new FileMeta { Writer = new StreamWriter(stream, _encoding) { AutoFlush = false }, FilePath = $"{path}.log", CurrentSize = stream.Length, LastAccessTime = DateTime.UtcNow };
            _fileCache.AddByKey(path, meta);
            return meta;
        }

        private string GetTimeFolder(DateTime utc) => _logger.Options.File.RollingInterval switch { RollingInterval.Minute => utc.ToString("yyyyMMddHHmm"), RollingInterval.Hour => utc.ToString("yyyyMMddHH"), _ => utc.ToString("yyyyMMdd") };
        public async Task FlushAsync()
        {
            foreach (var m in _fileCache.GetAll(IsDeepCopy: false).OfType<FileMeta>())
                await m.Writer.FlushAsync();
        }
        public Task CloseAsync() { foreach (var m in _fileCache.GetAll().OfType<FileMeta>()) m.Writer.Dispose(); _fileCache.Clear(); return Task.CompletedTask; }
    }
}

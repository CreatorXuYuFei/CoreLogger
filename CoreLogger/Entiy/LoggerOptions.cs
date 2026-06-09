using CoreLogger.Enum;
using Microsoft.Extensions.Logging;

namespace CoreLogger.Entiy
{
    public class LoggerOptions
    {
        public LogLevel GlobalMinLevel { get; set; } = LogLevel.Information;
        public double SampleRate { get; set; } = 1.0;
        public string TextTemplate { get; set; } = "{UtcTimestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level}] [{Module}] {Scope} {Message} {Exception}";

        public TimestampType TimestampType { get; set; } = TimestampType.Beijing;
        public int BatchSize { get; set; } = 50;
        public int ChannelCapacity { get; set; } = 1024 * 1024;
        public int FlushIntervalMs { get; set; } = 1000;
        public int FileExpireMinutes { get; set; } = 5;

        public ConsoleSinkOptions Console { get; set; } = new();
        public FileSinkOptions File { get; set; } = new();
        public LogFilterOptions Filter { get; set; } = new();
        public SensitiveMaskOptions SensitiveMask { get; set; } = new();
    }
}

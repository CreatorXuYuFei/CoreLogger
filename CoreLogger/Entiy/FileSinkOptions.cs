using CoreLogger.Enum;
using Microsoft.Extensions.Logging;

namespace CoreLogger.Entiy
{
    public class FileSinkOptions
    {
        public bool Enabled { get; set; } = true;
        public LogLevel MinLevel { get; set; } = LogLevel.Information;
        public LogFormatType Format { get; set; } = LogFormatType.Text;
        public string RootPath { get; set; } = Path.Combine(AppContext.BaseDirectory, "logs");
        public int MaxFileSizeMB { get; set; } = 10;
        public int MaxBackupCount { get; set; } = 30;
        public RollingInterval RollingInterval { get; set; } = RollingInterval.Day;
        public bool EnableCompress { get; set; } = true;
        //自定义日志文件名
        public string FileName { get; set; } = "log";
    }
}

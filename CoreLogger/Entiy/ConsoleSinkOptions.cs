using CoreLogger.Enum;
using Microsoft.Extensions.Logging;

namespace CoreLogger.Entiy
{
    public class ConsoleSinkOptions
    {
        public bool Enabled { get; set; } = true;
        public LogLevel MinLevel { get; set; } = LogLevel.Information;
        public LogFormatType Format { get; set; } = LogFormatType.Text;
    }
}

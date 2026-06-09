using CoreLogger.Enum;
using LogService;

namespace CoreLogger.Utils
{
    internal static class TimeHelper
    {
        public static DateTime GetTimestamp(TimestampType type)
        {
            return type switch
            {
                TimestampType.UTC => DateTime.UtcNow,
                TimestampType.Local => DateTime.Now,
                TimestampType.Beijing => GetBeijingTime(),
                _ => DateTime.UtcNow
            };
        }

        // 跨平台获取北京时间
        private static DateTime GetBeijingTime()
        {
            var utc = DateTime.UtcNow;
            try
            {
                // Windows
                return TimeZoneInfo.ConvertTimeFromUtc(utc, TimeZoneInfo.FindSystemTimeZoneById("China Standard Time"));
            }
            catch
            {
                // Linux / macOS / Docker
                return TimeZoneInfo.ConvertTimeFromUtc(utc, TimeZoneInfo.FindSystemTimeZoneById("Asia/Shanghai"));
            }
        }
    }
}

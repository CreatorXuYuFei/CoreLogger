using Microsoft.Extensions.Logging;
using System.Collections.Immutable;

namespace CoreLogger.Entiy
{
    internal sealed class LogMessage
    {
        public LogLevel Level { get; init; }
        public string Module { get; init; } = "Default";
        public string Message { get; init; } = string.Empty;
        public Exception? Exception { get; init; }
        public IImmutableDictionary<string, object> Scope { get; init; } = ImmutableDictionary<string, object>.Empty;
        public int ThreadId { get; init; } = Environment.CurrentManagedThreadId;
        public DateTime Time { get; init; } = DateTime.UtcNow; //默认UTC时间
    }
}

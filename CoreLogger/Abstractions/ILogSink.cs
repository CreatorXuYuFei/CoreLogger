using CoreLogger.Entiy;

namespace CoreLogger.Abstractions
{
    internal interface ILogSink
    {
        Task WriteAsync(List<LogMessage> batch);
        Task FlushAsync();
        Task CloseAsync();
    }
}

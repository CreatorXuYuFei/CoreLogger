namespace CoreLogger.Entiy
{
    internal sealed class FileMeta
    {
        public StreamWriter Writer { get; init; } = null!;

        public string FilePath { get; init; } = string.Empty;

        public long CurrentSize { get; set; }

        public DateTime LastAccessTime { get; set; }
    }
}

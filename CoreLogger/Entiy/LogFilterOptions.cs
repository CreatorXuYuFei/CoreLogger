namespace CoreLogger.Entiy
{
    public class LogFilterOptions
    {
        public bool Enabled { get; set; } = true;
        public List<string> IncludeModules { get; set; } = [];
        public List<string> ExcludeModules { get; set; } = [];
        public List<string> BlockKeywords { get; set; } = [];
    }
}

namespace CoreLogger.Entiy
{
    public class SensitiveMaskOptions
    {
        public bool Enabled { get; set; } = true;
        public Dictionary<string, string> Rules { get; set; } = new()
        {
            { @"1[3-9]\d{9}", "1****$&" },
            { @"\d{18}|\d{17}[xX]", "****************$&" }
        };
    }
}

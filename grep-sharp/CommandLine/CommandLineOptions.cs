namespace grep_sharp.CommandLine
{
    public class CommandLineOptions
    {
        public string Pattern { get; set; } = string.Empty;
        public string? FilePath { get; set; }
        public string? Strategy { get; set; }
        public bool CountOnly { get; set; }
        public bool ShowLineNumbers { get; set; }
        public bool Verbose { get; set; }
    }
}

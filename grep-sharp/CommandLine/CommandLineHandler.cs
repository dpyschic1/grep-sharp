namespace grep_sharp.CommandLine
{
    public static class CommandLineHandler
    {
        public static CommandLineOptions Parse(string[] args)
        {
            return CommandLineParser.Parse(args);
        }

        public static void ValidateOptions(CommandLineOptions options)
        {
            if (string.IsNullOrEmpty(options.Pattern))
            {
                throw new ArgumentException("Pattern is required. Use -e PATTERN or provide pattern as first argument.");
            }

            if (!string.IsNullOrEmpty(options.Strategy) &&
                !new[] { "dfa", "nfa", "auto" }.Contains(options.Strategy.ToLowerInvariant()))
            {
                throw new ArgumentException("Strategy must be one of: dfa, nfa, auto");
            }

            if (!string.IsNullOrEmpty(options.FilePath) && !File.Exists(options.FilePath))
            {
                throw new FileNotFoundException($"File not found: {options.FilePath}");
            }
        }
    }
}

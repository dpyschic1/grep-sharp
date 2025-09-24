namespace grep_sharp.CommandLine
{
    public static class CommandLineParser
    {
        public static CommandLineOptions Parse(string[] args)
        {
            var options = new CommandLineOptions();
            var argsList = new List<string>(args);

            for (int i = 0; i < argsList.Count; i++)
            {
                var arg = argsList[i];

                if (arg.StartsWith("-"))
                {
                    switch (arg.ToLowerInvariant())
                    {
                        case "-e":
                        case "--pattern":
                            if (i + 1 < argsList.Count && !argsList[i + 1].StartsWith("-"))
                                options.Pattern = argsList[++i];
                            break;

                        case "-f":
                        case "--file":
                            if (i + 1 < argsList.Count && !argsList[i + 1].StartsWith("-"))
                                options.FilePath = argsList[++i];
                            break;

                        case "-s":
                        case "--strategy":
                            if (i + 1 < argsList.Count && !argsList[i + 1].StartsWith("-"))
                                options.Strategy = argsList[++i];
                            break;

                        case "-c":
                        case "--count":
                            options.CountOnly = true;
                            break;

                        case "-n":
                        case "--line-number":
                            options.ShowLineNumbers = true;
                            break;

                        case "-v":
                        case "--verbose":
                            options.Verbose = true;
                            break;

                        case "-h":
                        case "--help":
                            ShowHelp();
                            Environment.Exit(0);
                            break;

                        default:
                            Console.Error.WriteLine($"Warning: Unknown option '{arg}'");
                            break;
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(options.Pattern))
                    {
                        options.Pattern = arg;
                    }
                    else if (string.IsNullOrEmpty(options.FilePath))
                    {
                        options.FilePath = arg;
                    }
                }
            }

            return options;
        }

        public static void ShowHelp()
        {
            Console.WriteLine(@"
                grep-sharp - A regex matching tool
                
                Usage:
                  grep-sharp [OPTIONS] PATTERN [FILE]
                  grep-sharp [OPTIONS] -e PATTERN -f FILE
                  grep-sharp [OPTIONS] -e PATTERN < FILE
                
                Options:
                  -e, --pattern PATTERN    Regex pattern to match (required)
                  -f, --file FILE          Input file to search in
                  -s, --strategy STRATEGY  Matching strategy: dfa, nfa, or auto (default: auto)
                  -c, --count              Count matching lines/occurrences
                  -n, --line-number        Print line numbers with matching lines
                  -q, --quiet              Suppress normal output
                  -h, --help               Show this help message
                
                Examples:
                  grep-sharp ""hello"" input.txt
                  grep-sharp -e ""[0-9]+"" -f data.txt
                  grep-sharp --strategy dfa -i ""world"" ""Hello World""
                  grep-sharp -c -n ""error"" logfile.txt
                  echo ""hello world"" | grep-sharp -e ""hello""
                ");
        }
    }
}


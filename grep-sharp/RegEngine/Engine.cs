using grep_sharp.CommandLine;
using grep_sharp.Compilation;
using grep_sharp.Compilation.NFAConstruction;
using grep_sharp.Matcher;

namespace grep_sharp.RegEngine
{
    public static class Engine
    {
        public static async Task<int> RunAsync(CommandLineOptions options,
            CancellationToken cancellationToken = default)
        {
            if (IsSimpleLiteral(options.Pattern))
            {
                if(options.Verbose)
                    Console.Error.WriteLine("Strategy: Fast path (literal string matching)");

                if (!string.IsNullOrEmpty(options.FilePath))
                {
                    return await ProcessFileWithFastPath(options, cancellationToken);
                }
                else
                {
                    var input = await Console.In.ReadToEndAsync(cancellationToken);
                    return ProcessTextWithFastPath(input, options);
                }

            }

            var compilationResult = RegexCompiler.Compile(options.Pattern);
            bool useDfa = ChooseStrategy(options.Strategy, compilationResult, options.FilePath);

            if (options.Verbose)
            {
                var explanation = StrategyHeuristic.GetStrategyExplanation(
                compilationResult,
                options.FilePath != null ? FileProcessor.EstimateLineCount(options.FilePath) : 1);

                Console.Error.WriteLine($"Strategy: {explanation}");
            }
            
            if (!string.IsNullOrEmpty(options.FilePath))
            {
                return await ProcessFile(options, compilationResult.CompiledPattern, useDfa, cancellationToken);
            }
            else
            {
                var input = await Console.In.ReadToEndAsync();
                return ProcessText(input, compilationResult.CompiledPattern, useDfa, options);
            }
        }

        private static bool IsSimpleLiteral(string pattern)
        {
            return !pattern.AsSpan().ContainsAny(".*+?[]{}()^$|\\");
        }

        private static bool ChooseStrategy(string? strategyOption, CompilationResult compilation, string? filePath)
        {
            if (!string.IsNullOrEmpty(strategyOption))
            {
                return strategyOption.ToLowerInvariant() switch
                {
                    "dfa" => true,
                    "nfa" => false,
                    _ => StrategyHeuristic.ShouldUseDfa(compilation,
                        filePath != null ? new FileInfo(filePath).Length : null,
                        filePath != null ? FileProcessor.EstimateLineCount(filePath) : null)
                };
            }

            return StrategyHeuristic.ShouldUseDfa(compilation,
                filePath != null ? new FileInfo(filePath).Length : null,
                filePath != null ? FileProcessor.EstimateLineCount(filePath) : null);
        }

        private static async Task<int> ProcessFileWithFastPath(CommandLineOptions options,
            CancellationToken cancellationToken)
        {

            if (!File.Exists(options.FilePath))
            {
                Console.Error.WriteLine($"File not found: {options.FilePath}");
                return 1;
            }

            var result = await FileProcessor.ProcessFileWithFastPathAsync(
                options.FilePath!, options.Pattern, options.ShowLineNumbers, options.CountOnly, cancellationToken);

            if (options.CountOnly)
            {
                Console.WriteLine(result.MatchCount);
            }
            return result.Success ? 0 : 1;
        }

        private static int ProcessTextWithFastPath(string text, CommandLineOptions options)
        {
            bool isMatch = text.Contains(options.Pattern, StringComparison.Ordinal);

            if (options.CountOnly)
            {
                Console.WriteLine(isMatch ? "1" : "0");
            }
            else if (isMatch)
            {
                Console.WriteLine(text);
            }

            return isMatch ? 0 : 1;
        }

        private static async Task<int> ProcessFile(CommandLineOptions options,
            State pattern,
            bool useDfa,
            CancellationToken cancellationToken)
        {
            if (!File.Exists(options.FilePath))
            {
                Console.Error.WriteLine($"File not found: {options.FilePath}");
                return 1;
            }

            var result = await FileProcessor.ProcessFileAsync(
                options.FilePath!, pattern, 
                useDfa, options.ShowLineNumbers, 
                options.CountOnly, cancellationToken);

            if (options.CountOnly)
            {
                Console.WriteLine(result.MatchCount);
            }

            return result.Success ? 0 : 1;
        }

        private static int ProcessText(string text, State pattern, bool useDfa, CommandLineOptions options)
        {
            bool isMatch = useDfa ?
                ReMatch.DFAMatch(text, pattern) :
                ReMatch.NFA2Match(text, pattern);

            if (options.CountOnly)
            {
                Console.WriteLine(isMatch ? "1" : "0");
                return isMatch ? 0 : 1;
            }

            if (isMatch && options.Verbose)
            {
                Console.WriteLine(text);
                return 0;
            }

            return 1;
        }
    }
}
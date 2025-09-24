using grep_sharp.Compilation.NFAConstruction;
using grep_sharp.Matcher;

namespace grep_sharp.RegEngine
{
    public static class FileProcessor
    {
        public static async Task<ProcessResult> ProcessFileAsync(
            string filePath,
            State pattern,
            bool useDfa,
            bool showLineNumbers,
            bool countOnly,
            bool quiet,
            CancellationToken cancellationToken = default)
        {
            var result = new ProcessResult();

            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var reader = new StreamReader(stream);

            string? line;
            int lineNumber = 0;

            while ((line = await reader.ReadLineAsync(cancellationToken)) != null)
            {
                lineNumber++;
                cancellationToken.ThrowIfCancellationRequested();

                bool isMatch = useDfa ?
                    ReMatch.DFAMatch(line, pattern) :
                    ReMatch.NFA2Match(line, pattern);

                if (isMatch)
                {
                    result.MatchCount++;

                    if (!countOnly)
                    {
                        if (showLineNumbers)
                            Console.WriteLine($"{lineNumber}: {line}");
                        else
                            Console.WriteLine(line);
                    }
                }
            }

            return result;
        }

        public static int EstimateLineCount(string filePath)
        {
            try
            {
                var fileInfo = new FileInfo(filePath);
                return (int)(fileInfo.Length / 60);
            }
            catch
            {
                return 0;
            }
        }
    }

    public record ProcessResult
    {
        public int MatchCount { get; set; }
        public bool Success => MatchCount > 0;
    }
}

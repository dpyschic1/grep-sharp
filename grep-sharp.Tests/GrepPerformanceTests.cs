using BenchmarkDotNet.Attributes;
using grep_sharp.CommandLine;
using grep_sharp.RegEngine;
using System.Text;

namespace grep_sharp.Tests
{
    public class GrepPerformanceTests
    {
        private string _smallFile = string.Empty;
        private string _mediumFile = string.Empty;
        private string _largeFile = string.Empty;
        private string _tempDirectory = string.Empty;

        private readonly string[] _testStrategies = { "basic", "optimized", "parallel" };
        private readonly string[] _commonPatterns = {
            "error",
            "function",
            "class",
            "[0-9]+",
            "test.*case",
            "public.*void"
        };

        [GlobalSetup]
        public void Setup()
        {
            _tempDirectory = Path.Combine(Path.GetTempPath(), "grep_sharp_perf_tests");
            Directory.CreateDirectory(_tempDirectory);

            CreateTestFiles();
        }

        public void Cleanup()
        {
            if (Directory.Exists(_tempDirectory))
            {
                Directory.Delete(_tempDirectory, true);
            }
        }

        private void CreateTestFiles()
        {
            _smallFile = Path.Combine(_tempDirectory, "small.txt");
            var smallContent = GenerateTestContent(200, includeCode: true);
            File.WriteAllText(_smallFile, smallContent);

            _mediumFile = Path.Combine(_tempDirectory, "medium.txt");
            var mediumContent = GenerateTestContent(20000, includeCode: true);
            File.WriteAllText(_mediumFile, mediumContent);

            _largeFile = Path.Combine(_tempDirectory, "large.txt");
            var largeContent = GenerateTestContent(200000, includeCode: true);
            File.WriteAllText(_largeFile, largeContent);
        }

        private string GenerateTestContent(int lineCount, bool includeCode = false)
        {
            var random = new Random(42);
            var sb = new StringBuilder();

            var codeSnippets = new[]
            {
                "public class TestClass {",
                "    public void TestMethod() {",
                "        var result = ProcessData();",
                "        if (result != null) {",
                "            Console.WriteLine(\"Success\");",
                "        } else {",
                "            Console.WriteLine(\"Error: Processing failed\");",
                "        }",
                "    }",
                "    private string ProcessData() {",
                "        return \"test data\";",
                "    }",
                "}",
                "// This is a test comment",
                "function calculateSum(a, b) { return a + b; }",
                "SELECT * FROM users WHERE id = 12345;",
                "ERROR: Connection timeout occurred",
                "INFO: Application started successfully",
                "DEBUG: Processing request with ID: 67890"
            };

            var normalText = new[]
            {
                "This is a normal line of text content.",
                "Here we have some sample data for testing purposes.",
                "The quick brown fox jumps over the lazy dog.",
                "Lorem ipsum dolor sit amet, consectetur adipiscing elit.",
                "Performance testing requires realistic data sets.",
                "Regular expressions can be complex and performance-sensitive.",
                "File processing should handle large amounts of data efficiently."
            };

            for (int i = 0; i < lineCount; i++)
            {
                if (includeCode && random.NextDouble() < 0.3) // 30% code content
                {
                    sb.AppendLine(codeSnippets[random.Next(codeSnippets.Length)]);
                }
                else
                {
                    sb.AppendLine($"Line {i + 1}: {normalText[random.Next(normalText.Length)]}");
                }
            }

            return sb.ToString();
        }

        [Benchmark]
        [Arguments("error")]
        public async Task<int> SmallFile_SimplePattern(string pattern)
        {
            var options = new CommandLineOptions
            {
                Pattern = pattern,
                FilePath = _smallFile,
                CountOnly = true
            };

            using var cts = new CancellationTokenSource();
            return await Engine.RunAsync(options, cts.Token);
        }

        [Benchmark]
        [Arguments("error")]
        public async Task<int> MediumFile_SimplePattern(string pattern)
        {
            var options = new CommandLineOptions
            {
                Pattern = pattern,
                FilePath = _mediumFile,
                CountOnly = true
            };

            using var cts = new CancellationTokenSource();
            return await Engine.RunAsync(options, cts.Token);
        }

        [Benchmark]
        [Arguments("error")]
        public async Task<int> LargeFile_SimplePattern(string pattern)
        {
            var options = new CommandLineOptions
            {
                Pattern = pattern,
                FilePath = _largeFile,
                CountOnly = true
            };

            using var cts = new CancellationTokenSource();
            return await Engine.RunAsync(options, cts.Token);
        }

        [Benchmark]
        [Arguments("[0-9]+")]
        public async Task<int> MediumFile_NumberPattern(string pattern)
        {
            var options = new CommandLineOptions
            {
                Pattern = pattern,
                FilePath = _mediumFile,
                CountOnly = true
            };

            using var cts = new CancellationTokenSource();
            return await Engine.RunAsync(options, cts.Token);
        }

        [Benchmark]
        [Arguments("public.*void")]
        public async Task<int> MediumFile_ComplexPattern(string pattern)
        {
            var options = new CommandLineOptions
            {
                Pattern = pattern,
                FilePath = _mediumFile,
                CountOnly = true
            };

            using var cts = new CancellationTokenSource();
            return await Engine.RunAsync(options, cts.Token);
        }

        [Benchmark]
        public async Task<int> MediumFile_CountOnly()
        {
            var options = new CommandLineOptions
            {
                Pattern = "error",
                FilePath = _mediumFile,
                CountOnly = true
            };

            using var cts = new CancellationTokenSource();
            return await Engine.RunAsync(options, cts.Token);
        }

        [Benchmark]
        public async Task<int> MediumFile_WithLineNumbers()
        {
            var options = new CommandLineOptions
            {
                Pattern = "error",
                FilePath = _mediumFile,
                ShowLineNumbers = true
            };

            using var cts = new CancellationTokenSource();
            return await Engine.RunAsync(options, cts.Token);
        }

        [Benchmark]
        public async Task<int> MediumFile_FullOutput()
        {
            var options = new CommandLineOptions
            {
                Pattern = "error",
                FilePath = _mediumFile
            };

            using var cts = new CancellationTokenSource();
            return await Engine.RunAsync(options, cts.Token);
        }

        [Benchmark]
        [Arguments("basic")]
        [Arguments("optimized")]
        public async Task<int> MediumFile_DifferentStrategies(string strategy)
        {
            var options = new CommandLineOptions
            {
                Pattern = "function",
                FilePath = _mediumFile,
                Strategy = strategy,
                CountOnly = true
            };

            using var cts = new CancellationTokenSource();
            return await Engine.RunAsync(options, cts.Token);
        }

        [Benchmark]
        public async Task<int> LargeFile_MemoryPressure()
        {
            var options = new CommandLineOptions
            {
                Pattern = "test.*case",
                FilePath = _largeFile,
                CountOnly = true
            };

            using var cts = new CancellationTokenSource();
            return await Engine.RunAsync(options, cts.Token);
        }

        [Benchmark]
        public async Task<int> LargeFile_EarlyCancellation()
        {
            var options = new CommandLineOptions
            {
                Pattern = "never_found_pattern_xyz",
                FilePath = _largeFile,
                CountOnly = true
            };

            using var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromMilliseconds(100));

            try
            {
                return await Engine.RunAsync(options, cts.Token);
            }
            catch (OperationCanceledException)
            {
                return 130;
            }
        }
    }
}

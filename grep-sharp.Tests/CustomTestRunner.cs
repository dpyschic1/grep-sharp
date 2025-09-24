using grep_sharp.CommandLine;
using grep_sharp.RegEngine;
using System.Diagnostics;

namespace grep_sharp.Tests
{
    public class CustomTestRunner
    {
        private readonly string _tempDir;

        public CustomTestRunner()
        {
            _tempDir = Path.Combine(Path.GetTempPath(), "grep_custom_tests");
            Directory.CreateDirectory(_tempDir);
        }

        public async Task TestPatternComplexity()
        {
            Console.WriteLine("=== Pattern Complexity Analysis ===");

            var testFile = CreateTestFile(10000);
            var patterns = new[]
            {
                ("Simple", "error"),
                ("Character Class", "[a-zA-Z]+"),
                ("Quantifiers", "test.*case"),
                ("Complex", @"\b\w+@\w+\.\w+\b"), // Email-like pattern
                ("Very Complex", @"^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$") // IP pattern
            };

            foreach (var (name, pattern) in patterns)
            {
                var sw = Stopwatch.StartNew();
                var options = new CommandLineOptions
                {
                    Pattern = pattern,
                    FilePath = testFile,
                    CountOnly = true
                };

                using var cts = new CancellationTokenSource();
                var result = await Engine.RunAsync(options, cts.Token);
                sw.Stop();

                Console.WriteLine($"{name,-15}: {sw.ElapsedMilliseconds,6}ms ({result} matches)");
            }
            Console.WriteLine();
        }

        public async Task TestFileScaling()
        {
            Console.WriteLine("=== File Size Scaling Analysis ===");

            var fileSizes = new[] { 1000, 5000, 10000, 50000, 100000 };

            foreach (var size in fileSizes)
            {
                var testFile = CreateTestFile(size);
                var sw = Stopwatch.StartNew();

                var options = new CommandLineOptions
                {
                    Pattern = "function",
                    FilePath = testFile,
                    CountOnly = true
                };

                using var cts = new CancellationTokenSource();
                var result = await Engine.RunAsync(options, cts.Token);
                sw.Stop();

                var fileSizeKB = new FileInfo(testFile).Length / 1024;
                Console.WriteLine($"{size,6} lines ({fileSizeKB,4}KB): {sw.ElapsedMilliseconds,6}ms ({result} matches) - {(double)sw.ElapsedMilliseconds / size:F3}ms/line");
            }
            Console.WriteLine();
        }

        public async Task TestMemoryUsage()
        {
            Console.WriteLine("=== Memory Usage Analysis ===");

            var testFile = CreateTestFile(50000);

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            var beforeMemory = GC.GetTotalMemory(false);

            var options = new CommandLineOptions
            {
                Pattern = "test.*case",
                FilePath = testFile,
                CountOnly = true
            };

            using var cts = new CancellationTokenSource();
            var sw = Stopwatch.StartNew();
            var result = await Engine.RunAsync(options, cts.Token);
            sw.Stop();

            var afterMemory = GC.GetTotalMemory(false);
            var memoryUsed = afterMemory - beforeMemory;

            Console.WriteLine($"Memory used: {memoryUsed / 1024:N0} KB");
            Console.WriteLine($"Time taken: {sw.ElapsedMilliseconds} ms");
            Console.WriteLine($"Matches found: {result}");
            Console.WriteLine();
        }

        public async Task TestConcurrentPerformance()
        {
            Console.WriteLine("=== Concurrent Performance Analysis ===");

            var testFile = CreateTestFile(20000);
            var concurrencyLevels = new[] { 1, 2, 4, 8 };

            foreach (var concurrency in concurrencyLevels)
            {
                var sw = Stopwatch.StartNew();
                var tasks = new List<Task<int>>();

                for (int i = 0; i < concurrency; i++)
                {
                    var options = new CommandLineOptions
                    {
                        Pattern = $"test{i % 3}",
                        FilePath = testFile,
                        CountOnly = true
                    };

                    using var cts = new CancellationTokenSource();
                    tasks.Add(Engine.RunAsync(options, cts.Token));
                }

                var results = await Task.WhenAll(tasks);
                sw.Stop();

                var totalMatches = results.Sum();
                Console.WriteLine($"{concurrency,2} concurrent tasks: {sw.ElapsedMilliseconds,6}ms (total matches: {totalMatches})");
            }
            Console.WriteLine();
        }

        private string CreateTestFile(int lineCount)
        {
            var fileName = Path.Combine(_tempDir, $"test_{lineCount}_{Guid.NewGuid():N}.txt");
            var random = new Random(42);

            var lines = new[]
            {
                "This is a test line with some function calls",
                "Error occurred during processing",
                "function myTestFunction() { return true; }",
                "class TestCase { public void test0() {} }",
                "Normal text line without special patterns",
                "Another test case for pattern matching",
                "Email: user@example.com, Phone: 123-456-7890"
            };

            using var writer = new StreamWriter(fileName);
            for (int i = 0; i < lineCount; i++)
            {
                writer.WriteLine($"Line {i}: {lines[random.Next(lines.Length)]}");
            }

            return fileName;
        }
    }
}

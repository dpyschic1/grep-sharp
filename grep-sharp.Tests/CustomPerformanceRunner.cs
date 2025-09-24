namespace grep_sharp.Tests
{
    public static class CustomPerformanceRunner
    {
        public static async Task RunCustomTests()
        {
            var testRunner = new CustomTestRunner();

            Console.WriteLine("Running custom performance analysis...\n");

            await testRunner.TestPatternComplexity();

            await testRunner.TestFileScaling();

            await testRunner.TestMemoryUsage();

            await testRunner.TestConcurrentPerformance();
        }
    }
}

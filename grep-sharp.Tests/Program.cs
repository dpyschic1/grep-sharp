using BenchmarkDotNet.Running;
using grep_sharp.Tests;

public class Program
{
    public static void Main(string[] args)
    {
        BenchmarkRunner.Run<GrepPerformanceTests>();
    }
}

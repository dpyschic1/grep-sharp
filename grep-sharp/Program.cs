using grep_sharp.CommandLine;
using grep_sharp.Core;

namespace grep_sharp;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        try
        {
            if (args.Length == 0 || args[0] == "-h" || args[0] == "--help")
            {
                CommandLineParser.ShowHelp();
                return 0;
            }

            var options = CommandLineParser.Parse(args);

            CommandLineHandler.ValidateOptions(options);

            using var cts = new CancellationTokenSource();
            SetupCancelHandler(cts);

            return await Engine.RunAsync(options, cts.Token);
        }
        catch (ArgumentException ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            Console.Error.WriteLine("Use -h for help.");
            return 1;
        }
        catch (FileNotFoundException ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            return 1;
        }
        catch (OperationCanceledException)
        {
            Console.Error.WriteLine("Operation cancelled.");
            return 130;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Unexpected error: {ex.Message}");
            return 1;
        }
    }
    private static void SetupCancelHandler(CancellationTokenSource cts)
    {
        Console.CancelKeyPress += (s, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
        };
    }
}
   

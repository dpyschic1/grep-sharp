using grep_sharp.Parser;

var options = new Dictionary<string, string>();
string input = string.Empty;
for(int i = 0; i < args.Length; i++)
{
    if (args[i].StartsWith("-"))
    {
        var option = args[i].TrimStart('-');
        var value = (i + 1 < args.Length && !args[i + 1].StartsWith("-")) ? args[++i] : "true";
        options[option] = value.ToUpper();
    }
    else
    {
        input = args[i];
    }
}

if (!options.TryGetValue("E", out var pattern))
{
    Console.WriteLine("Pattern string not provided");
}

var tokens = Tokenizer.Tokenize(pattern);
foreach(var token in tokens)
{
    Console.WriteLine($"{token}");
}

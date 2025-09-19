using grep_sharp.Matcher;
using grep_sharp.Parser;

var options = new Dictionary<string, string>();
string input = string.Empty;
for(int i = 0; i < args.Length; i++)
{
    if (args[i].StartsWith("-"))
    {
        var option = args[i].TrimStart('-');
        var value = (i + 1 < args.Length && !args[i + 1].StartsWith("-")) ? args[++i] : "true";
        options[option.ToUpper()] = value;
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
var exapndedTokens = Tokenizer.ExpandQuantifierTokens(tokens);


Console.WriteLine("Original Tokens:");
foreach (var token in tokens)
{
    Console.Write($"{{ {token} }}\t");
}

Console.Write("\n\nExpanded Tokens:");
foreach(var token in exapndedTokens)
{
    Console.Write($"{{ {token} }}\t");
}


var rpnOut = ReRPN.InfixToPostfix(tokens);

Console.WriteLine("Original Postfix: {0}", rpnOut);

var rpnOutExp = ReRPN.InfixToPostfix(exapndedTokens);

Console.WriteLine("Expanded Postfix: {0}", rpnOutExp);

var stateOut = NFABuilder.Post2NFA(rpnOutExp);
void Walk(State s, HashSet<State> visited)
{
    if (s == null || visited.Contains(s)) return;
    visited.Add(s);
    Console.Write($"{s.Type} {(s.Type == StateType.Char ? s.Character.ToString() : "")}-->");
    Walk(s.Out1, visited);
    Walk(s.Out2, visited);
}

Console.WriteLine("WalkStarted:");


Walk(stateOut, new HashSet<State>());

Console.WriteLine("Walk Ended");
Console.WriteLine(GraphvizVisualizer.GenerateDot(stateOut));
// TODO: Build Execution engine for matching inputs.

Console.WriteLine(rpnOut);

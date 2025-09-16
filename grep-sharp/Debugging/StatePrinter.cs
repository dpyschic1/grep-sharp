using grep_sharp.Matcher;
using System.Text;

public static class GraphvizVisualizer
{
    public static string GenerateDot(State start)
    {
        var visited = new HashSet<State>();
        var sb = new StringBuilder();

        sb.AppendLine("digraph NFA {");
        sb.AppendLine("  rankdir=LR;");
        sb.AppendLine("  node [shape=circle];");

        // Mark start and accept states
        sb.AppendLine($"  {GetStateId(start)} [style=bold, color=green];");

        GenerateDotNodes(start, visited, sb);

        visited.Clear();
        GenerateDotEdges(start, visited, sb);

        sb.AppendLine("}");
        return sb.ToString();
    }

    private static void GenerateDotNodes(State s, HashSet<State> visited, StringBuilder sb)
    {
        if (s == null || visited.Contains(s)) return;
        visited.Add(s);

        string label = s.Type switch
        {
            StateType.Char => s.Character.ToString(),
            StateType.Split => "ε",
            StateType.Match => "MATCH",
            _ => "?"
        };

        string color = s.Type == StateType.Match ? "red" : "black";
        sb.AppendLine($"  {GetStateId(s)} [label=\"{label}\", color={color}];");

        GenerateDotNodes(s.Out1, visited, sb);
        GenerateDotNodes(s.Out2, visited, sb);
    }

    private static void GenerateDotEdges(State s, HashSet<State> visited, StringBuilder sb)
    {
        if (s == null || visited.Contains(s)) return;
        visited.Add(s);

        if (s.Out1 != null)
            sb.AppendLine($"  {GetStateId(s)} -> {GetStateId(s.Out1)};");
        if (s.Out2 != null)
            sb.AppendLine($"  {GetStateId(s)} -> {GetStateId(s.Out2)};");

        GenerateDotEdges(s.Out1, visited, sb);
        GenerateDotEdges(s.Out2, visited, sb);
    }

    private static string GetStateId(State s) => $"s{s.GetHashCode():X}";
}
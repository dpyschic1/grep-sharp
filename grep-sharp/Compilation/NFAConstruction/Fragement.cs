namespace grep_sharp.Compilation.NFAConstruction
{
    public class Fragement
    {
        public required State Start;
        public List<Action<State>> DanglingAction = new();
    }
}

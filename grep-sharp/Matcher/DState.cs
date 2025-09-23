using grep_sharp.Compilation.NFAConstruction;

namespace grep_sharp.Matcher
{
    public class DState
    {
        public List<State> NFAStates { get; }
        public DState?[] Next { get; }
        public bool IsMatch { get; }

        public DState(List<State> states)
        {
            NFAStates = states;
            Next = new DState[256];
            IsMatch = NFAStates.Any(s => s.Type == StateType.Match);
        }
    }
}

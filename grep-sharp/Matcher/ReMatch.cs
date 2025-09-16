namespace grep_sharp.Matcher
{
    internal class ReMatch
    {
        private int currentGeneration = 0;
        public State Post2NFA(string postFixPattern)
        {
            var fragStack = new Stack<Frag>();
            for (int i = 0; i < postFixPattern.Length; i++)
            {
                char c = postFixPattern[i];
                currentGeneration++;
                switch (c)
                {
                    case '|':
                        fragStack.Push(BuildAlternation(fragStack.Pop(), fragStack.Pop()));
                        break;
                    case '*':
                        fragStack.Push(BuildStar(fragStack.Pop()));
                        break;
                    case '.':
                        fragStack.Push(BuildConcatenation(fragStack.Pop(), fragStack.Pop()));
                        break;
                    case '+':
                        fragStack.Push(BuildPlus(fragStack.Pop()));
                        break;
                    case '?':
                        fragStack.Push(BuildQuestion(fragStack.Pop()));
                        break;
                    default:
                        fragStack.Push(BuildLiteral(c));
                        break;
                    
                }

            }
            var finalFrag = fragStack.Pop();
            var matchState = new State() { Type = StateType.Match };

            foreach (var action in finalFrag.DanglingAction) action(matchState);

            return finalFrag.start;

        }

        private Frag BuildAlternation(Frag right, Frag left)
        {
            var splitState = new State { Type = StateType.Split, Out1 = left.start, Out2 = right.start };
            return new Frag
            {
                start = splitState,
                DanglingAction = left.DanglingAction.Concat(right.DanglingAction).ToList()
            };
        }

        private Frag BuildStar(Frag frag)
        {
            var splitState = new State { Type = StateType.Split, Out1 = frag.start, Out2 = null };
            foreach (var action in frag.DanglingAction) action(splitState);
            return new Frag
            {
                start = splitState,
                DanglingAction = { target => splitState.Out2 = target }
            };
        }

        private Frag BuildConcatenation(Frag right, Frag left)
        {
            foreach (var action in left.DanglingAction) action(right.start);

            return new Frag
            {
                start = left.start,
                DanglingAction = right.DanglingAction
            };
        }

        private Frag BuildLiteral(char c)
        {
            var state = new State {Type = StateType.Char , Character = c };
            return new Frag
            {
                start = state,
                DanglingAction = { target => state.Out1 = target }
            };
        }

        private Frag BuildPlus(Frag frag)
        {
            var splitState = new State { Type = StateType.Split, Out1 = frag.start, Out2 = null };
            foreach (var action in frag.DanglingAction) action(splitState);

            return new Frag
            {
                start = frag.start,
                DanglingAction = { target => splitState.Out2 = target }
            };
        }

        private Frag BuildQuestion(Frag frag)
        {
            var splitState = new State { Type = StateType.Split, Out1 = frag.start, Out2 = null };
            var fragementDangling = frag.DanglingAction.ToList();
            fragementDangling.Add(target => splitState.Out2 = target);
            return new Frag
            {
                start = splitState,
                DanglingAction = fragementDangling
            };
        }
    }

    public class State
    {
        public StateType Type;
        public char Character;
        public State Out1;
        public State Out2;
    }

    public class Frag
    {
        public State start;
        public List<Action<State>> DanglingAction = new();
    }
    public enum StateType { Char, Split, Match };
}

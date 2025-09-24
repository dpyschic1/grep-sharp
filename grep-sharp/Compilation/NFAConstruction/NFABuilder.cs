using static grep_sharp.Constants;
namespace grep_sharp.Compilation.NFAConstruction
{
    public static class NFABuilder
    {
        public static State Build(string postFixPattern)
        {
            var fragStack = new Stack<Fragement>();
            for (int i = 0; i < postFixPattern.Length; i++)
            {
                char c = postFixPattern[i];
                switch (c)
                {
                    case '|':
                        fragStack.Push(BuildAlternation(fragStack.Pop(), fragStack.Pop()));
                        break;
                    case '*':
                        fragStack.Push(BuildStar(fragStack.Pop()));
                        break;
                    case CONCAT:
                        fragStack.Push(BuildConcatenation(fragStack.Pop(), fragStack.Pop()));
                        break;
                    case ANCHORSTART:
                        fragStack.Push(BuildAnchor(StateType.AnchorStart));
                        break;
                    case ANCHOREND:
                        fragStack.Push(BuildAnchor(StateType.AnchorEnd));
                        break;
                    case '+':
                        fragStack.Push(BuildPlus(fragStack.Pop()));
                        break;
                    case '?':
                        fragStack.Push(BuildQuestion(fragStack.Pop()));
                        break;
                    case '[':
                        int end = FindClosingBracket(postFixPattern, i);
                        var charSet = ParseCharacterClass(postFixPattern, i + 1, end - 1);
                        fragStack.Push(BuildCharacterSet(charSet));
                        i = end;
                        break;
                    default:
                        fragStack.Push(BuildLiteral(c));
                        break;
                }
            }
            var finalFrag = fragStack.Pop();
            var matchState = new State() { Type = StateType.Match };

            foreach (var action in finalFrag.DanglingAction) action(matchState);

            return finalFrag.Start;

        }

        private static Fragement BuildAlternation(Fragement right, Fragement left)
        {
            var splitState = new State { Type = StateType.Split, Out1 = left.Start, Out2 = right.Start };
            return new Fragement
            {
                Start = splitState,
                DanglingAction = left.DanglingAction.Concat(right.DanglingAction).ToList()
            };
        }

        private static Fragement BuildStar(Fragement frag)
        {
            var splitState = new State { Type = StateType.Split, Out1 = frag.Start, Out2 = null };
            foreach (var action in frag.DanglingAction) action(splitState);
            return new Fragement
            {
                Start = splitState,
                DanglingAction = { target => splitState.Out2 = target }
            };
        }

        private static Fragement BuildConcatenation(Fragement right, Fragement left)
        {
            foreach (var action in left.DanglingAction) action(right.Start);

            return new Fragement
            {
                Start = left.Start,
                DanglingAction = right.DanglingAction
            };
        }

        private static Fragement BuildLiteral(char c)
        {
            var state = new State {Type = StateType.Char , Character = c };
            return new Fragement
            {
                Start = state,
                DanglingAction = { target => state.Out1 = target }
            };
        }

        private static Fragement BuildPlus(Fragement frag)
        {
            var splitState = new State { Type = StateType.Split, Out1 = frag.Start, Out2 = null };
            foreach (var action in frag.DanglingAction) action(splitState);

            return new Fragement
            {
                Start = frag.Start,
                DanglingAction = { target => splitState.Out2 = target }
            };
        }

        private static Fragement BuildQuestion(Fragement frag)
        {
            var splitState = new State { Type = StateType.Split, Out1 = frag.Start, Out2 = null };
            var fragementDangling = frag.DanglingAction.ToList();
            fragementDangling.Add(target => splitState.Out2 = target);
            return new Fragement
            {
                Start = splitState,
                DanglingAction = fragementDangling
            };
        }

        private static Fragement BuildCharacterSet(CharacterSet cSet)
        {
            var cClassState = new State { Type = StateType.CharSet, CharacterSet = cSet};
            return new Fragement
            {
                Start = cClassState,
                DanglingAction = { target => cClassState.Out1 = target }
            };
        }

        private static Fragement BuildAnchor(StateType type)
        {
            var anchorState = new State { Type = type };
            return new Fragement
            {
                Start = anchorState,
                DanglingAction = { target => anchorState.Out1 = target }
            };
        }

        private static CharacterSet ParseCharacterClass(string cClass, int start, int end)
        {
            var charSet = new CharacterSet();
            int i = start;

            if (end > i && cClass[i] == '^')
            {
                charSet.IsNegated = true;
                i++;
            }

            while (i <= end)
            {
                if (i + 2 < end && cClass[i + 1] == '-')
                {
                    if (cClass[i] < cClass[i + 2])
                    {
                        charSet.AddRange(cClass[i], cClass[i + 2]);
                        i += 3;
                    }
                }
                else charSet.Add(cClass[i++]);
            }

            return charSet;
        }

        private static int FindClosingBracket(string pattern, int start)
        {
            int i = start + 1;
            while (i < pattern.Length && pattern[i] != ']') i++;
            return i;
        }
    }
}

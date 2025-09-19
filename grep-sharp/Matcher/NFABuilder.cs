using System.Collections;
using static grep_sharp.Constants;
namespace grep_sharp.Matcher
{
    public static class NFABuilder
    {
        //TODO: Anchors
        public static State Post2NFA(string postFixPattern)
        {
            var fragStack = new Stack<Frag>();
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
                    case '+':
                        fragStack.Push(BuildPlus(fragStack.Pop()));
                        break;
                    case '?':
                        fragStack.Push(BuildQuestion(fragStack.Pop()));
                        break;
                    case '[':
                        int end = postFixPattern.IndexOf(']', i);
                        var charSet = ParseCharacterClass(postFixPattern.Substring(i + 1, end - i - 1));
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

            return finalFrag.start;

        }

        private static Frag BuildAlternation(Frag right, Frag left)
        {
            var splitState = new State { Type = StateType.Split, Out1 = left.start, Out2 = right.start };
            return new Frag
            {
                start = splitState,
                DanglingAction = left.DanglingAction.Concat(right.DanglingAction).ToList()
            };
        }

        private static Frag BuildStar(Frag frag)
        {
            var splitState = new State { Type = StateType.Split, Out1 = frag.start, Out2 = null };
            foreach (var action in frag.DanglingAction) action(splitState);
            return new Frag
            {
                start = splitState,
                DanglingAction = { target => splitState.Out2 = target }
            };
        }

        private static Frag BuildConcatenation(Frag right, Frag left)
        {
            foreach (var action in left.DanglingAction) action(right.start);

            return new Frag
            {
                start = left.start,
                DanglingAction = right.DanglingAction
            };
        }

        private static Frag BuildLiteral(char c)
        {
            var state = new State {Type = StateType.Char , Character = c };
            return new Frag
            {
                start = state,
                DanglingAction = { target => state.Out1 = target }
            };
        }

        private static Frag BuildPlus(Frag frag)
        {
            var splitState = new State { Type = StateType.Split, Out1 = frag.start, Out2 = null };
            foreach (var action in frag.DanglingAction) action(splitState);

            return new Frag
            {
                start = frag.start,
                DanglingAction = { target => splitState.Out2 = target }
            };
        }

        private static Frag BuildQuestion(Frag frag)
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

        private static Frag BuildCharacterSet(CharacterSet cSet)
        {
            var cClassState = new State { Type = StateType.CharSet, CharacterSet = cSet};
            return new Frag
            {
                start = cClassState,
                DanglingAction = { target => cClassState.Out1 = target }
            };
        }

        private static CharacterSet ParseCharacterClass(string cClass)
        {
            var charSet = new CharacterSet();
            for (int i = 0; i < cClass.Length; i++)
            {
                if (cClass[0] == '^')
                {
                    charSet.IsNegated = true; 
                    continue;
                }

                if (i + 2 < cClass.Length && cClass[i + 1] == '-')
                {
                    if (cClass[i] < cClass[i + 2])
                    {
                        charSet.AddRange(cClass[i], cClass[i + 2]);
                        i += 2;
                    }
                }
                else charSet.Add(cClass[i]);
            }

            return charSet;
        }
    }

    public class Frag
    {
        public State start;
        public List<Action<State>> DanglingAction = new();
    }
}

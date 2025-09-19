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
                    case '{':
                        int closeBrace = postFixPattern.IndexOf('}', i);
                        string quantStr = postFixPattern.Substring(i, closeBrace - i + 1);
                        fragStack.Push(BuildQuantifier(fragStack.Pop(), quantStr));
                        i = closeBrace;
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

        private static Frag BuildQuantifier(Frag frag, string quantifier)
        {
            var content = quantifier.Substring(1, quantifier.Length - 2);
            var parts = content.Split(',');

            int min = int.Parse(parts[0]);
            int max = parts.Length > 1 && !string.IsNullOrEmpty(parts[1]) ? int.Parse(parts[1]) : -1;

            Frag result = null;

            for(int i = 0; i < min; i++)
            {
                result = result == null ? frag : BuildConcatenation(frag, result);
            }

            if(max == -1)
            {
                var star = BuildStar(frag);
                result = result == null ? star : BuildConcatenation(star, result);
            }
            else
            {
                for (int i = min; i < max; i++)
                {
                    var optional = BuildQuestion(frag);
                    result = result == null ? optional : BuildConcatenation(optional, result);
                }
            }
            return result;
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
                else
                {
                    charSet.Add(cClass[i]);
                }
            }

            return charSet;
        }
    }

    public class State
    {
        public StateType Type;
        public char Character;
        public CharacterSet CharacterSet;
        public State Out1;
        public State Out2;
        public int MinCount;
        public int MaxCount;
    }

    public class Frag
    {
        public State start;
        public List<Action<State>> DanglingAction = new();
    }

    public class CharacterSet
    {
        private readonly BitArray bits = new(128);
        public bool IsNegated { get; set; }
        public void Add(char c) => bits[c] = true;
        public void AddRange(char start, char end)
        {
            for (char c = start; c <= end; c++)
                bits[c] = true;
        }

        public bool Contains(char c)
        {
            if (c >= 128) return IsNegated;

            bool inSet = bits[c];
            return IsNegated ? !inSet : inSet;
        }
    }

    public enum StateType { Char, Split, Match, CharSet, Quantifier };
}

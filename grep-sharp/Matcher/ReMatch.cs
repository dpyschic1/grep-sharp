using grep_sharp.Compilation.NFAConstruction;
using System.Collections.Generic;

namespace grep_sharp.Matcher
{
    public static class ReMatch
    {
        private static int listId = 0;

        public static bool DFAMatch(string input, State NFA)
        {
            var startList = StartList(NFA);
            var current = DStateCache.GetOrCreate(startList);

            foreach( char c in input )
            {
                current = GetNextState(current, c);
                if(current == null) return false;
            }

            return current.IsMatch;
        }

        public static bool NFA2Match(string input, State NFA)
        {
            listId = 0;
            var clist = StartList(NFA);
            var nlist = new List<State>();

            foreach (char c in input)
            {
                Step(clist, nlist, c);
                (nlist, clist) = (clist, nlist);
            }

            foreach(var state in clist)
            {
                Console.WriteLine($"State type: {state.Type}");
                if (state.Type == StateType.Match) return true;
            }

            return false;
        }

        private static DState? GetNextState(DState current, char c)
        {
            if (current.Next[c] != null) return current.Next[c];

            var nextList = new List<State>();
            Step(current.NFAStates, nextList, c);

            if(nextList.Count == 0)
            {
                current.Next[c] = null;
                return null;
            }

            var nextDState = DStateCache.GetOrCreate(nextList);

            current.Next[c] = nextDState;
            return nextDState;
        }

        private static void Step(List<State> clist, List<State> nlist, char c)
        {
            listId++;
            foreach(var state in clist)
            {
                switch (state.Type)
                {
                    case StateType.Char:
                        if (state.Character == c) AddState(nlist, state.Out1);
                        break;

                    case StateType.CharSet:
                        if (state.CharacterSet.Contains(c)) AddState(nlist, state.Out1);
                        break;
                    default: break;
                }
            }
        }

        private static List<State> StartList(State state)
        {
            listId++;
            var stList = new List<State>();
            AddState(stList, state);
            return stList;
        }

        private static void AddState(List<State> list, State state)
        {
            if (list == null || state.LastList == listId) return;

            state.LastList = listId;
            if(state.Type == StateType.Split)
            {
                AddState(list, state.Out1);
                AddState(list, state.Out2);
                return;
            }
            list.Add(state);
        }

    }
}

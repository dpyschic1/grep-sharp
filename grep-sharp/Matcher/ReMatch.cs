using grep_sharp.Compilation.NFAConstruction;

namespace grep_sharp.Matcher
{
    public static class ReMatch
    {
        private static readonly Dictionary<State, int> _visited = [];
        private static int _currentGeneration = 0;
        private static readonly List<State> _list1 = [];
        private static readonly List<State> _list2 = [];

        public static bool DFAMatch(string input, State NFA)
        {
            Initialize();
            var startList = new List<State>();
            StartList(startList, NFA);

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
            Initialize();
            var clist = _list1;
            var nlist = _list2;

            StartList(clist, NFA);

            foreach (char c in input)
            {
                Step(clist, nlist, c);
                (nlist, clist) = (clist, nlist);
                nlist.Clear();
            }

            foreach(var state in clist)
            {
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
            _currentGeneration++;
            foreach (var state in clist)
            {
                switch (state.Type)
                {
                    case StateType.Char:
                        if (state.Character == c) AddState(nlist, state.Out1); 
                        break;
                    case StateType.CharSet:
                        if (state.CharacterSet.Contains(c)) AddState(nlist, state.Out1);
                        break;
                    default:
                        break;
                }
            }
        }

        private static void StartList(List<State> list, State state)
        {
            _currentGeneration++;
            AddState(list, state);
        }

        private static void AddState(List<State> list, State state)
        {
            if (state == null) return;

            if (_visited.TryGetValue(state, out int lastGen) && lastGen == _currentGeneration)
                return;

            _visited[state] = _currentGeneration;

            if(state.Type == StateType.Split)
            {
                AddState(list, state.Out1);
                AddState(list, state.Out2);
                return;
            }

            list.Add(state);
        }

        private static void Initialize()
        {
            _visited.Clear();
            _currentGeneration = 0;
            _list1.Clear();
            _list2.Clear();
        }

    }
}

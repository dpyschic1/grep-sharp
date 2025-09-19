using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace grep_sharp.Matcher
{
    public static class ReMatch
    {
        private static int listId = 0;
        public static bool NFA2Match(string input, State NFA)
        {
            var l1 = new StateList(1024);
            var l2 = new StateList(1024);
            var clist = StartList(NFA, l1);
            var nlist = l2;

            foreach (char c in input)
            {
                Step(clist, nlist, c);
                var tmp = clist;
                clist = nlist;
                nlist= tmp;
            }

            for(int i = 0; i< clist.Count; i++)
            {
                if (clist.States[i].Type == StateType.Match) return true;
            }

            return false;
        }

        private static void Step(StateList clist, StateList nlist, char c)
        {
            listId++;
            nlist.Count = 0;
            for (int i = 0; i < clist.Count; i++)
            {
                var s = clist.States[i];
                switch (s.Type)
                {
                    case StateType.Char:
                        if (s.Character == c) AddState(nlist, s.Out1); break;

                    case StateType.CharSet:
                        if (s.CharacterSet.Contains(c)) AddState(nlist, s.Out1);
                        break;
                    default: break;
                }
            }
        }

        private static StateList StartList(State state, StateList list)
        {
            listId++;
            list.Count = 0;
            AddState(list, state);
            return list;
        }

        private static void AddState(StateList list, State state)
        {
            if (list == null || state.LastList == listId) return;

            state.LastList = listId;
            if(state.Type == StateType.Split)
            {
                AddState(list, state.Out1);
                AddState(list, state.Out2);
                return;
            }

            list.States[list.Count++] = state;
        }
    }

    public class StateList
    {
        public State[] States;
        public int Count;

        public StateList(int capacity)
        {
            States = new State[capacity];
            Count = 0;
        }
    }
}

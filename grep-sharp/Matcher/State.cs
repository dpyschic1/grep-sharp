using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace grep_sharp.Matcher
{
    public class State
    {
        public StateType Type;
        public char Character;
        public CharacterSet CharacterSet;
        public State Out1;
        public State Out2;
        public int LastList;
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

    public enum StateType { Char, Split, Match, CharSet };
}

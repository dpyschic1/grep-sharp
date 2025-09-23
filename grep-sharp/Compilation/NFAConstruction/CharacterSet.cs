using System.Collections;

namespace grep_sharp.Compilation.NFAConstruction
{
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
}

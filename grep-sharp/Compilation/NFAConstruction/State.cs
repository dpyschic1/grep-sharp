namespace grep_sharp.Compilation.NFAConstruction
{
    public class State
    {
        public StateType Type;
        public char Character;
        public CharacterSet? CharacterSet;
        public State? Out1;
        public State? Out2;
    }
}

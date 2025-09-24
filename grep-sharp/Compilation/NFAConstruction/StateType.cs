namespace grep_sharp.Compilation.NFAConstruction
{
    public enum StateType 
    { 
        Char, 
        Split, 
        Match, 
        CharSet,
        AnchorStart,
        AnchorEnd
    };
}

namespace grep_sharp.Compilation.Tokenization
{
    public enum TokenType
    {
        Literal,        // a single character or escaped literal
        CharClass,      // [abc], [a-z], \d, etc.
        Operator,       // *, +, ?
        GroupOpen,      // (
        GroupClose,     // )
        AnchorStart,    // ^
        AnchorEnd,      // $
        Quantifier,     // {,}
        Alternation,    // |
        WildCard,       // .
        End
    }
}

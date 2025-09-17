namespace grep_sharp.Parser
{
    internal static class Tokenizer
    {
        //TODO handle anchors explicitly
        public static List<Token> Tokenize(string pattern)
        {
            int bStart = 0;
            var tokens = new List<Token>();
            var chars = pattern.AsSpan();
            for (int i = 0; i < pattern.Length; i++)
            {
                char c = chars[i];
                switch (c)
                {
                    case '(':
                        tokens.Add(new Token(TokenType.GroupOpen));
                        break;

                    case ')':
                        tokens.Add(new Token(TokenType.GroupClose));
                        break;

                    case '.':
                        tokens.Add(new Token(TokenType.WildCard));
                        break;

                    case '*':
                    case '+':
                    case '?':
                        tokens.Add(new Token(TokenType.Operator, c.ToString()));
                        break;

                    case '|':
                        tokens.Add(new Token(TokenType.Alternation, c.ToString()));
                        break;

                    case '[':
                        bStart = i;
                        while (i < chars.Length && chars[i] != ']') i++;
                        tokens.Add(new Token(TokenType.CharClass, chars.Slice(bStart, i - bStart + 1).ToString()));
                        break;

                    case '{':
                        bStart = i;
                        while(i < chars.Length && chars[i] != '}')
                        {
                            if (chars[i] == ',')
                            {
                                i++;
                                continue;
                            }
                            if (chars[i] <= '0' && chars[i] >= '9') throw new ArgumentException("Invalid digit");
                            i++;
                        }
                        tokens.Add(new Token(TokenType.Quantifier, chars.Slice(bStart, i - bStart + 1).ToString()));
                        break;

                    case '\\':
                        i++;
                        string val;
                        switch (chars[i])
                        {
                            case 'd': val = "[0-9]"; break;
                            case 'D': val = "[^0-9]"; break;
                            case 'w': val = "[A-Za-z0-9_]"; break;
                            case 'W': val = "[^A-Za-z0-9_]"; break;
                            case 's': val = "[ \t\r\n\f]"; break;
                            case 'S': val = "[^ \t\r\n\f]"; break;
                            default: val = chars[i].ToString(); break;
                        }
                        tokens.Add(new Token(val.Length == 1 ? TokenType.Literal : TokenType.CharClass, val));
                        break;

                    default:
                        tokens.Add(new Token(TokenType.Literal, chars.Slice(i, 1).ToString()));
                        break;

                }
            }

            tokens.Add(new Token(TokenType.End));
            return tokens;
        }

    }

    public readonly struct Token
    {
        public TokenType Type { get; }
        public string Value { get; }

        public Token(TokenType type, string value = "")
        {
            Type = type;
            Value = value;
        }

        public override string ToString() => $"{Type}: {Value}";
    }

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
        WildCard,
        End
    } 

}

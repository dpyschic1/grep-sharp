namespace grep_sharp.Compilation.Tokenization
{
    public static class Tokenizer
    {
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
                    case '^':
                        tokens.Add(new Token(TokenType.AnchorStart));
                        break;

                    case '$':
                        tokens.Add(new Token(TokenType.AnchorEnd));
                        break;

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
                        i++;
                        while(i < chars.Length && chars[i] != '}')
                        {
                            if (chars[i] == ',')
                            {
                                i++;
                                continue;
                            }
                            if (chars[i] < '0' || chars[i] > '9') throw new ArgumentException("Invalid digit");
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

        public static List<Token> ExpandQuantifierTokens(List<Token> tokens)
        {
            var result = new List<Token>();
            var tokenList = tokens.ToList();

            for (int i = 0; i < tokenList.Count; i++)
            {
                var token = tokenList[i];

                if (token.Type != TokenType.Quantifier) { result.Add(token); continue; }

                if (result.Count == 0) throw new ArgumentException("Quantifier must follow a token");

                int atomStart = result.Count - 1;
                if (result[atomStart].Type == TokenType.GroupClose)
                {
                    int depth = 1;
                    atomStart--;
                    while (atomStart >= 0 && depth > 0)
                    {
                        if (result[atomStart].Type == TokenType.GroupClose) depth++;
                        else if (result[atomStart].Type == TokenType.GroupOpen) depth--;
                        atomStart--;
                    }
                    atomStart++;
                }

                var atom = result.Skip(atomStart).ToList();

                var content = token.Value[1..^1];
                var parts = content.Split(',');
                int min = int.Parse(parts[0]);
                int max = parts.Length > 1 && !string.IsNullOrEmpty(parts[1]) ? int.Parse(parts[1]) : -1;

                var expanded = new List<Token>();

                for (int j = 0; j < min; j++)
                    expanded.AddRange(atom);

                if(max == -1)
                {
                    expanded.AddRange(atom);
                    expanded.Add(new Token(TokenType.Operator, "*"));
                }
                else
                {
                    int optionalCount = max - min;
                    for(int j = 0;j < optionalCount; j++)
                    {
                        expanded.AddRange(atom);
                        expanded.Add(new Token(TokenType.Operator, "?"));
                    }
                }

                result.RemoveRange(atomStart, result.Count - atomStart);
                result.AddRange(expanded);
            }

            return result;
        }

    }
}

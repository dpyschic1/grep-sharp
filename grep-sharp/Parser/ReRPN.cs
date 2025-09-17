using System.Text;

namespace grep_sharp.Parser
{
    public static class ReRPN
    {
        public const char CONCAT = (char)0xFFFF;
        private const char WILDCARD = (char)0xFFFE;
        public static string InfixToPostfix(List<Token> tokens)
        {
            var outBuff = new StringBuilder();
            var parenTrack = new Stack<(int nalt, int natom)>();
            int nalt = 0;
            int natom = 0;

            foreach (var token in tokens)
            {
                switch (token.Type)
                {
                    case TokenType.CharClass:
                    case TokenType.Literal:
                        if(natom > 1)
                        {
                            natom--;
                            outBuff.Append(CONCAT);
                        }

                        outBuff.Append(token.Value);
                        natom++;
                        break;

                    case TokenType.Operator:
                        if (natom == 0)
                            throw new ArgumentException("No operands to perform the operation on");
                        outBuff.Append(token.Value);
                        break;

                    case TokenType.WildCard:
                        if(natom > 1)
                        {
                            natom--;
                            outBuff.Append(CONCAT);
                        }
                        outBuff.Append(WILDCARD);
                        break;

                    case TokenType.GroupOpen:
                        if (natom > 1)
                        {
                            natom--;
                            outBuff.Append(CONCAT);
                        }
                        parenTrack.Push((nalt, natom));
                        nalt = natom = 0;
                        break;

                    case TokenType.GroupClose:
                        if (natom == 0)
                            throw new ArgumentException("Group is empty");

                        while (--natom > 0) outBuff.Append(CONCAT);
                        for (; nalt > 0; nalt--) outBuff.Append('|');

                        (nalt, natom) = parenTrack.Pop();
                        natom++;
                        break;

                    case TokenType.Quantifier:
                        if (natom == 0)
                            throw new ArgumentException("Quantifiers not attached with anything");
                        outBuff.Append(token.Value);
                        break;

                    case TokenType.Alternation:
                        if (natom == 0) throw new ArgumentException("empty alternation");
                        while (--natom > 0) outBuff.Append(CONCAT);
                        nalt++;
                        break;

                    case TokenType.End:
                        while (--natom > 0) outBuff.Append(CONCAT);
                        for(; nalt > 0; nalt--) outBuff.Append('|');
                        break;

                    default: throw new NotImplementedException();
                }
            }

            return outBuff.ToString();
        }
    }
}

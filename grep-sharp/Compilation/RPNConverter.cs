using grep_sharp.Compilation.Tokenization;
using System.Text;
using static grep_sharp.Constants;

namespace grep_sharp.Compilation
{
    public static class RPNConverter
    {
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

                    case TokenType.Quantifier:
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

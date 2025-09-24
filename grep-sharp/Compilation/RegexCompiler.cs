using grep_sharp.Compilation.NFAConstruction;
using grep_sharp.Compilation.Tokenization;

namespace grep_sharp.Compilation
{
    public static class RegexCompiler
    {
        public static CompilationResult Compile(string pattern)
        {
            if(pattern == null) throw new ArgumentException("Empty pattern provided");
            var tokens = Tokenizer.Tokenize(pattern);

            if(ShouldExpand(tokens)) 
                tokens = Tokenizer.ExpandQuantifierTokens(tokens);

            var rpnOutExp = RPNConverter.InfixToPostfix(tokens);
            var nfa =  NFABuilder.Build(rpnOutExp);
            return new CompilationResult(nfa, tokens);
        }

        private static bool ShouldExpand(List<Token> tokens) => tokens.Any(x => x.Type == TokenType.Quantifier);
    }
}

using grep_sharp.Compilation.NFAConstruction;
using grep_sharp.Compilation.Tokenization;

namespace grep_sharp.Compilation
{
    public class CompilationResult
    {
        public State CompiledPattern { get; }
        public List<Token> Tokens { get; }
        public PatternComplexity Complexity { get; }

        public CompilationResult(State compiledPattern, List<Token> tokens)
        {
            CompiledPattern = compiledPattern;
            Tokens = tokens;
            Complexity = AnalyzeComplexity(tokens);
        }

        private static PatternComplexity AnalyzeComplexity(List<Token> tokens)
        {
            bool hasQuantifiers = tokens.Any(t => t.Type == TokenType.Quantifier);
            bool hasAlternation = tokens.Any(t => t.Type == TokenType.Alternation);
            bool hasCharacterClasses = tokens.Any(t => t.Type == TokenType.CharClass);
            bool hasGroups = tokens.Any(t => t.Type == TokenType.GroupOpen);
            bool hasWildcards = tokens.Any(t => t.Type == TokenType.WildCard);

            return new PatternComplexity(
                HasComplexFeatures: hasQuantifiers || hasAlternation || hasCharacterClasses || hasGroups,
                HasQuantifiers: hasQuantifiers,
                HasAlternation: hasAlternation,
                HasWildcards: hasWildcards,
                TokenCount: tokens.Count
            );
        }
    }

    public record PatternComplexity(
        bool HasComplexFeatures,
        bool HasQuantifiers,
        bool HasAlternation,
        bool HasWildcards,
        int TokenCount
    );
}

using grep_sharp.Compilation;

namespace grep_sharp.RegEngine
{
    public static class StrategyHeuristic
    {
        private const int DFA_THRESHOLD_LINES = 50;
        private const int COMPLEX_PATTERN_THRESHOLD = 10;

        public static bool ShouldUseDfa(CompilationResult compilation, 
            long? fileSize = null, 
            int? estimatedLines = null)
        {
            var complexity = compilation.Complexity;

            if (!estimatedLines.HasValue || estimatedLines <= 1)
                return false;

            if (estimatedLines > DFA_THRESHOLD_LINES && !complexity.HasComplexFeatures)
                return true;

            if (complexity.HasAlternation || complexity.TokenCount > COMPLEX_PATTERN_THRESHOLD)
                return false;

            return true;
        }

        public static string GetStrategyExplanation(CompilationResult compilation, int? estimatedLines)
        {
            var complexity = compilation.Complexity;

            if (!estimatedLines.HasValue || estimatedLines <= 1)
                return "NFA: Single line input - avoiding DFA construction cost";

            if (estimatedLines > DFA_THRESHOLD_LINES && !complexity.HasComplexFeatures)
                return "DFA: Large file with simple pattern - amortizing construction cost";

            if (complexity.HasAlternation)
                return "NFA: Pattern contains alternation - DFA construction too expensive";

            if (complexity.TokenCount > COMPLEX_PATTERN_THRESHOLD)
                return "NFA: Complex pattern - avoiding expensive DFA construction";

            if (estimatedLines > 5 && !complexity.HasComplexFeatures)
                return "DFA: Medium file with simple pattern - worth construction cost";

            return "NFA: Default choice to minimize upfront cost";
        }
    }
}

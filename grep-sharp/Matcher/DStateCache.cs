using grep_sharp.Compilation.NFAConstruction;

namespace grep_sharp.Matcher
{
    public static class DStateCache
    {
        private static readonly Dictionary<string, DState> _cache = new();

        public static DState GetOrCreate(List<State> NFAStates)
        {
            var key = ComputeKey(NFAStates);
            if (_cache.TryGetValue(key, out var existing)) return existing;

            var dstate = new DState(NFAStates);
            _cache[key] = dstate;
            return dstate;
        }

        private static string ComputeKey(List<State> states)
        {
            return string.Join(',', states.OrderBy(s => s.GetHashCode())
                .Select(s => s.GetHashCode()));
        }
    }
}

# grep-sharp

A  regex matching tool built in C# with custom NFA/DFA implementation.

## Features

- **Custom Regex Engine**: Built-from-scratch NFA/DFA implementation with Thompson's construction
- **Auto Strategy Selection**: Automatic choice between NFA and DFA based on pattern complexity and input size
- **Multiple Matching Modes**: Count-only, line numbers, verbose output
- **Cross-platform**: Runs on .NET 8+ across Windows, Linux, and macOS
- **Standard Input Support**: Works with pipes and redirected input

## Installation

```bash
git clone https://github.com/yourusername/grep-sharp.git
cd grep-sharp
dotnet build -c Release
```

## Usage

### Basic Syntax
```bash
grep-sharp [OPTIONS] PATTERN [FILE]
grep-sharp [OPTIONS] -e PATTERN -f FILE
echo "text" | grep-sharp [OPTIONS] PATTERN
```

### Command Line Options

| Option | Long Form | Description |
|--------|-----------|-------------|
| `-e` | `--pattern` | Regex pattern to match (required) |
| `-f` | `--file` | Input file to search in |
| `-s` | `--strategy` | Matching strategy: `dfa`, `nfa`, or `auto` (default: auto) |
| `-c` | `--count` | Count matching lines instead of printing them |
| `-n` | `--line-number` | Show line numbers with matching lines |
| `-v` | `--verbose` | Show detailed strategy information |
| `-h` | `--help` | Display help message |

## Examples

### Basic Pattern Matching
```bash
# Find lines containing "error" in a log file
grep-sharp "error" application.log

# Search for email patterns
grep-sharp "[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}" contacts.txt

# Count occurrences of "function" in source code
grep-sharp -c "function" script.js
```

### Using Different Strategies
```bash
# Force DFA strategy for complex patterns on large files
grep-sharp -s dfa "[0-9]{3}-[0-9]{2}-[0-9]{4}" large-dataset.txt

# Force NFA strategy for simple patterns
grep-sharp -s nfa "hello" small-file.txt

# Verbose mode shows strategy selection reasoning
grep-sharp -v "complex.*pattern" data.txt
```

### Pipeline Usage
```bash
# Search in command output
cat logfile.txt | grep-sharp "ERROR|WARN"

# Combine with other tools
find . -name "*.cs" | xargs grep-sharp -n "TODO"

# Count matches from stdin
echo -e "line1\nline2\nline1" | grep-sharp -c "line1"
```

### Advanced Pattern Examples
```bash
# Character classes
grep-sharp "[0-9]+" numbers.txt              # Find digits
grep-sharp "[a-zA-Z]+" text.txt              # Find letters
grep-sharp "[^0-9]+" non-digits.txt          # Find non-digits

# Quantifiers
grep-sharp "ab*c" patterns.txt               # 'a' followed by zero or more 'b', then 'c'
grep-sharp "ab+c" patterns.txt               # 'a' followed by one or more 'b', then 'c'
grep-sharp "ab?c" patterns.txt               # 'a' followed by optional 'b', then 'c'

# Grouping and alternation
grep-sharp "(cat|dog)" animals.txt           # Find "cat" or "dog"
grep-sharp "gr(e|a)y" colors.txt             # Find "grey" or "gray"
```

## Architecture and Design

### Core Components

#### 1. Tokenization and Parsing
The regex engine follows a multi-stage compilation process:

- **Tokenizer**: Converts regex strings into tokens (literals, operators, character classes, etc.)
- **RPN Converter**: Transforms infix notation to postfix using Dijkstra's shunting-yard algorithm (with an implicit operator stack)
- **NFA Builder**: Constructs Thompson NFA using postfix expression and fragment-based approach

#### 2. Regex Engine Strategies

**NFA (Non-deterministic Finite Automaton)**
- Memory efficient: O(states) space complexity
- Consistent O(n*m) time complexity where n=input length, m=pattern complexity
- Better for complex patterns with many alternations

**DFA (Deterministic Finite Automaton)**
- Time efficient: O(n) time complexity after construction
- Can require exponential space in worst case
- Better for simple patterns on large inputs
- Implements on-demand subset construction with caching

#### 3. Strategy Selection Heuristics

The engine automatically chooses between NFA and DFA based on:
- Pattern complexity (number of states, alternations, quantifiers)
- Estimated input size
- Character class density

```csharp
// Simplified heuristic logic
bool ShouldUseDFA(pattern, fileSize, lineCount) =>
    (fileSize > LARGE_FILE_THRESHOLD && pattern.IsSimple()) ||
    (pattern.StateCount < MAX_DFA_STATES && lineCount > MIN_LINES_FOR_DFA);
```

### Design Decisions

#### Why Custom Implementation?
1. **Educational Value**: Understanding regex internals and finite automata theory
2. **Performance Control**: Fine-tuned optimizations for specific use cases
3. **Strategy Flexibility**: Ability to choose optimal algorithm per pattern
4. **Memory Management**: Precise control over allocations and GC pressure

#### Supported Regex Features
- ✅ Literal characters
- ✅ Character classes `[abc]`, `[a-z]`, `[^0-9]`
- ✅ Quantifiers `*`, `+`, `?`, `{n,m}`
- ✅ Alternation `|`
- ✅ Grouping `()`
- ✅ Anchors `^`, `$` (in development)
- - ✅ Wildcards `.` (in development)
- ❌ Subexpression extraction
- ❌ Backreferences `\1`, `\2`
- ❌ Lookahead/lookbehind `(?=...)`, `(?<!...)`
- ❌ Named groups `(?<name>...)`

## Performance Benchmarks

Based on internal testing with various pattern types:

| Pattern Type | Input Size | Strategy | Time (ms) | Memory |
|-------------|------------|----------|-----------|---------|
| Simple literal | 1MB | Fast Path | 2-3 | Low |
| Simple literal | 1MB | NFA | 8-9 | Low |
| Character class `[0-9]+` | 1MB | NFA | 12-15* | Low |
| Complex pattern | 1MB | DFA | 15-20 | Medium |
| Large file | 10MB | Auto | 80-100 | Medium |

*Significantly improved from original 63ms after character class parsing fixes


## Future Enhancements

- [ ] Anchor support (`^`, `$`) for line boundaries
- [ ] Unicode support and character categories
- [ ] Multi-line mode and file-wide matching
- [ ] Parallel processing for multiple files
- [ ] JIT compilation for hot patterns
- [ ] SIMD optimizations for character matching
- [ ] Plugin architecture for custom optimizations

## References

- Thompson, Ken (1968). "Regular Expression Search Algorithm"
- Aho, Alfred V.; Sethi, Ravi; Ullman, Jeffrey (1986). "Compilers: Principles, Techniques, and Tools"
- Cox, Russ (2007). "Regular Expression Matching Can Be Simple And Fast"

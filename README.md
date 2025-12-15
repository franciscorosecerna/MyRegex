**MyRegex — Regular Expression Engine (C#)**

Implementation from scratch of a regular expression engine in C# without using `System.Text.RegularExpressions`.

Main features:
- Syntax Tree (AST)-based engine with composite nodes.

- Manual backtracking to support quantifiers and alternations.

**Architecture**

The project models regular expressions as a set of nodes (`RegexNode`) that make up an AST. Design patterns such as Composite and Interpreter are used, along with a backtracking mechanism to support complex matches.

Leaves:
- `CharacterClass`
- `Digit`
- `EndAnchor`
- `Literal`
- `StartAnchor`
- `Whitespace`
- `Wildcard`
- `WordChar`

Composite Nodes:
- `OneOrMore` (one or more)
- `ZeroOrMore` (zero or more)
- `Optional` (optional)
- `RangeQuantifier` (range `{m,n}`)
- `Alternation` (`|`)
- `Group` (capturing groups)
- `Sequence`

**How it works**

Class: `RegexEngine`
- Constructor: `new RegexEngine(RegexNode root)` Creates the engine from the AST root node.

- `RegexMatch? Match(string text, int startPosition = 0)` attempts to match exactly from `startPosition`. Returns `RegexMatch` or `null`.

- `RegexMatch? Search(string text)` searches for the first match in the entire text (similar to `Regex.Match`).

- `IEnumerable<RegexMatch> Matches(string text)` iterates through all (non-overlapping) matches.

- `bool IsMatch(string text)` true if at least one match exists.

- `string Replace(string text, string replacement)` replaces all matches with `replacement` (note: does not yet support group references like `$1`).

- `IEnumerable<string> Split(string text)` splits the string using the matches as separators.

```csharp
//pattern: a{2,4}
var pattern = new RangeQuantifier(
    new Literal('a'),
    min: 2,
    max: 4
);

var engine = new RegexEngine(pattern);

Console.WriteLine(engine.IsMatch("aa"));    // true
Console.WriteLine(engine.IsMatch("aaaa"));  // true
Console.WriteLine(engine.IsMatch("a"));     // false
Console.WriteLine(engine.IsMatch("aaaaa")); // false
```

Current limitations
- Does not use `System.Text.RegularExpressions`. It is an educational and experimental implementation.
- `Replace` does not yet support group references (`$1`, `$2`, ...).
- There is (not yet) a parser to convert a pattern string directly into `RegexNode`; expressions can be manually constructed from nodes.
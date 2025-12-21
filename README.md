## **MyRegex — Regular Expression Engine (C#)**

Implementation from scratch of a regular expression engine in C# without using `System.Text.RegularExpressions`.

## Main features

- Full AST-based regex engine.
- Manual backtracking for quantifiers and alternations.
- Regex parser that converts a pattern string into an AST (RegexParser).

## **Architecture**

The project models regular expressions as a set of nodes (`RegexNode`) that make up an AST. Design patterns such as Composite and Interpreter are used, along with a backtracking mechanism to support complex matches.

Leaves:
- `CharacterClass`
- `Digit`
- `EndAnchor`
- `Literal`
- `StartAnchor`
- `Whitespace`
- `Wildcard`
- `WordBoundary`
- `WordChar`

Composite Nodes:
- `LookAhead`
- `LookBehind`
- `OneOrMore` (one or more)
- `ZeroOrMore` (zero or more)
- `Optional` (optional)
- `RangeQuantifier` (range `{m,n}`)
- `Alternation` (`|`)
- `Group` (capturing groups)
- `Sequence`

## **How it works**

Class: `RegexEngine`
- Constructor: `new RegexEngine(RegexNode root)` Creates the engine from the AST root node.

- `RegexMatch? Match(string text, int startPosition = 0)` attempts to match exactly from `startPosition`. Returns `RegexMatch` or `null`.

- `RegexMatch? Search(string text)` searches for the first match in the entire text (similar to `Regex.Match`).

- `IEnumerable<RegexMatch> Matches(string text)` iterates through all (non-overlapping) matches.

- `bool IsMatch(string text)` true if at least one match exists.

- `string Replace(string text, string replacement)` replaces all matches with `replacement`.

- `IEnumerable<string> Split(string text)` splits the string using the matches as separators.

```csharp
var pattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9])[A-Za-z\d\W_]*$";
var parser = new RegexParser(pattern);
var ast = parser.ParseExpression();
var engine = new RegexEngine(ast);


// Valid passwords
Assert.True(engine.IsMatch("Abc1!"));
Assert.True(engine.IsMatch("Password1@"));


// Invalid passwords
Assert.False(engine.IsMatch("password1!"));
Assert.False(engine.IsMatch("PASSWORD1!"));
Assert.False(engine.IsMatch("Password!"));
Assert.False(engine.IsMatch("Password1"));
```

## Current limitations
- Performance is not comparable to production regex engines.
- Error messages are minimal (parser-focused).

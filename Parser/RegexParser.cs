using MyRegex.Leaf;
using MyRegex.Nodes;

namespace MyRegex.Parser
{
    public class RegexParser
    {
        private readonly RegexLexer _lexer;
        private Token _current;

        public RegexParser(string pattern)
        {
            _lexer = new RegexLexer(pattern);
            _current = _lexer.Next();
        }

        public void Eat(TokenType type)
        {
            if (_current.Type != type)
                throw new Exception($"Expected {type}, got {_current.Type}");

            _current = _lexer.Next();
        }

        public RegexNode ParseExpression()
        {
            var left = ParseConcatenation();

            while (_current.Type == TokenType.Or)
            {
                Eat(TokenType.Or);
                var right = ParseConcatenation();
                left = new Alternation(left, right);
            }

            return left;
        }

        static bool CanStartPrimary(TokenType t) =>
            t is TokenType.Literal
               or TokenType.LParen
               or TokenType.Dot
               or TokenType.LBracket
               or TokenType.Caret
               or TokenType.Dollar
               or TokenType.Backslash;

        public RegexNode ParseConcatenation()
        {
            var nodes = new List<RegexNode>();

            while (CanStartPrimary(_current.Type))
            {
                nodes.Add(ParseRepetition());
            }

            return nodes.Count switch
            {
                0 => new Sequence(),
                1 => nodes[0],
                _ => new Sequence([.. nodes])
            };
        }

        public RegexNode ParseRepetition()
        {
            var node = ParsePrimary();

            while (_current.Type is TokenType.Star or TokenType.Plus or TokenType.Question or TokenType.LBrace)
            {
                node = _current.Type switch
                {
                    TokenType.Star => ConsumeAnd(() => new ZeroOrMore(node)),
                    TokenType.Plus => ConsumeAnd(() => new OneOrMore(node)),
                    TokenType.Question => ConsumeAnd(() => new Optional(node)),
                    TokenType.LBrace => ParseRange(node),
                    _ => node
                };
            }

            return node;
        }

        public RegexNode ConsumeAnd(Func<RegexNode> f)
        {
            Eat(_current.Type);
            return f();
        }

        public RegexNode ParseRange(RegexNode child)
        {
            Eat(TokenType.LBrace);

            int min = int.Parse(_current.Value);
            Eat(TokenType.Number);

            int? max = null;

            if (_current.Type == TokenType.Comma)
            {
                Eat(TokenType.Comma);
                if (_current.Type == TokenType.Number)
                {
                    max = int.Parse(_current.Value);
                    Eat(TokenType.Number);
                }
            }
            else
            {
                max = min;
            }

            Eat(TokenType.RBrace);
            return new RangeQuantifier(child, min, max);
        }

        public RegexNode ParsePrimary()
        {
            if (_current.Type == TokenType.Literal)
            {
                var c = _current.Value[0];
                Eat(TokenType.Literal);
                return new Literal(c);
            }

            if (_current.Type == TokenType.Dot)
            {
                Eat(TokenType.Dot);
                return new Wildcard();
            }

            if (_current.Type == TokenType.LParen)
            {
                Eat(TokenType.LParen);
                var expr = ParseExpression();
                Eat(TokenType.RParen);
                return new Group(expr);
            }

            if (_current.Type == TokenType.Caret)
            {
                Eat(TokenType.Caret);
                return new StartAnchor();
            }

            if (_current.Type == TokenType.Dollar)
            {
                Eat(TokenType.Dollar);
                return new EndAnchor();
            }

            if (_current.Type == TokenType.Backslash)
            {
                Eat(TokenType.Backslash);

                if (_current.Type != TokenType.Literal)
                    throw new Exception("Invalid escape");

                char c = _current.Value[0];
                Eat(TokenType.Literal);

                return c switch
                {
                    'd' => new Digit(),
                    'w' => new WordChar(),
                    's' => new Whitespace(),

                    '.' or '*' or '+' or '?' or '(' or ')' or
                    '[' or ']' or '{' or '}' or '|' or '^' or '$' or '\\'
                        => new Literal(c),

                    _ => new Literal(c)
                };
            }

            if (_current.Type == TokenType.LBracket)
            {
                return ParseCharacterClass();
            }

            throw new Exception("Unexpected token");
        }

        public RegexNode ParseCharacterClass()
        {
            Eat(TokenType.LBracket);

            bool negated = false;
            if (_current.Type == TokenType.Caret)
            {
                Eat(TokenType.Caret);
                negated = true;
            }

            var ranges = new List<CharacterRange>();
            var singles = new List<char>();

            while (_current.Type != TokenType.RBracket)
            {
                char start;

                if (_current.Type == TokenType.Backslash)
                {
                    Eat(TokenType.Backslash);

                    char esc = _current.Value[0];
                    Eat(TokenType.Literal);

                    switch (esc)
                    {
                        case 'd':
                            ranges.Add(new CharacterRange('0', '9'));
                            continue;
                        case 'w':
                            ranges.Add(new CharacterRange('a', 'z'));
                            ranges.Add(new CharacterRange('A', 'Z'));
                            ranges.Add(new CharacterRange('0', '9'));
                            singles.Add('_');
                            continue;
                        case 's':
                            singles.Add(' ');
                            singles.Add('\t');
                            singles.Add('\n');
                            singles.Add('\r');
                            continue;
                        default:
                            singles.Add(esc);
                            continue;
                    }
                }
                else
                {
                    start = _current.Value[0];
                    Eat(TokenType.Literal);
                }

                if (_current.Type == TokenType.Dash)
                {
                    Eat(TokenType.Dash);

                    char end = _current.Value[0];
                    Eat(TokenType.Literal);

                    ranges.Add(new CharacterRange(start, end));
                }
                else
                {
                    singles.Add(start);
                }
            }

            Eat(TokenType.RBracket);

            var cls = new CharacterClass(ranges, singles);

            return negated
                ? new NegatedNode(cls)
                : cls;
        }
    }
}
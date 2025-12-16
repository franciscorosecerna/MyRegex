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

        public RegexNode ParseConcatenation()
        {
            var nodes = new List<RegexNode>();

            while (_current.Type is TokenType.Literal or TokenType.LParen
                   or TokenType.Dot or TokenType.LBracket
                   or TokenType.Caret or TokenType.Dollar)
            {
                nodes.Add(ParseRepetition());
            }

            return nodes.Count == 1
                ? nodes[0]
                : new Sequence([.. nodes]);
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
                return expr;
            }

            throw new Exception("Unexpected token");
        }
    }
}
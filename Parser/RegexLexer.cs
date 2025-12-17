namespace MyRegex.Parser
{
    public enum TokenType
    {
        Literal,
        Dot,
        Star,
        Plus,
        Question,
        Or,
        LParen,
        RParen,
        LBracket,
        RBracket,
        Caret,
        Dollar,
        LBrace,
        RBrace,
        Comma,
        Number,
        Backslash,
        End,
        Dash,
        Equals,
        Exclamation,
        LessThan
    }
    public record Token(TokenType Type, string Value);

    public class RegexLexer
    {
        private readonly string _pattern;
        private int _pos;

        public RegexLexer(string pattern)
            => _pattern = pattern;

        public Token Next()
        {
            if (_pos >= _pattern.Length)
                return new Token(TokenType.End, "");

            char c = _pattern[_pos++];

            if (char.IsDigit(c))
            {
                int start = _pos - 1;
                while (_pos < _pattern.Length && char.IsDigit(_pattern[_pos]))
                    _pos++;
                return new Token(
                    TokenType.Number,
                    _pattern[start.._pos]
                );
            }

            return c switch
            {
                '.' => new Token(TokenType.Dot, "."),
                '*' => new Token(TokenType.Star, "*"),
                '+' => new Token(TokenType.Plus, "+"),
                '?' => new Token(TokenType.Question, "?"),
                '|' => new Token(TokenType.Or, "|"),
                '(' => new Token(TokenType.LParen, "("),
                ')' => new Token(TokenType.RParen, ")"),
                '[' => new Token(TokenType.LBracket, "["),
                ']' => new Token(TokenType.RBracket, "]"),
                '^' => new Token(TokenType.Caret, "^"),
                '$' => new Token(TokenType.Dollar, "$"),
                '{' => new Token(TokenType.LBrace, "{"),
                '}' => new Token(TokenType.RBrace, "}"),
                ',' => new Token(TokenType.Comma, ","),
                '-' => new Token(TokenType.Dash, "-"),
                '\\' => new Token(TokenType.Backslash, "\\"),
                '=' => new Token(TokenType.Equals, "="),
                '!' => new Token(TokenType.Exclamation, "!"),
                '<' => new Token(TokenType.LessThan, "<"),
                _ => new Token(TokenType.Literal, c.ToString())
            };
        }
    }
}
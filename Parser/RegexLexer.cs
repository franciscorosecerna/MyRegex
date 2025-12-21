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
        private bool _inCharClass;

        public RegexLexer(string pattern)
        {
            _pattern = pattern;
            _pos = 0;
            _inCharClass = false;
        }

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

            switch (c)
            {
                case '.': return new Token(TokenType.Dot, ".");
                case '*': return new Token(TokenType.Star, "*");
                case '+': return new Token(TokenType.Plus, "+");
                case '?': return new Token(TokenType.Question, "?");
                case '|': return new Token(TokenType.Or, "|");
                case '(': return new Token(TokenType.LParen, "(");
                case ')': return new Token(TokenType.RParen, ")");
                case '^': return new Token(TokenType.Caret, "^");
                case '$': return new Token(TokenType.Dollar, "$");
                case '{': return new Token(TokenType.LBrace, "{");
                case '}': return new Token(TokenType.RBrace, "}");
                case ',': return new Token(TokenType.Comma, ",");
                case '\\': return new Token(TokenType.Backslash, "\\");
                case '=': return new Token(TokenType.Equals, "=");
                case '!': return new Token(TokenType.Exclamation, "!");
                case '<': return new Token(TokenType.LessThan, "<");

                case '[':
                    _inCharClass = true;
                    return new Token(TokenType.LBracket, "[");

                case ']':
                    _inCharClass = false;
                    return new Token(TokenType.RBracket, "]");

                case '-':
                    return _inCharClass
                        ? new Token(TokenType.Dash, "-")
                        : new Token(TokenType.Literal, "-");

                default:
                    return new Token(TokenType.Literal, c.ToString());
            }
        }
    }
}
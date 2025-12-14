namespace MyRegex.Leaf
{
    public class Literal : RegexNode
    {
        private readonly char _char;

        public Literal(char c)
            => _char = c;

        public override MatchResult Match(MatchContext context, int position)
        {
            if (position >= context.Text.Length)
                return MatchResult.Failure(context);

            if (context.Text[position] == _char)
                return MatchResult.Success(position + 1, context);

            return MatchResult.Failure(context);
        }

        public override string ToString() => $"'{_char}'";
    }
}
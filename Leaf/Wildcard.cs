namespace MyRegex.Leaf
{
    public class Wildcard : RegexNode
    {
        public override MatchResult Match(MatchContext context, int position)
        {
            if (position >= context.Text.Length)
                return MatchResult.Failure(context);

            char c = context.Text[position];

            if (!context.Singleline && context.IsNewLine(c))
                return MatchResult.Failure(context);

            return MatchResult.Success(position + 1, context);
        }

        public override string ToString() => ".";
    }
}
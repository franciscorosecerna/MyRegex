namespace MyRegex.Leaf
{
    public class EndAnchor : RegexNode
    {
        public override MatchResult Match(MatchContext context, int position)
        {
            if (position == context.Text.Length)
                return MatchResult.Success(position, context);

            if (context.Multiline &&
                position < context.Text.Length &&
                context.IsNewLine(context.Text[position]))
            {
                return MatchResult.Success(position, context);
            }

            return MatchResult.Failure(context);
        }

        public override string ToString() => "$";
    }
}
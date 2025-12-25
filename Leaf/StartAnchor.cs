namespace MyRegex.Leaf
{
    public class StartAnchor : RegexNode
    {
        public override bool IsZeroWidth => true;
        public override MatchResult Match(MatchContext context, int position)
        {
            if (position == 0)
                return MatchResult.Success(position, context);

            if (context.Multiline &&
                position > 0 &&
                context.IsNewLine(context.Text[position - 1]))
            {
                return MatchResult.Success(position, context);
            }

            return MatchResult.Failure(context);
        }

        public override string ToString() => "^";
    }
}
namespace MyRegex.Leaf
{
    public class StartAnchor : RegexNode
    {
        public override MatchResult Match(MatchContext context, int position)
        {
            if (position == 0)
                return MatchResult.Success(position, context);

            return MatchResult.Failure(context);
        }

        public override string ToString() => "^";
    }
}
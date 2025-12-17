namespace MyRegex.Nodes
{
    public class PositiveLookahead : RegexNode
    {
        public RegexNode Inner { get; }

        public PositiveLookahead(RegexNode inner)
        {
            Inner = inner;
        }

        public override MatchResult Match(MatchContext context, int position)
        {
            var snapshot = context.Snapshot();

            var result = Inner.Match(context, position);

            context.RestoreFrom(snapshot);

            if (result.IsSuccess)
                return MatchResult.Success(position, context);

            return MatchResult.Failure(context);
        }
    }
}
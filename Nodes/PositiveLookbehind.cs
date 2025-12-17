namespace MyRegex.Nodes
{
    public class PositiveLookbehind : RegexNode
    {
        public RegexNode Inner { get; }
        private readonly int _length;

        public PositiveLookbehind(RegexNode inner, int length)
        {
            Inner = inner;
            _length = length;
        }

        public override MatchResult Match(MatchContext context, int position)
        {
            if (position < _length)
                return MatchResult.Failure(context);

            var snapshot = context.Snapshot();
            var result = Inner.Match(context, position - _length);
            context.RestoreFrom(snapshot);

            if (result.IsSuccess && result.Position == position)
                return MatchResult.Success(position, context);

            return MatchResult.Failure(context);
        }
    }
}

namespace MyRegex
{
    public class NegatedNode : RegexNode
    {
        private readonly RegexNode _inner;

        public NegatedNode(RegexNode inner)
            => _inner = inner;

        public override MatchResult Match(MatchContext context, int position)
        {
            if (position >= context.Text.Length)
                return MatchResult.Failure(context);

            var snapshot = context.Snapshot();
            var result = _inner.Match(context, position);

            context.RestoreFrom(snapshot);

            if (!result.IsSuccess)
                return MatchResult.Success(position + 1, context);

            return MatchResult.Failure(context);
        }

        public override string ToString() => $"[^{_inner}]";
    }
}
using MyRegex;
using MyRegex.Leaf;

namespace MyRegex
{
    public class NegatedNode : RegexNode
    {
        private readonly RegexNode _inner;
        public NegatedNode(RegexNode inner)
        {
            _inner = inner;
        }

        public override MatchResult Match(MatchContext context, int position)
        {
            if (!_inner.IsZeroWidth && position >= context.Text.Length)
                return MatchResult.Failure(context);

            var snapshot = context.Snapshot();
            var result = _inner.Match(context, position);
            context.RestoreFrom(snapshot);

            if (!result.IsSuccess)
            {
                int newPosition = _inner.IsZeroWidth ? position : position + 1;
                return MatchResult.Success(newPosition, context);
            }

            return MatchResult.Failure(context);
        }
    }
}
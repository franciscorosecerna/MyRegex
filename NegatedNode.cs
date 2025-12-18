using MyRegex;
using MyRegex.Leaf;

namespace MyRegex
{
    public class NegatedNode : RegexNode
    {
        private readonly RegexNode _inner;
        private readonly bool _isZeroWidth = false;

        public NegatedNode(RegexNode inner)
        {
            _inner = inner;
            _isZeroWidth = inner is WordBoundary ||
                           inner is StartAnchor ||
                           inner is EndAnchor;
        }

        public override MatchResult Match(MatchContext context, int position)
        {
            if (!_isZeroWidth && position >= context.Text.Length)
                return MatchResult.Failure(context);

            var snapshot = context.Snapshot();
            var result = _inner.Match(context, position);
            context.RestoreFrom(snapshot);

            if (!result.IsSuccess)
            {
                int newPosition = _isZeroWidth ? position : position + 1;
                return MatchResult.Success(newPosition, context);
            }

            return MatchResult.Failure(context);
        }
    }
}
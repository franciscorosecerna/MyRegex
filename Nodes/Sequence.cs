namespace MyRegex.Nodes
{
    public class Sequence : RegexNode
    {
        private readonly List<RegexNode> _children;

        public Sequence(params RegexNode[] children)
        {
            _children = [.. children];
        }

        public List<RegexNode> Childrens => _children;
        public override MatchResult Match(MatchContext context, int position)
        {
            return MatchFrom(context, position, 0);
        }

        private MatchResult MatchFrom(MatchContext context, int position, int childIndex)
        {
            if (childIndex >= _children.Count)
                return MatchResult.Success(position, context);

            var snapshot = context.Snapshot();
            var child = _children[childIndex];
            var result = child.Match(context, position);

            if (!result.IsSuccess)
            {
                context.RestoreFrom(snapshot);
                return MatchResult.Failure(context);
            }

            var restResult = MatchFrom(result.Context, result.Position, childIndex + 1);

            if (restResult.IsSuccess)
                return restResult;

            while (context.TryPopBacktrack(out var bt))
            {
                if (bt.Node != child)
                {
                    context.PushBacktrack(bt);
                    break;
                }

                var retryResult = bt.Node.Resume(bt.Context, bt.Position, bt.State);

                if (retryResult.IsSuccess)
                {
                    var retryRest = MatchFrom(retryResult.Context, retryResult.Position, childIndex + 1);

                    if (retryRest.IsSuccess)
                        return retryRest;
                }
            }

            context.RestoreFrom(snapshot);
            return MatchResult.Failure(context);
        }

        public override string ToString() => $"Seq({string.Join(", ", _children)})";
    }
}
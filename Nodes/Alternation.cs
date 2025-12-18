namespace MyRegex.Nodes
{
    public class Alternation : RegexNode
    {
        private readonly RegexNode _left;
        private readonly RegexNode _right;

        public Alternation(RegexNode left, RegexNode right)
        {
            _left = left;
            _right = right;
        }

        public override MatchResult Match(MatchContext context, int position)
        {
            var snapshot = context.Snapshot();

            var leftResult = _left.Match(context, position);

            if (leftResult.IsSuccess)
            {
                context.PushBacktrack(new BacktrackPoint(
                    Node: this,
                    Position: position,
                    Context: snapshot,
                    State: 0
                ));
                return leftResult;
            }

            context.RestoreFrom(snapshot);
            return _right.Match(context, position);
        }

        public override MatchResult Resume(
            MatchContext context,
            int position,
            int state)
        {
            return _right.Match(context, position);
        }

        public override string ToString() => $"({_left}|{_right})";
    }
}
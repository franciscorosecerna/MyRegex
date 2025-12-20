namespace MyRegex.Nodes
{
    public class Group : RegexNode
    {
        private readonly RegexNode _child;
        private readonly int _groupIndex;

        public Group(RegexNode child, int groupIndex)
        {
            _child = child;
            _groupIndex = groupIndex;
        }

        public int GroupIndex => _groupIndex;

        public RegexNode Child => _child;

        public override MatchResult Match(MatchContext context, int position)
        {
            var snapshot = context.Snapshot();
            int startPos = position;

            var result = _child.Match(context, position);

            if (result.IsSuccess)
            {
                string captured = result.GetMatched(startPos);
                result.Context.SetCapture(_groupIndex, captured);
                return result;
            }

            context.RestoreFrom(snapshot);
            return MatchResult.Failure(context);
        }

        public override string ToString() => $"(#{_groupIndex}:{_child})";
    }
}
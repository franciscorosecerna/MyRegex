namespace MyRegex.Nodes
{
    public abstract class Quantifier : RegexNode
    {
        protected readonly RegexNode Child;
        protected readonly int Min;
        protected readonly int? Max;

        protected Quantifier(RegexNode child, int min, int? max)
        {
            Child = child;
            Min = min;
            Max = max;
        }

        public override MatchResult Match(MatchContext context, int position)
        {
            var states = new List<(int Position, MatchContext Context)>();

            int currentPos = position;
            var currentCtx = context;
            int count = 0;

            while (Max == null || count < Max)
            {
                var snapshot = currentCtx.Snapshot();
                var r = Child.Match(currentCtx, currentPos);

                if (!r.IsSuccess || r.Position == currentPos)
                {
                    currentCtx.RestoreFrom(snapshot);
                    break;
                }

                count++;
                states.Add((r.Position, r.Context));
                currentPos = r.Position;
                currentCtx = r.Context;
            }

            if (count < Min)
                return MatchResult.Failure(context);

            for (int i = count - 1; i >= Min; i--)
            {
                context.PushBacktrack(
                    new BacktrackPoint(
                        Node: this,
                        Position: i == 0 ? position : states[i - 1].Position,
                        Context: i == 0 ? context : states[i - 1].Context,
                        State: i
                    )
                );
            }

            if (count == 0)
                return MatchResult.Success(position, context);

            var last = states[^1];
            return MatchResult.Success(last.Position, last.Context);
        }

        public override MatchResult Resume(
        MatchContext context,
        int position,
        int state)
        {
            if (state < Min)
                return MatchResult.Failure(context);
            return MatchResult.Success(position, context);
        }
    }

    public class ZeroOrMore : Quantifier
    {
        public ZeroOrMore(RegexNode child)
            : base(child, 0, null) { }

        public override string ToString() => $"({Child})*";
    }

    public class OneOrMore : Quantifier
    {
        public OneOrMore(RegexNode child)
            : base(child, 1, null) { }

        public override string ToString() => $"({Child})+";
    }

    public class Optional : Quantifier
    {
        public Optional(RegexNode child)
            : base(child, 0, 1) { }

        public override string ToString() => $"({Child})?";
    }

    public class RangeQuantifier : Quantifier
    {
        public RangeQuantifier(RegexNode child, int min, int? max)
            : base(child, min, max)
        {
            if (max.HasValue && max < min)
                throw new ArgumentException("max must be >= min");
        }

        public override string ToString()
        {
            if (Max == null)
                return $"({Child}){{{Min},}}";

            if (Min == Max)
                return $"({Child}){{{Min}}}";

            return $"({Child}){{{Min},{Max}}}";
        }
    }
}
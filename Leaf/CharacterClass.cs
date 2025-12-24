namespace MyRegex.Leaf
{
    public class CharacterClass : RegexNode
    {
        private readonly List<CharacterRange> _ranges;
        private readonly HashSet<char> _singles;
        private readonly List<RegexNode> _specialClasses;

        public CharacterClass(
            IEnumerable<CharacterRange>? ranges,
            IEnumerable<char>? singles,
            IEnumerable<RegexNode>? specialClasses = null)
        {
            _ranges = ranges?.ToList() ?? [];
            _singles = singles != null ? [.. singles] : new();
            _specialClasses = specialClasses?.ToList() ?? [];
        }

        public override MatchResult Match(MatchContext context, int position)
        {
            if (position >= context.Text.Length)
                return MatchResult.Failure(context);

            char c = context.Text[position];
            if (context.IgnoreCase)
                c = char.ToLowerInvariant(c);

            foreach (var s in _singles)
            {
                char sc = context.IgnoreCase
                    ? char.ToLowerInvariant(s)
                    : s;

                if (c == sc)
                    return MatchResult.Success(position + 1, context);
            }

            foreach (var r in _ranges)
            {
                char start = context.IgnoreCase
                    ? char.ToLowerInvariant(r.Start)
                    : r.Start;

                char end = context.IgnoreCase
                    ? char.ToLowerInvariant(r.End)
                    : r.End;

                if (c >= start && c <= end)
                    return MatchResult.Success(position + 1, context);
            }

            foreach (var special in _specialClasses)
            {
                var result = special.Match(context, position);
                if (result.IsSuccess)
                    return MatchResult.Success(position + 1, context);
            }

            return MatchResult.Failure(context);
        }
    }

    public record CharacterRange
    {
        public char Start { get; }
        public char End { get; }

        public CharacterRange(char start, char end)
        {
            if (start > end)
                (start, end) = (end, start);

            Start = start;
            End = end;
        }
    }
}
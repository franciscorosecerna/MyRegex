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

            char c = context.NormalizeCase(context.Text[position]);

            foreach (var s in _singles)
            {
                if (c == context.NormalizeCase(s))
                    return MatchResult.Success(position + 1, context);
            }

            foreach (var r in _ranges)
            {
                char start = context.NormalizeCase(r.Start);
                char end = context.NormalizeCase(r.End);

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
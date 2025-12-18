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

            if (_singles.Contains(c))
                return MatchResult.Success(position + 1, context);

            if (_ranges.Any(r => c >= r.Start && c <= r.End))
                return MatchResult.Success(position + 1, context);

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
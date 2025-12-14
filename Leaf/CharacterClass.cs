
namespace MyRegex.Leaf
{
    public class CharacterClass : RegexNode
    {
        private readonly List<CharacterRange> _ranges;
        private readonly HashSet<char> _singles;

        public CharacterClass(
            IEnumerable<CharacterRange>? ranges,
            IEnumerable<char>? singles)
        {
            _ranges = ranges?.ToList() ?? [];
            _singles = singles != null ? [.. singles] : new();
        }

        public override MatchResult Match(MatchContext context, int position)
        {
            if (position >= context.Text.Length)
                return MatchResult.Failure(context);

            char c = context.Text[position];

            bool match =
                _singles.Contains(c) ||
                _ranges.Any(r => c >= r.Start && c <= r.End);

            if (match)
                return MatchResult.Success(position + 1, context);

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
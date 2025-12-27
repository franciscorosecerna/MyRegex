using System.Globalization;

namespace MyRegex
{
    public class MatchContext
    {
        private Dictionary<int, RegexGroup> _captures;
        private Stack<BacktrackPoint> _backtrack;
        private readonly MyRegexOptions _options;

        public string Text { get; }
        public int MatchStart { get; set; } = -1;
        public bool HasMatchStarted => MatchStart >= 0;

        public MatchContext(string text, MyRegexOptions options)
        {
            Text = text;
            _captures = [];
            _backtrack = new();
            _options = options;
        }

        private MatchContext(
            string text,
            Dictionary<int, RegexGroup> captures,
            Stack<BacktrackPoint> backtrack)
        {
            Text = text;
            _captures = new Dictionary<int, RegexGroup>(captures);
            _backtrack = new Stack<BacktrackPoint>(backtrack.Reverse());
        }

        public RegexGroup? GetCapture(int groupIndex)
            => _captures.TryGetValue(groupIndex, out var g) ? g : null;

        public void SetCapture(int groupIndex, int start, int end)
        {
            _captures[groupIndex] = new RegexGroup(start, end);
        }

        public Dictionary<int, RegexGroup> GetAllCaptures()
            => new(_captures);

        public MatchContext Snapshot()
            => new(Text, _captures, _backtrack);

        public void RestoreFrom(MatchContext other)
        {
            _captures = new Dictionary<int, RegexGroup>(other._captures);
            _backtrack = new Stack<BacktrackPoint>(other._backtrack.Reverse());
        }

        public void PushBacktrack(BacktrackPoint point)
            => _backtrack.Push(point);

        public bool TryPopBacktrack(out BacktrackPoint point)
            => _backtrack.TryPop(out point!);

        public void DiscardBacktracksFrom(int checkpoint)
        {
            while (_backtrack.Count > checkpoint)
                _backtrack.Pop();
        }

        public int BacktrackCount 
            => _backtrack.Count;

        public bool IgnoreCase
            => (_options & MyRegexOptions.IgnoreCase) != 0;

        public bool Multiline 
            => (_options & MyRegexOptions.Multiline) != 0;

        public bool Singleline
            => (_options & MyRegexOptions.Singleline) != 0;

        public bool CultureInvariant
            => (_options & MyRegexOptions.CultureInvariant) != 0;

        public char NormalizeCase(char c)
        {
            if (!IgnoreCase)
                return c;

            return CultureInvariant
                ? char.ToLowerInvariant(c)
                : char.ToLower(c, CultureInfo.CurrentCulture);
        }

        public bool IsNewLine(char c)
            => c is '\n' or '\r';
    }

    public class MatchResult
    {
        public bool IsSuccess { get; set; }
        public int Position { get; set; }
        public required MatchContext Context { get; set; }

        public static MatchResult Failure(MatchContext context) =>
            new() { IsSuccess = false, Position = -1, Context = context };

        public static MatchResult Success(int pos, MatchContext context) =>
            new() { IsSuccess = true, Position = pos, Context = context };
    }

    public record BacktrackPoint(
        RegexNode Node,
        int Position,
        MatchContext Context,
        int State
    );
}
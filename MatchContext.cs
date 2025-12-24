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

        public bool IgnoreCase
            => _options.HasFlag(MyRegexOptions.IgnoreCase);

        public bool Multiline 
            => _options.HasFlag(MyRegexOptions.Multiline);

        public bool Singleline
            => _options.HasFlag(MyRegexOptions.Singleline);

        public bool CultureInvariant
            => _options.HasFlag(MyRegexOptions.CultureInvariant);

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

        public string GetMatched(int startPos)
        {
            if (!IsSuccess || startPos < 0 || Position > Context.Text.Length)
                return "";
            return Context.Text.Substring(startPos, Position - startPos);
        }
    }

    public record BacktrackPoint(
        RegexNode Node,
        int Position,
        MatchContext Context,
        int State
    );
}
namespace MyRegex
{
    public class MatchContext
    {
        private Dictionary<int, string> _captures;
        private Stack<BacktrackPoint> _backtrack;

        public string Text { get; }
        public int MatchStart { get; set; } = -1;
        public bool HasMatchStarted => MatchStart >= 0;

        public MatchContext(string text)
        {
            Text = text;
            _captures = [];
            _backtrack = new();
        }

        private MatchContext(
            string text,
            Dictionary<int, string> captures,
            Stack<BacktrackPoint> backtrack)
        {
            Text = text;
            _captures = new Dictionary<int, string>(captures);
            _backtrack = new Stack<BacktrackPoint>(backtrack.Reverse());
        }

        public string? GetCapture(int groupIndex)
            => _captures.TryGetValue(groupIndex, out var capture) ? capture : null;

        public void SetCapture(int groupIndex, string value)
            => _captures[groupIndex] = value;

        public Dictionary<int, string> GetAllCaptures()
            => new(_captures);

        public MatchContext Snapshot()
            => new(Text, _captures, _backtrack);

        public void RestoreFrom(MatchContext other)
        {
            _captures = new Dictionary<int, string>(other._captures);
            _backtrack = new Stack<BacktrackPoint>(other._backtrack.Reverse());
        }

        public void PushBacktrack(BacktrackPoint point)
            => _backtrack.Push(point);

        public bool TryPopBacktrack(out BacktrackPoint point)
            => _backtrack.TryPop(out point!);
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
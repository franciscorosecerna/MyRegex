using MyRegex.Parser;
using System.Text;

namespace MyRegex
{
    public class RegexEngine
    {
        private readonly RegexNode _root;
        private readonly MyRegexOptions _options;

        public RegexEngine(RegexNode root, MyRegexOptions options = MyRegexOptions.None)
        {
            _root = root;
            _options = options;
        }

        public RegexEngine(string pattern, MyRegexOptions options = MyRegexOptions.None)
        {
            _root = new RegexParser(pattern).ParseExpression();
            _options = options;
        }

        public RegexMatch? Match(string text, int startPosition = 0)
        {
            var context = new MatchContext(text, _options);
            var result = _root.Match(context, startPosition);

            if (result.IsSuccess)
            {
                int start = context.HasMatchStarted
                    ? context.MatchStart
                    : startPosition;

                return new RegexMatch(
                    text,
                    start,
                    result.Position,
                    result.Context.GetAllCaptures()
                );
            }

            while (context.TryPopBacktrack(out var point))
            {
                context.RestoreFrom(point.Context);
                result = point.Node.Resume(context, point.Position, point.State);

                if (result.IsSuccess)
                {
                    int start = context.HasMatchStarted
                        ? context.MatchStart
                        : startPosition;

                    return new RegexMatch(
                        text,
                        start,
                        result.Position,
                        result.Context.GetAllCaptures()
                    );
                }
            }

            return null;
        }

        public IEnumerable<RegexMatch> Matches(string text)
        {
            int index = 0;

            while (index <= text.Length)
            {
                var match = Match(text, index);

                if (match == null)
                {
                    index++;
                    continue;
                }

                yield return match;

                if (match.Start == match.End)
                    index = match.End + 1;
                else
                    index = match.End;
            }
        }

        public RegexMatch? Search(string text)
        {
            for (int i = 0; i <= text.Length; i++)
            {
                var match = Match(text, i);
                if (match != null)
                    return match;
            }

            return null;
        }

        public bool IsMatch(string text)
            => Search(text) != null;

        private string Replace(string text, Func<RegexMatch, string> evaluator)
        {
            var matches = Matches(text).ToList();
            if (matches.Count == 0) return text;

            var sb = new StringBuilder();
            int lastIndex = 0;

            foreach (var match in matches)
            {
                if (match.Start < lastIndex) continue;
                sb.Append(text[lastIndex..match.Start]);
                sb.Append(evaluator(match));
                lastIndex = match.End;
            }
            sb.Append(text[lastIndex..]);
            return sb.ToString();
        }

        public string Replace(string text, string replacement)
            => Replace(text, match => ExpandReplacement(replacement, match));

        private static string ExpandReplacement(string replacement, RegexMatch match)
        {
            var sb = new StringBuilder();

            for (int i = 0; i < replacement.Length; i++)
            {
                char c = replacement[i];

                if (c != '$')
                {
                    sb.Append(c);
                    continue;
                }

                if (i + 1 >= replacement.Length)
                {
                    sb.Append('$');
                    break;
                }

                char next = replacement[++i];

                if (next == '$')
                {
                    sb.Append('$');
                    continue;
                }

                if (next == '&' || next == '0')
                {
                    sb.Append(match.Value);
                    continue;
                }

                if (char.IsDigit(next))
                {
                    int index = next - '0';

                    while (i + 1 < replacement.Length &&
                           char.IsDigit(replacement[i + 1]))
                    {
                        index = index * 10 + (replacement[++i] - '0');
                    }

                    if (match.Groups.TryGetValue(index, out var group))
                    {
                        sb.Append(match.Text[group.Start..group.End]);
                    }

                    continue;
                }

                sb.Append('$').Append(next);
            }

            return sb.ToString();
        }

        public IEnumerable<string> Split(string text)
        {
            var matches = Matches(text).ToList();

            var parts = new List<string>();
            int lastIndex = 0;

            foreach (var match in matches)
            {
                if (match.Start == match.End && match.Start == lastIndex)
                {
                    parts.Add(text[lastIndex..match.Start]);
                    lastIndex = match.End;
                    continue;
                }

                if (match.Start < lastIndex)
                    continue;

                parts.Add(text[lastIndex..match.Start]);
                lastIndex = match.End;
            }

            parts.Add(text[lastIndex..]);
            return parts;
        }
    }

    public class RegexMatch
    {
        public string Text { get; }
        public int Start { get; }
        public int End { get; }
        public string Value => Text[Start..End];
        public Dictionary<int, RegexGroup> Groups { get; }

        public RegexMatch(string text, int start, int end, Dictionary<int, RegexGroup> groups)
        {
            Text = text;
            Start = start;
            End = end;
            Groups = new Dictionary<int, RegexGroup>(groups);
        }
    }

    public class RegexGroup
    {
        public int Start { get; }
        public int End { get; }

        public RegexGroup(int start, int end)
        {
            Start = start;
            End = end;
        }
    }

    [Flags]
    public enum MyRegexOptions
    {
        None = 0,
        IgnoreCase = 1,
    }
}
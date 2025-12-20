using System.Text;
using System.Text.RegularExpressions;

namespace MyRegex
{
    public class RegexEngine
    {
        private readonly RegexNode _root;

        public RegexEngine(RegexNode root)
        {
            _root = root;
        }

        public RegexMatch? Match(string text, int startPosition = 0)
        {
            var context = new MatchContext(text);
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
            for (int i = 0; i < text.Length; i++)
            {
                var match = Match(text, i);
                if (match != null)
                {
                    yield return match;
                    i = Math.Max(match.End - 1, i);
                }
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

        public string Replace(string text, Func<RegexMatch, string> evaluator)
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

                if (next == '&')
                {
                    sb.Append(match.Value);
                    continue;
                }

                if (next == '0')
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

                    if (match.Groups.TryGetValue(index, out var value))
                        sb.Append(value);

                    continue;
                }
                sb.Append('$').Append(next);
            }

            return sb.ToString();
        }

        public IEnumerable<string> Split(string text)
        {
            var matches = Matches(text).ToList();

            if (matches.Count == 0)
                return [text];

            var parts = new List<string>();
            int lastIndex = 0;

            foreach (var match in matches)
            {
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
        public Dictionary<int, string> Groups { get; }

        public RegexMatch(string text, int start, int end, Dictionary<int, string> groups)
        {
            Text = text;
            Start = start;
            End = end;
            Groups = new Dictionary<int, string>(groups);
        }
    }
}
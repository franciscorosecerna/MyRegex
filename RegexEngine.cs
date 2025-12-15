using System.Text;

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
                var finalContext = result.Context;

                return new RegexMatch(
                    text,
                    startPosition,
                    result.Position,
                    finalContext.GetAllCaptures()
                );

            }

            while (context.TryPopBacktrack(out var point))
            {
                context.RestoreFrom(point.Context);
                result = point.Node.Resume(context, point.Position, point.State);

                if (result.IsSuccess)
                {
                    return new RegexMatch(
                        text,
                        startPosition,
                        result.Position,
                        context.GetAllCaptures()
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

        public string Replace(string text, string replacement)
        {
            var matches = Matches(text).ToList();
            if (matches.Count == 0) return text;

            var sb = new StringBuilder();
            int lastIndex = 0;

            foreach (var match in matches)
            {
                if (match.Start < lastIndex)
                    continue;

                sb.Append(text[lastIndex..match.Start]);
                sb.Append(replacement);
                lastIndex = match.End;
            }

            sb.Append(text[lastIndex..]);

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
            Groups = groups;
        }
    }
}
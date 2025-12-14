namespace MyRegex.Leaf
{
    public class WordChar : RegexNode
    {
        public override MatchResult Match(MatchContext context, int position)
        {
            if (position >= context.Text.Length)
                return MatchResult.Failure(context);

            char c = context.Text[position];

            if (char.IsLetterOrDigit(c) || c == '_')
                return MatchResult.Success(position + 1, context);

            return MatchResult.Failure(context);
        }

        public override string ToString() => @"\w";
    }
}
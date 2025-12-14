namespace MyRegex.Leaf
{
    public class Whitespace : RegexNode
    {
        public override MatchResult Match(MatchContext context, int position)
        {
            if (position >= context.Text.Length)
                return MatchResult.Failure(context);

            if (char.IsWhiteSpace(context.Text[position]))
                return MatchResult.Success(position + 1, context);

            return MatchResult.Failure(context);
        }

        public override string ToString() => @"\s";
    }
}
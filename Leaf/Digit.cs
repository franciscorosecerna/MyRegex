namespace MyRegex.Leaf
{
    public class Digit : RegexNode
    {
        public override MatchResult Match(MatchContext context, int position)
        {
            if (position < context.Text.Length &&
                context.Text[position] >= '0' &&
                context.Text[position] <= '9')
            {
                return MatchResult.Success(position + 1, context);
            }

            return MatchResult.Failure(context);
        }

        public override string ToString() => @"\d";
    }
}
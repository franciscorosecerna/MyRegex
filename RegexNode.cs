namespace MyRegex
{
    public abstract class RegexNode
    {
        public abstract MatchResult Match(MatchContext context, int position);

        public virtual MatchResult Resume(
        MatchContext context,
        int position,
        int state)
        {
            return MatchResult.Failure(context);
        }
    }
}
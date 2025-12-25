namespace MyRegex
{
    public abstract class RegexNode
    {
        public virtual bool IsZeroWidth => false;
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
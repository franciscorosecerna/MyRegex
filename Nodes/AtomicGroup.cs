using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyRegex.Nodes
{
    public class AtomicGroup : RegexNode
    {
        public RegexNode Inner { get; }

        public AtomicGroup(RegexNode inner)
            => Inner = inner;

        public override MatchResult Match(MatchContext context, int position)
        {
            var snapshot = context.Snapshot();
            int checkpoint = context.BacktrackCount;

            var result = Inner.Match(context, position);

            if (!result.IsSuccess)
            {
                context.RestoreFrom(snapshot);
                return MatchResult.Failure(context);
            }

            context.DiscardBacktracksFrom(checkpoint);

            return result;
        }
    }
}

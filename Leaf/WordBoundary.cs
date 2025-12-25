using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyRegex.Leaf
{
    public class WordBoundary : RegexNode
    {
        public override bool IsZeroWidth => true;
        public override MatchResult Match(MatchContext context, int position)
        {
            bool leftIsWord =
                position > 0 &&
                IsWordChar(context.Text[position - 1]);

            bool rightIsWord =
                position < context.Text.Length &&
                IsWordChar(context.Text[position]);

            return leftIsWord != rightIsWord
                ? MatchResult.Success(position, context)
                : MatchResult.Failure(context);
        }

        private static bool IsWordChar(char c)
            => char.IsLetterOrDigit(c) || c == '_';
    }
}
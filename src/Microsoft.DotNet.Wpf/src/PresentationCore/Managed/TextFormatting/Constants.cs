
namespace Managed.TextFormatting
{
    internal static partial class Constants
    {
        public const double DefaultRealToIdeal = 28800.0 / 96;
        public const double DefaultIdealToReal = 1 / DefaultRealToIdeal;
        public const int    IdealInfiniteWidth = 0x3FFFFFFE;
        public const double RealInfiniteWidth = IdealInfiniteWidth * DefaultIdealToReal;

        // A reasonable maximum interword spacing for normal text is half an em
        // while a good average is a third of em. 
        // A reasonable minimum interword spacing for normal text is a fifth of em
        // while a good average is a quarter of em.
        //
        // [Robert Bringhurst "The Elements of Typographic Style" p.26]
        public const double MinInterWordCompressionPerEm = 0.2;
        public const double MaxInterWordExpansionPerEm = 0.5;

        // According to Knuth, the linebreak that results in a line with stretchability
        // 4.7 or greater is considered awful and should be thrown away from the optimal
        // calculation. 
        //
        // Stretchability (S) is the value proportion of the expandable excess width in
        // the line and the maximum expansion allowed. S <= 1 is considered a 'good'
        // linebreak. 1 < S < 4.7 is kinda bad but still considered acceptable, and 
        // S >= 4.7 is just plain awful and should never be chosen.
        //
        // PTLS allows for multiple layers of expansion insertion. We define the first
        // layer to be for 'good' break, which by definition max out at 1.0 stretchability. 
        // The second layout is defined as 'acceptable' break and max out at an addition of
        // 2.0. The third layer is defined as 'bad'. It is at this layer where we start to
        // distribute expansion between letters (inter-letter expansion). This layer max
        // out at infinity. The value defined below is the max out stretchability of the
        // second layer. This means we are saying - we'll start inter-letter distribution
        // for a break which yields the line with stretchability greater than 3.0. Thus, a
        // line with inter-letter expansion may be accepted within an optimal paragraph 
        // calculation only if it has stretchability >= 3.0 but < 4.7.
        //
        // So, just to give an idea of how difference a line with S = 1.0 and S = 4.7 is in 
        // Knuth's calculation; the badness of the line is the power of six of its stretchability 
        // value. Line with greater stretchability is significantly worse than one with smaller value.
        //
        // For more detail on the badness calculation; see the "Optimal Paragraph" design spec
        // Paragraph.doc.
        public const int AcceptableLineStretchability = 2;

        // Minimum number of characters before and after the current position to
        // be analyzed by the lexical service component such as hyphenation or 
        // word-break and cached so future call within the same character range 
        // can be done efficiently.
        //
        // Making these numbers too small would result in more cache miss during 
        // line-breaking. Making it too great wastes memory.
        public const int MinCchToCacheBeforeAndAfter = 16;

        /// <summary>
        /// Greatest multiple of em allowed in composite font file
        /// </summary>
        public const double GreatestMutiplierOfEm = 100;
    }
}

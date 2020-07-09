using System.Collections.Generic;

namespace Lx
{
    public class Expression : Segment//, ICompositeSegment<Expression>
    {
        public SegmentChain<Morpheme> Sequence { get; set; }
        public new SegmentChain<Expression> Composition;

        public override void Initialize()
        {
            Sequence = new SegmentChain<Morpheme>();
            Composition = new SegmentChain<Expression>();
        }

        public Expression(string rawExpression = "")
        {
            Initialize();
            Graph = rawExpression;
        }

        public enum GrammaticalityJudgement { Good, Marginal, Bad, Weird }
        public HashSet<string> Translations { get; set; }
        public GrammaticalityJudgement Judgement { get; set; }
        public Language Language { get; set; }

        public override string ToString()
        {
            //var expression = new StringBuilder();
            //Morpheme lastMorpheme = null;

            //foreach (var morpheme in this)
            //{
            //    if (lastMorpheme)
            //}

            return string.Join("", Sequence);
        }

        public SegmentChain<Expression> SetComposition(List<Expression> componentSegments)
        {
            Composition = SegmentChain<Expression>.NewSegmentChain(componentSegments);
            return Composition;
        }

        //internal void AddLast(Morpheme morph)
        //{
        //    Sequence.AddLast(morph);
        //}

        //public SegmentChain<Expression> GetComposition()
        //{
        //    return Composition;
        //}
    }

    //Discource set as a way of specifying features for a sequence of Expressions.


}


using System.Collections.Generic;
using System.Linq;

namespace Lx
{
    /// <summary>
    /// A linked list of Expression objects; expected to form a larger discourse unit. 
    /// e.g. a paragraph, a dialogue, etc.
    /// </summary>
    /// 
    public class Discourse : Segment//, ICompositeSegment<Discourse>
    {
        public SegmentChain<Expression> Expressions { get; set; }
        public new SegmentChain<Discourse> Composition { get; set; }

        public override void Initialize()
        {
            Expressions = new SegmentChain<Expression>();
            Composition = new SegmentChain<Discourse>();
        }

        public Discourse()
        {
            Expressions = new SegmentChain<Expression>();
            Composition = new SegmentChain<Discourse>();
        }

        public SegmentChain<Discourse> GetComposition()
        {
            return Composition;
        }

        public SegmentChain<Discourse> SetComposition(List<Discourse> componentSegments)
        {
            Composition = SegmentChain<Discourse>.NewSegmentChain(componentSegments);
            return Composition;
        }

        public override string ToString()
        {
            return string.Join("\n", Expressions.Select(exp => exp.Graph).ToArray());
        }

        //internal void AddLast(Expression expression)
        //{
        //    Expressions.AddLast(expression);
        //}
    }

    //Discource set as a way of specifying features for a sequence of Expressions.


}


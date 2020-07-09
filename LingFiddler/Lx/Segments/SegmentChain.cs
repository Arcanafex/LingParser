using System.Collections.Generic;
using System.Text;

namespace Lx
{
    public class SegmentChain<T> : LinkedList<T> where T : Segment
    {
        private string graph = null;
        public string Graph
        {
            get
            {
                if (graph == null)
                {
                    graph = this.ToString();
                }

                return graph;
            }
        }

        public static SegmentChain<T> NewSegmentChain(List<T> segmentList)
        {
            var chain = new SegmentChain<T>();
            foreach (var segment in segmentList)
            {
                chain.AddLast(segment);
            }
            return chain;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            foreach(var item in this)
            {
                sb.Append(item.ToString());
            }

            return sb.ToString();
        }
    }


}


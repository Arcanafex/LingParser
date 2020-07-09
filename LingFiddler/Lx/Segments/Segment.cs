using System;

namespace Lx
{
    public abstract class Segment
    {
        internal readonly Guid id = new Guid();
        public string Name { get; set; }
        public string Graph { get; set; }
        public SegmentChain<Segment> Composition { get; set; }

        public abstract void Initialize();

        public virtual Guid GetID()
        {
            return id;
        }
    }


}


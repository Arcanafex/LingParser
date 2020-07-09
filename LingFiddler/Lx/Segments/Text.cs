using System.Text;

namespace Lx
{
    public class Text : Segment
    {
        public SegmentChain<Discourse> Discourse { get; set; }
        public new SegmentChain<Text> Composition;

        //a set of Discourse units of Expressions
        public string Title { get; set; }
        public Lexicon Lexicon { get; set; }
        public Lexicon Paralexicon { get; set; }
        public int Count
        {
            get
            {
                return Discourse.Count;
            }
        }

        public override void Initialize()
        {
            Discourse = new SegmentChain<Discourse>();
            Composition = new SegmentChain<Text>();
        }

        public Text()
        {
            Discourse = new SegmentChain<Discourse>();
            Composition = new SegmentChain<Text>();
            Lexicon = new Lexicon();
            Paralexicon = new Lexicon
            {
                string.Empty
            };
        }

        public override string ToString()
        {
            var text = new StringBuilder();

            foreach (var paragraph in Discourse)
            {
                text.AppendLine(paragraph.ToString());
                text.AppendLine();
            }

            return text.ToString();
        }

        //internal void AddLast(Discourse paragraph)
        //{
        //    Discourse.AddLast(paragraph);
        //}

        internal void Clear()
        {
            if (Discourse != null)
                Discourse.Clear();

            if (Composition != null)
                Composition.Clear();
        }
    }


}


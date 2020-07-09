using System.Collections.Generic;

namespace Lx
{
    /// <summary>
    /// Class representing the surface word form.
    /// </summary>
    public class Morpheme : Segment
    {
        //public string Graph { get; set; }
        public int Length { get { return Graph.Length; } }

        public List<SegmentChain<Grapheme>> GraphemeChain { get; set; }
        public List<SegmentChain<Phoneme>> PhonemeChain { get; set; }
        public Concept Meaning { get; set; }
        public HashSet<Feature> Features { get; set; }
        public new SegmentChain<Morpheme> Composition;

        public override void Initialize()
        {
            Composition = new SegmentChain<Morpheme>();
            GraphemeChain = new List<SegmentChain<Grapheme>>();
            PhonemeChain = new List<SegmentChain<Phoneme>>();
            Features = new HashSet<Feature>();
        }

        public Morpheme(string graph)
        {
            Graph = graph;

            GraphemeChain = new List<SegmentChain<Grapheme>>();
            PhonemeChain = new List<SegmentChain<Phoneme>>();
            Meaning = new Concept();
            Features = new HashSet<Feature>();
        }

        public Morpheme(SegmentChain<Grapheme> glyphChain)
        {
            Graph = glyphChain.ToString();

            GraphemeChain = new List<SegmentChain<Grapheme>>() { glyphChain };
            PhonemeChain = new List<SegmentChain<Phoneme>>();
            Meaning = new Concept();
            Features = new HashSet<Feature>();
        }

        public Morpheme(List<Grapheme> graphemes)
        {
            var graphemeChain = SegmentChain<Grapheme>.NewSegmentChain(graphemes);
            Graph = graphemeChain.ToString();

            GraphemeChain = new List<SegmentChain<Grapheme>>() { graphemeChain };
            PhonemeChain = new List<SegmentChain<Phoneme>>();
            Meaning = new Concept();
            Features = new HashSet<Feature>();
        }

        public static bool operator ==(Morpheme left, Morpheme right)
        {
            if (left is null)
                return right is null;
            else
                return left.Equals(right);
        }

        public static bool operator !=(Morpheme left, Morpheme right)
        {
            return !(left.Equals(right));
        }

        public bool Equals(Morpheme other)
        {
            if (other is null)
                return false;
            else
                return this.Graph == other.Graph;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Morpheme);
        }

        public override string ToString()
        {
            return Graph;
        }

        public override int GetHashCode()
        {
            return Graph.GetHashCode();
        }
    }

}


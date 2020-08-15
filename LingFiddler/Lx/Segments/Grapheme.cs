using System.Collections.Generic;
using System.Linq;

namespace Lx
{
    /// <summary>
    /// A Grapheme is a language specific application of a specific glyph or arrangement of glyphs. 
    /// </summary>
    public class Grapheme : Segment
    {
        public List<GlyphChain> Glyphs { get; set; }
        public new SegmentChain<Grapheme> Composition { get; set; }

        public override void Initialize()
        {
            Glyphs = new List<GlyphChain>();
            Composition = new SegmentChain<Grapheme>();
        }

        public Grapheme(Glyph glyph)
        {
            Initialize();

            var glyphChain = new GlyphChain(new List<Glyph>() { glyph });
            Glyphs.Add(glyphChain);
            Graph = glyph.ToString();
        }

        public Grapheme(List<Glyph> glyphs)
        {
            Initialize();

            var glyphChain = new GlyphChain(glyphs);
            Glyphs.Add(glyphChain);
            Graph = glyphChain.ToString();
        }

        public void AddGlyphChain(Glyph glyph)
        {
            var glyphChain = new GlyphChain(new List<Glyph>() { glyph });

            if (Glyphs != null)
            {
                if (Glyphs.Count == 0)
                    Graph = glyph.ToString();

                if (!Glyphs.Contains(glyphChain))
                    Glyphs.Add(glyphChain);
            }
        }

        public void AddGlyphChain(List<Glyph> glyphs)
        {
            var glyphChain = new GlyphChain(glyphs);

            if (Glyphs != null)
            {
                if (Glyphs.Count == 0)
                    Graph = glyphChain.ToString();

                if (!Glyphs.Contains(glyphChain))
                    Glyphs.Add(glyphChain);
            }
        }

        public override string ToString()
        {
            string graph = Graph;

            if (Glyphs != null && Glyphs.Count > 0)
                graph = Glyphs.First().ToString();

            return graph;
        }

        public SegmentChain<Grapheme> SetComposition(List<Grapheme> componentSegments)
        {
            Composition = SegmentChain<Grapheme>.NewSegmentChain(componentSegments);
            return Composition;
        }

        public static bool operator ==(Grapheme left, Grapheme right)
        {
            if (left is null)
                return right is null;
            else
                return left.Equals(right);
        }

        public static bool operator !=(Grapheme left, Grapheme right)
        {
            return !(left.Equals(right));
        }

        public bool Equals(Grapheme other)
        {
            if (other is null)
                return false;
            else
                return this.Graph == other.Graph;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Grapheme);
        }

        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }
    }

    //Discource set as a way of specifying features for a sequence of Expressions.


}


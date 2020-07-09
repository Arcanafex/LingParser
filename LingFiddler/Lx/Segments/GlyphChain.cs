using System.Collections.Generic;
using System.Text;

namespace Lx
{
    public class GlyphChain : SegmentChain<Glyph>
    {
        public GlyphChain(List<Glyph> glyphs)
        {
            foreach (var g in glyphs)
            {
                AddLast(g);
            }
        }

        public override string ToString()
        {
            var chain = new StringBuilder();

            foreach(var g in this)
            {
                chain.Append(g.Characters);
            }

            return chain.ToString();
        }

        public static bool operator ==(GlyphChain left, GlyphChain right)
        {
            if (left is null)
                return right is null;
            else
                return left.Equals(right);
        }

        public static bool operator !=(GlyphChain left, GlyphChain right)
        {
            return !(left.Equals(right));
        }

        public bool Equals(GlyphChain other)
        {
            if (other is null)
                return false;
            else
                return this.ToString() == other.ToString();
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as GlyphChain);
        }

        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }
    }

    //Discource set as a way of specifying features for a sequence of Expressions.


}


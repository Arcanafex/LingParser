using System.Collections.Generic;

namespace Lx
{
    /// <summary>
    /// One or more chars that together act as a single segmental unit of symbolic representation
    /// </summary>
    public class Glyph : Segment
    {
        public Script Script;
        internal readonly string graph;
        public new string Graph { get { return graph; } }
        internal readonly char[] characters;
        public char[] Characters { get { return characters; } }
        public new SegmentChain<Glyph> Composition { get; set; }

        public override void Initialize()
        {
            Composition = new SegmentChain<Glyph>();
        }

        public Glyph(string symbol)
        {
            Initialize();

            graph = symbol;
            characters = symbol.ToCharArray();
        }

        public Glyph(char[] symbols)
        {
            Initialize();

            graph = new string(symbols);
            characters = new char[symbols.Length];
            symbols.CopyTo(Characters, 0);
        }

        public SegmentChain<Glyph> SetComposition(List<Glyph> componentSegments)
        {
            Composition = SegmentChain<Glyph>.NewSegmentChain(componentSegments);
            return Composition;
        }

        public static bool operator ==(Glyph left, Glyph right)
        {
            if (left is null)
                return right is null;
            else
                return left.Equals(right);
        }

        public static bool operator !=(Glyph left, Glyph right)
        {
            return !(left.Equals(right));
        }

        public bool Equals(Glyph other)
        {
            if (other is null)
                return false;
            else
                return this.Graph == other.Graph;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Glyph);
        }

        public override string ToString()
        {
            return new string(Characters);
        }

        public override int GetHashCode()
        {
            return Graph.GetHashCode();
        }
    }

    //Discource set as a way of specifying features for a sequence of Expressions.


}


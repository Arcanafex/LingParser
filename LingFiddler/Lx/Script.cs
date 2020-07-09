using System.Collections.Generic;

namespace Lx
{
    /// <summary>
    /// a set of Glyphs
    /// </summary>
    public class Script : Dictionary<string, Glyph>
    {
        public string Name { get; set; }
        // Ordering of the set of glyphs?

        public Glyph AddGlyph(string glyph)
        {
            if (ContainsKey(glyph))
            {
                return this[glyph];
            }
            else
            {
                var outGlyph = new Glyph(glyph);
                Add(glyph, outGlyph);
                return outGlyph;
            }
        }

        public List<Glyph> AddGlyphs(string[] glyphs)
        {
            var glyphList = new List<Glyph>();

            foreach (var glyph in glyphs)
            {
                glyphList.Add(AddGlyph(glyph));
            }

            return glyphList;
        }

        public List<Glyph> AddGlyphs(char[] glyphs)
        {
            var glyphList = new List<Glyph>();

            foreach (var glyph in glyphs)
            {
                glyphList.Add(AddGlyph(glyph.ToString()));
            }

            return glyphList;
        }
    }

}


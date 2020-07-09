using System.Collections.Generic;

namespace Lx
{
    /// <summary>
    /// Orthography is a language-specific application of a script's glyphs
    /// </summary>
    public class Orthography : Dictionary<Grapheme, int>
    {
        public Script Script { get; set; }
        //public Dictionary<Glyph, Schema> Rules { get; set; }
        //public List<string> Environments;

        // Rules for glyph expression
        //public class Schema
        //{
        //    public Glyph Lemma { get; set; }
        //    public List<string> Environments { get; set; }
        //    public Glyph Form { get; set; }
        //}

        public List<Grapheme> AddGraphemes(List<Glyph> glyphs)
        {
            var graphemes = new List<Grapheme>();

            foreach (var glyph in glyphs)
            {
                graphemes.Add(AddGrapheme(glyph));
            }

            return graphemes;
        }

        public Grapheme AddGrapheme(Glyph glyph)
        {
            Grapheme grapheme = new Grapheme(glyph);
            
            if (!ContainsKey(grapheme))
            {
                Add(grapheme, Count);
            }

            return grapheme;
        }
    }

    //Discource set as a way of specifying features for a sequence of Expressions.


}


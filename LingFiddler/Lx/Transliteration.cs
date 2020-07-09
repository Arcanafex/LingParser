namespace Lx
{
    /// <summary>
    /// A Mapping between a Glyph and a Grapheme
    /// </summary>
    public class Transliteration
    {
        public class Schema
        {
            public Glyph Glyph { get; set; }
            public Grapheme Grapheme { get; set; }
        }
    }

}


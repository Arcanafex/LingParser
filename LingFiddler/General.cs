using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LingFiddler
{
    public class Language
    {
        public string Name { get; set; }
        public string Autonym { get; set; }
        public List<string> Exonyms { get; set; }

        // This perhaps will hold the handle for serializing to the language specific Database.

        public PragmaSet Lexicon { get; set; }
        public ParadigmSet Morphology { get; set; }
    }

    /// <summary>
    /// a set of Glyphs
    /// </summary>
    public class Script : Dictionary<Glyph, int>
    {
        public string Name { get; set; }
        // Ordering of the set of glyphs?
        // public Dictionary<Glyph, string> GlyphInventory { get; set; }
    }

    public class Glyph
    {
        // Ideally, the glyph will be a single unicode code point, with the Grapheme being the combination of, for example, a single character + a diacritic

        public char Graph { get; set; }
        //image form?
    }

    /// <summary>
    /// Class representing the surface word form.
    /// </summary>
    public class Morph
    {
        public string Graph { get; set; }
        public int Length { get { return Graph.Length; } }
        public int Frequency
        {
            get
            {
                return Lexicon.ContainsKey(this) ? Lexicon[this] : 0;
            }
        }

        public List<Grapheme> GraphemeChain { get; set; }
        public List<Phoneme> PhonemeChain { get; set; }

        public Pragma Meaning { get; set; }
        public HashSet<Feature> Features { get; set; }

        public Morph(string graph)
        {
            this.Graph = graph;
        }

        #region Static stuff
        private static Dictionary<Morph, int> lexicon;
        public static Dictionary<Morph, int> Lexicon
        {
            get
            {
                if (lexicon == null)
                {
                    lexicon = new Dictionary<Morph, int>();
                }

                return lexicon;
            }
            set
            {
                lexicon = value;
            }
        }

        public static void Add(string word, int weight = 1)
        {
            if (String.IsNullOrEmpty(word))
                return;

            //word = word.ToLower().Trim(Morph.FilterChars);

            //if (word == String.Empty)
            //    return;

            //foreach (var morph in word.Split(Morph.FilterChars, StringSplitOptions.RemoveEmptyEntries))
            //{
                var thisWord = Lexicon.Keys.FirstOrDefault(w => w.Graph == word);

                if (thisWord == null)
                {
                    thisWord = new Morph(word);

                    Lexicon.Add(thisWord, weight);
                }
                else
                {
                    Lexicon[thisWord] += weight;
                }
            //}
        }

        public static void Remove(Morph word)
        {
            Lexicon.Remove(word);
        }

        public static void Clear()
        {
            Lexicon.Clear();
        }
        #endregion


        public override bool Equals(object obj)
        {
            var other = obj as Morph;
            return this.Graph.Equals(other.Graph);
        }

        public override string ToString()
        {
            return this.Graph;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    //public struct Context<T>
    //{
    //    public T Item;
    //    public int Frequency;
    //    //N Levels deep?
    //    public List<Context<T>> Subcontext;
    //}



    public class Expression : List<Morph>
    {
        public enum GrammaticalityJudgement { Good, Bad, Weird }

        public string Graph { get; set; }
        //public List<Morph> MorphChain { get; set; }
        public HashSet<string> Translations { get; set; }
        public GrammaticalityJudgement Judgement { get; set; }
        public Language Language { get; set; }

        // context? This may be retrieved from Text set
        // discourse context? e.g. semantic/pragmatic assumptions affection expression
        // leaving aside issues of code-switching for now, wrt Language source
    }

    public class Text : List<Expression>
    {
        //a set of Expressions
        public string Title { get; set; }
        //public List<Expression> Content { get; set; }
    }

    public class Corpus : HashSet<Text>
    {
        //a set of Texts
        public string Title { get; set; }
        //public HashSet<Text> Texts { get; set; }
        public string Description { get; set; }
    }


    /// <summary>
    /// A Graph is a language specific use of a specific glyph or arrangement of glyphs. 
    /// </summary>
    public class Grapheme
    {
        #region Static Members
        private static HashSet<Grapheme> graphemes;
        public static HashSet<Grapheme> Graphemes
        {
            get
            {
                if (graphemes == null)
                    graphemes = new HashSet<Grapheme>();

                return graphemes;
            }
            set
            {
                graphemes = value;
            }
        }
        #endregion

        public string Graph { get; set; }
        public List<Glyph> GlyphChain { get; set; }
        public int Frequency { get; set; }

        //context: attested sequences
        // next graph, frequency
        // last graph
        //
    }

    /// <summary>
    /// Orthography is a language-specific application of a script's glyphs
    /// </summary>
    //public class Orthography : HashSet<Grapheme> { }

    public class Phoneme { }

    public class Phonology { }

    public class Pragma
    {
        public string Symbol { get; set; }
    }

    public class PragmaSet : HashSet<Pragma>
    {
        //public HashSet<Pragma> Pragmata { get; set; }
    }

    public class Feature { }
    public class FeatureSet { }

    public class Paradigm
    {
        // geometry of inflectional features
        // functionally the mapping of Pragma to Morph (one to many)
        // function(Pragma, Features) => Morph
    }

    public class ParadigmSet : HashSet<Paradigm>
    {
        // The set of Paradigms
        //public HashSet<Paradigm> Paradigms { get; set; }
    }

    //Discource set as a way of specifying features for a sequence of Expressions.
}

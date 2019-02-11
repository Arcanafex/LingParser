using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lingx
{
    public class Language
    {
        public static Language CurrentLanguage { get; set; }

        public string Name { get; set; }
        public string Autonym { get; set; }
        public List<string> Exonyms { get; set; }

        public PragmaSet Ontology { get; set; }
        public ParadigmSet Morphology { get; set; }
        public Lexicon Lexicon { get; set; }
    }

    /// <summary>
    /// a set of Glyphs
    /// </summary>
    public class Script : Dictionary<Glyph, string>
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

        public Glyph(char symbol)
        {
            this.Graph = symbol;
        }
    }

    /// <summary>
    /// A Graph is a language specific use of a specific glyph or arrangement of glyphs. 
    /// </summary>
    public class Grapheme
    {
        #region Static Members
        private static HashSet<Grapheme> orthography;
        public static HashSet<Grapheme> Orthography
        {
            get
            {
                if (orthography == null)
                    orthography = new HashSet<Grapheme>();

                return orthography;
            }
            set
            {
                orthography = value;
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


    /// <summary>
    /// Class representing the surface word form.
    /// </summary>
    public class Morph
    {
        public string Graph { get; set; }
        public int Length { get { return Graph.Length; } }

        public List<Grapheme> GraphemeChain { get; set; }
        public List<Phoneme> PhonemeChain { get; set; }
        //public HashSet<Expression> Expressions { get; set; }
        public Pragma Meaning { get; set; }
        public HashSet<Feature> Features { get; set; }

        public Morph(string graph)
        {
            this.Graph = graph;
        }

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

    public class Lexicon : Dictionary<Morph, int>
    {
        public void Add(string word, int weight = 1)
        {
            if (String.IsNullOrEmpty(word))
                return;

            var thisWord = Keys.FirstOrDefault(w => w.Graph == word);

            if (thisWord == null)
            {
                thisWord = new Morph(word);

                Add(thisWord, weight);
            }
            else
            {
                this[thisWord] += weight;
            }
        }

        //public void Merge(Morph target, Morph mergee)
        //{
        //    if (target == mergee)
        //        return;

        //    if (Language.CurrentLanguage.Lexicon.ContainsKey(target) && Language.CurrentLanguage.Lexicon.ContainsKey(mergee))
        //    {
        //        Language.CurrentLanguage.Lexicon[target] += Language.CurrentLanguage.Lexicon[mergee];

        //        Language.CurrentLanguage.Lexicon.Remove(mergee);
        //    }
        //}

        //public void Merge(IEnumerable<Morph> morphs)
        //{
        //    foreach (var morph in morphs)
        //    {
        //        this.Merge(morph);
        //    }
        //}

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
        public Lexicon Lexicon { get; set; }
    }

    public class Corpus : HashSet<Text>
    {
        //a set of Texts
        public string Title { get; set; }
        //public HashSet<Text> Texts { get; set; }
        public string Description { get; set; }
    }

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

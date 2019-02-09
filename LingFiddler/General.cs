using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LingFiddler
{
    /// <summary>
    /// Class representing the surface word form.
    /// </summary>
    public class Morph
    {
        //public static char[] FilterChars { get; set; } = { '_', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };

        public string Graph { get; set; }
        public int Length { get { return Graph.Length; } }
        public int Frequency { get; set; }
        public List<Grapheme> GraphemeChain { get; set; }
        public Pragma Meaning { get; set; }
        public HashSet<Feature> Features { get; set; }

        // context - issue being how much to worry about syntax.
        // prior morphs
        // following morphs

        public Morph(string graph)
        {
            this.Graph = graph;
        }

        #region Static stuff
        private static HashSet<Morph> words;
        public static HashSet<Morph> Words
        {
            get
            {
                if (words == null)
                {
                    words = new HashSet<Morph>();
                }

                return words;
            }
            set
            {
                words = value;
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
                var thisWord = Words.FirstOrDefault(w => w.Graph == word);

                if (thisWord == null)
                {
                    thisWord = new Morph(word)
                    {
                        Frequency = weight
                    };

                    Words.Add(thisWord);
                }
                else
                {
                    thisWord.Frequency += weight;
                }
            //}
        }

        public static void Remove(Morph word)
        {
            Words.Remove(word);
        }

        public static void Clear()
        {
            Words.Clear();
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


    public class Expression
    {
        public enum GrammaticalityJudgement { Good, Bad, Weird }

        public string Graph { get; set; }
        public List<Morph> MorphChain { get; set; }
        public HashSet<string> Translations { get; set; }
        public GrammaticalityJudgement Judgement { get; set; }
        public Language Language { get; set; }

        // context? This may be retrieved from Text set
        // discourse context? e.g. semantic/pragmatic assumptions affection expression
        // leaving aside issues of code-switching for now, wrt Language source
    }

    public class Text
    {
        //a set of Expressions
        public string Title { get; set; }
        public List<Expression> Content { get; set; }
    }

    public class Corpus
    {
        //a set of Texts
        public string Title { get; set; }
        public HashSet<Text> Texts { get; set; }
        public string Description { get; set; }
    }

    public class Script
    {
        //a set of Glyphs
        public string Name { get; set; }
        public HashSet<Glyph> GlyphSet { get; set; }
    }

    public class Glyph
    {
        public string Graph { get; set; }
        //image form?
    }

    public class Language
    {
        public string Name { get; set; }
        public string Autonym { get; set; }
        public List<string> Exonyms { get; set; }

        // This perhaps will hold the handle for serializing to the language specific Database.

        public PragmaSet Lexicon { get; set; }
        public ParadigmSet Morphology { get; set; }
    }

    public class Pragma
    {
        public string Symbol { get; set; }
    }

    public class PragmaSet
    {
        public HashSet<Pragma> Pragmata { get; set; }
    }

    public class Feature { }
    public class FeatureSet { }

    public class Paradigm
    {
        // geometry of inflectional features
        // functionally the mapping of Pragma to Morph (one to many)
        // function(Pragma, Features) => Morph
    }

    public class ParadigmSet
    {
        // The set of Paradigms
        public HashSet<Paradigm> Paradigms { get; set; }
    }

    //Discource set as a way of specifying features for a sequence of Expressions.
}

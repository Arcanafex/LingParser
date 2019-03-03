using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lx
{
    public class Language
    {
        public static Language CurrentLanguage { get; set; }

        public string Name { get; set; }
        public string Autonym { get; set; }
        public List<string> Exonyms { get; set; }

        public ConceptSet Ontology { get; set; }
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
        public HashSet<Expression> Expressions { get; set; }
        public Concept Meaning { get; set; }
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

    public class FiniteStateAutomoton
    {
        public Node Start { get; private set; }
        public Node End { get; private set; }
        public Dictionary<string, Node> States { get; private set; }

        public FiniteStateAutomoton()
        {
            Start = new Node("[");
            End = new Node("]");
            States = new Dictionary<string, Node>();
        }

        public void AddTransition(string from, string to)
        {
            Node startNode;
            Node endNode;

            if (from == "_")
            {
                startNode = Start;
            }
            else if (States.ContainsKey(from))
            {
                startNode = States[from];
            }
            else
            {
                startNode = new Node(from);
                States.Add(from, startNode);
            }

            if (to == "_")
            {
                endNode = End;
            }
            else if (States.ContainsKey(to))
            {
                endNode = States[to];
            }
            else
            {
                endNode = new Node(to);
                States.Add(to, endNode);
            }


        }
    }

    public class Node
    {
        public string Value { get; private set; }
        public Dictionary<Node, int> Transitions { get; private set; }

        public Node(string value)
        {
            Value = value;
            Transitions = new Dictionary<Node, int>();
        }

        public void AddTransition(Node to, int weight = 1)
        {
            if (Transitions.ContainsKey(to))
            {
                Transitions[to] += weight;
            }
            else
            {
                Transitions.Add(to, weight);
            }
        }
    }

    public class Ngram
    {
        private string[] ngram;
        private string value;

        public Ngram(int n = 1)
        {
            ngram = new string[n];
        }

        public Ngram(string gram)
        {
            ngram = gram.ToCharArray().Select(n => n.ToString()).ToArray();
        }

        public string Onset
        {
            get
            {
                return ngram.First();
            }
            set
            {
                ngram[0] = string.IsNullOrEmpty(value) ? ngram[0] = string.Empty : ngram[0] = value;
            }
        }

        public string Coda
        {
            get
            {
                return ngram.Last();
            }
            set
            {
                int i = ngram.Length - 1;
                ngram[i] = string.IsNullOrEmpty(value) ? ngram[i] = string.Empty : ngram[i] = value;
            }
        }

        public string this[int i]
        {
            get
            {
                return ngram[i];
            }
            set
            {
                ngram[i] = value;
            }
        }

        public string Value
        {
            get
            {
                if (string.IsNullOrEmpty(value))
                    value = ToString();

                return value;
            }
        }

        public static List<Ngram> Parse(string input, int n, string pattern = "")
        {
            var ngrams = new List<Ngram>();

            if (!string.IsNullOrEmpty(input))
            {
                if (string.IsNullOrEmpty(pattern))
                {
                    // Only include edges in bigrams or longer
                    if (n > 1)
                        input = "_" + input + "_";

                    for(int i = 0; i + n <= input.Length; i++)
                    {
                        ngrams.Add(new Ngram(input.Substring(i, n)));
                    }
                }
            }

            return ngrams;
        }

        public override string ToString()
        {
            return ToString();
        }

        public string ToString(string separator = "", string edges = "_")
        {
            return ToString(separator, edges, edges);
        }

        public string ToString(string separator, string left, string right)
        {
            StringBuilder ngramstring = new StringBuilder();

            for (int n = 0; n < ngram.Length; n++)
            {
                if (n == 0)
                {
                    ngramstring.Append(string.IsNullOrEmpty(ngram[n]) ? left : ngram[n]);
                }
                else 
                {
                    ngramstring.Append(separator);

                    if (n == ngram.Length - 1)
                    {
                        ngramstring.Append(string.IsNullOrEmpty(ngram[n]) ? right : ngram[n]);
                    }
                    else
                    {
                        ngramstring.Append(ngram[n]);
                    }
                }
            }

            return ngramstring.ToString();
        }

        //public override bool Equals(object obj)
        //{
        //    if (obj is Ngram)
        //    {
        //        var nobj = obj as Ngram;

        //        return this.ToString() == nobj.ToString();
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //}

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

        public void Merge(Morph target, Morph mergee)
        {
            if (target == mergee)
                return;

            if (this.ContainsKey(target) && this.ContainsKey(mergee))
            {
                this[target] += this[mergee];

                if (mergee.Expressions != null)
                {
                    foreach (var expression in mergee.Expressions)
                    {
                        expression.Where(m => m == mergee).Select(m => expression.IndexOf(m)).ToList().ForEach(i => expression[i] = target);

                        if (target.Expressions == null)
                            target.Expressions = new HashSet<Expression>();

                        target.Expressions.Add(expression);
                    }
                }

                Remove(mergee);
            }
        }

        public void Merge(Morph target, IEnumerable<Morph> morphs)
        {
            foreach (var morph in morphs)
            {
                Merge(target, morph);
            }
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

    public class Concept
    {
        public string Symbol { get; set; }
    }

    public class ConceptSet : HashSet<Concept>
    {
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
    }

    //Discource set as a way of specifying features for a sequence of Expressions.
}

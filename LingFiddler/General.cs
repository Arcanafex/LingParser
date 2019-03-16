using System;
using System.Collections;
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

    public class FiniteStateAutomoton<T>
    {
        public int Size { get; set; }

        public Node<T> Start { get; private set; }
        public Node<T> End { get; private set; }
        public HashSet<Node<T>> States { get; private set; }

        public FiniteStateAutomoton(int size = 2)
        {
            Size = size;
            Start = Node<T>.Start;
            End = Node<T>.End;
            States = new HashSet<Node<T>>();
        }

        public void AddTransition(T[] from, T[] to)
        {
            Node<T> startNode;
            Node<T> endNode;

            if (from == null)
            {
                startNode = Start;
            }
            else
            {
                startNode = States.FirstOrDefault(n => n.Value.SequenceEqual(from));

                if (startNode == null)
                {
                    startNode = new Node<T>(from);
                }

                States.Add(startNode);
            }

            if (to == null)
            {
                endNode = End;
            }
            else
            {
                endNode = States.FirstOrDefault(n => n.Value.SequenceEqual(to));

                if (endNode == null)
                    endNode = new Node<T>(to);

                States.Add(endNode);
            }

            startNode.AddTransition(endNode);
        }

        public void Parse(T[] input)
        {
            var ngrams = new Queue<T[]>();

            if (input != null && input as IEnumerable != null)
            {
                if (Size > input.Length)
                {
                    ngrams.Enqueue(input);
                }
                else
                {
                    for (int i = 0; i + Size <= input.Length; i++)
                    {
                        ngrams.Enqueue(input.Slice(i, Size));
                    }
                }
            }

            T[] from = null;

            while (ngrams.Count > 0)
            {
                T[] to = ngrams.Dequeue();
                AddTransition(from, to);
                from = to;
            }

            AddTransition(from, null);
        }

        public List<T> GenerateRandomChain()
        {
            var random = new Random();

            return GenerateRandomChain(random);
        }

        public List<T> GenerateRandomChain(Random random)
        {
            var ngramQueue = new Queue<T[]>();

            Node<T> currentNode = Start.GetNext(random);

            while (currentNode.Type == NodeType.End)
            {
                if (Start.Transitions.Count == 0)
                {
                    return new List<T>();
                }
                else
                {
                    // TODO: prevent Start from having transitions directly to End, cause that's maybe pointless?
                    currentNode = Start.GetNext(random);
                }
            }

            while (currentNode.Type != NodeType.End)
            {
                ngramQueue.Enqueue(currentNode.Value);
                currentNode = currentNode.GetNext(random);                 
            }

            var output = new List<T>(ngramQueue.Dequeue());

            while (ngramQueue.Count > 0)
            {
                output.Add(ngramQueue.Dequeue().Last());
            }

            return output;
        }
    }

    public enum NodeType { Start, End, Value }

    public class Node<T>
    {
        internal readonly NodeType type;
        public NodeType Type
        {
            get { return type; }
        }

        public T[] Value { get; private set; }
        public Dictionary<Node<T>, int> Transitions { get; private set; }

        public Node(T[] value)
        {
            type = NodeType.Value;
            Value = value;
            Transitions = new Dictionary<Node<T>, int>();
        }

        internal Node(NodeType type)
        {
            this.type = type;
            Transitions = new Dictionary<Node<T>, int>();
        }

        public static Node<T> Start
        {
            get
            {
                return new Node<T>(NodeType.Start);
            }
        }

        public static Node<T> End
        {
            get
            {
                return new Node<T>(NodeType.End);
            }
        }


        public void AddTransition(Node<T> to, int weight = 1)
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

        public Node<T> GetNext(Random random)
        {
            int total = Transitions.Values.Sum();
            int pick = random.Next(total);

            foreach(var node in Transitions.Keys)
            {
                pick -= Transitions[node];

                if (pick <= 0)
                {
                    return node;
                }
            }

            return End;
        }

        public static bool operator== (Node<T> left, Node<T> right)
        {
            if (left is null)
                return right is null;
            else
                return left.Equals(right);
        }

        public static bool operator!=(Node<T> left, Node<T> right)
        {
            return !left.Equals(right); 
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Node<T> other))
                return false;
            else
                return Equals(other);
        }

        public bool Equals(Node<T> other)
        {
            if (other is null)
                return false;

            if (
                (Type == NodeType.Start && other.Type == NodeType.Start)
                || (Type == NodeType.End && other.Type == NodeType.End)
                )
                return true;

            if (Value != null && other.Value != null && Value.Length == other.Value.Length)
            {
                return Value.SequenceEqual(other.Value);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return GetHashCode(this);
        }

        public int GetHashCode(Node<T> node)
        {
            var hashCode = 1873668506;
            hashCode = hashCode * -1521134295 + node.Type.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<T[]>.Default.GetHashCode(node.Value);
            hashCode = hashCode * -1521134295 + EqualityComparer<Dictionary<Node<T>, int>>.Default.GetHashCode(node.Transitions);
            return hashCode;
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

        public static List<string> Parse(string input, int n, string pattern = "")
        {
            var ngrams = new List<string>();

            if (!string.IsNullOrEmpty(input))
            {
                if (string.IsNullOrEmpty(pattern))
                {
                    // Only include edges in bigrams or longer
                    if (n > 1)
                        input = "_" + input + "_";

                    for(int i = 0; i + n <= input.Length; i++)
                    {
                        ngrams.Add(input.Substring(i, n));
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
        public int UniqueWordCount
        {
            get
            {
                return Keys.Count;
            }
        }

        public Morph Add(string word, int weight = 1)
        {
            if (String.IsNullOrEmpty(word))
                return null;

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

            return thisWord;
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
        public Expression(string rawExpression = "")
        {
            Graph = rawExpression;
        }

        public enum GrammaticalityJudgement { Good, Bad, Weird }

        public string Graph { get; set; }
        public HashSet<string> Translations { get; set; }
        public GrammaticalityJudgement Judgement { get; set; }
        public Language Language { get; set; }

        // context? This may be retrieved from Text set
        // discourse context? e.g. semantic/pragmatic assumptions affection expression
        // leaving aside issues of code-switching for now, wrt Language source

        public override string ToString()
        {
            return string.Join(" ", this);
        }
    }

    public class Text : List<Expression>
    {
        //a set of Expressions
        public string Title { get; set; }
        public Lexicon Lexicon { get; set; }
        public Lexicon Morphosyntax { get; set; }

        public Text()
        {
            Lexicon = new Lexicon();
            Morphosyntax = new Lexicon
            {
                string.Empty
            };
        }

        public override string ToString()
        {
            var text = new StringBuilder();

            foreach (var exp in this)
            {
                text.Append(exp.Graph);
                text.AppendLine();
            }

            return text.ToString();
        }
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

    public static class Extensions
    {
        public static T[] Slice<T>(this T[] source, int start, int length)
        {
            if (length < 0)
            {
                return null;
            }

            T[] slice = new T[length];

            for (int i = 0; i < length; i++)
            {
                slice[i] = source[i + start];
            }

            return slice;
        }
    }

}

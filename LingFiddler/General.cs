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

        internal readonly Guid id;
        public string Name { get; set; }
        public string Autonym { get; set; }
        public List<string> Exonyms { get; set; }

        public ConceptSet Ontology { get; set; }
        public ParadigmSet Morphology { get; set; }
        public Lexicon Lexicon { get; set; }
        public Corpus Corpus { get; set; }
    }

    public abstract class Segment
    {
        internal readonly Guid id = new Guid();
        public string Name { get; set; }
        public string Graph { get; set; }
        public SegmentChain<Segment> Composition { get; set; }

        public abstract void Initialize();

        public virtual Guid GetID()
        {
            return id;
        }
    }

    public class SegmentChain<T> : LinkedList<T> where T : Segment
    {
        public static SegmentChain<T> NewSegmentChain(List<T> segmentList)
        {
            var chain = new SegmentChain<T>();
            foreach (var segment in segmentList)
            {
                chain.AddLast(segment);
            }
            return chain;
        }
    }

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

    /// <summary>
    /// A Grapheme is a language specific application of a specific glyph or arrangement of glyphs. 
    /// </summary>
    public class Grapheme : Segment
    {
        public List<GlyphChain> Glyphs { get; set; }
        public new SegmentChain<Grapheme> Composition { get; set; }

        public override void Initialize()
        {
            Glyphs = new List<GlyphChain>();
            Composition = new SegmentChain<Grapheme>();
        }

        public Grapheme(Glyph glyph)
        {
            Initialize();

            var glyphChain = new GlyphChain(new List<Glyph>() { glyph });
            Glyphs.Add(glyphChain);
            Graph = glyph.ToString();
        }

        public Grapheme(List<Glyph> glyphs)
        {
            Initialize();

            var glyphChain = new GlyphChain(glyphs);
            Glyphs.Add(glyphChain);
            Graph = glyphChain.ToString();
        }

        public void AddGlyphChain(Glyph glyph)
        {
            var glyphChain = new GlyphChain(new List<Glyph>() { glyph });

            if (Glyphs != null)
            {
                if (Glyphs.Count == 0)
                    Graph = glyph.ToString();

                if (!Glyphs.Contains(glyphChain))
                    Glyphs.Add(glyphChain);
            }
        }

        public void AddGlyphChain(List<Glyph> glyphs)
        {
            var glyphChain = new GlyphChain(glyphs);

            if (Glyphs != null)
            {
                if (Glyphs.Count == 0)
                    Graph = glyphChain.ToString();

                if (!Glyphs.Contains(glyphChain))
                    Glyphs.Add(glyphChain);
            }
        }

        public override string ToString()
        {
            string graph = Graph;

            if (Glyphs != null && Glyphs.Count > 0)
                graph = Glyphs.First().ToString();

            return graph;
        }

        public SegmentChain<Grapheme> SetComposition(List<Grapheme> componentSegments)
        {
            Composition = SegmentChain<Grapheme>.NewSegmentChain(componentSegments);
            return Composition;
        }

        public static bool operator ==(Grapheme left, Grapheme right)
        {
            if (left is null)
                return right is null;
            else
                return left.Equals(right);
        }

        public static bool operator !=(Grapheme left, Grapheme right)
        {
            return !(left.Equals(right));
        }

        public bool Equals(Grapheme other)
        {
            if (other is null)
                return false;
            else
                return this.Graph == other.Graph;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Grapheme);
        }

        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }
    }


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

    public class Phoneme : Segment
    {
        public override void Initialize()
        {
            throw new NotImplementedException();
        }
    }

    public class Phonology { }

    /// <summary>
    /// Class representing the surface word form.
    /// </summary>
    public class Morpheme : Segment
    {
        //public string Graph { get; set; }
        public int Length { get { return Graph.Length; } }

        public List<SegmentChain<Grapheme>> GraphemeChain { get; set; }
        public List<SegmentChain<Phoneme>> PhonemeChain { get; set; }
        public Concept Meaning { get; set; }
        public HashSet<Feature> Features { get; set; }
        public new SegmentChain<Morpheme> Composition;

        public override void Initialize()
        {
            Composition = new SegmentChain<Morpheme>();
            GraphemeChain = new List<SegmentChain<Grapheme>>();
            PhonemeChain = new List<SegmentChain<Phoneme>>();
            Features = new HashSet<Feature>();
        }

        public Morpheme(string graph)
        {
            Graph = graph;

            GraphemeChain = new List<SegmentChain<Grapheme>>();
            PhonemeChain = new List<SegmentChain<Phoneme>>();
            Meaning = new Concept();
            Features = new HashSet<Feature>();
        }

        public Morpheme(SegmentChain<Grapheme> glyphChain)
        {
            Graph = glyphChain.ToString();

            GraphemeChain = new List<SegmentChain<Grapheme>>() { glyphChain };
            PhonemeChain = new List<SegmentChain<Phoneme>>();
            Meaning = new Concept();
            Features = new HashSet<Feature>();
        }

        public Morpheme(List<Grapheme> graphemes)
        {
            var graphemeChain = SegmentChain<Grapheme>.NewSegmentChain(graphemes);
            Graph = graphemeChain.ToString();

            GraphemeChain = new List<SegmentChain<Grapheme>>() { graphemeChain };
            PhonemeChain = new List<SegmentChain<Phoneme>>();
            Meaning = new Concept();
            Features = new HashSet<Feature>();
        }

        public static bool operator ==(Morpheme left, Morpheme right)
        {
            if (left is null)
                return right is null;
            else
                return left.Equals(right);
        }

        public static bool operator !=(Morpheme left, Morpheme right)
        {
            return !(left.Equals(right));
        }

        public bool Equals(Morpheme other)
        {
            if (other is null)
                return false;
            else
                return this.Graph == other.Graph;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Morpheme);
        }

        public override string ToString()
        {
            return Graph;
        }

        public override int GetHashCode()
        {
            return Graph.GetHashCode();
        }
    }

    public class FiniteStateAutomoton<T>
    {
        public Dictionary<T, Node<T>> States { get; private set; }
        public Dictionary<NodeChain<T>, Dictionary<Node<T>, Transition<T>>> Transitions { get; private set; }        

        public FiniteStateAutomoton()
        {
            States = new Dictionary<T, Node<T>>();
            Transitions = new Dictionary<NodeChain<T>, Dictionary<Node<T>, Transition<T>>>();
        }

        //public void AddTransition(T[] from, T[] to)
        //{
        //    Node<T> startNode;
        //    Node<T> endNode;

        //    if (from == null)
        //    {
        //        startNode = Start;
        //    }
        //    else
        //    {
        //        startNode = States.FirstOrDefault(n => n.Value.SequenceEqual(from));

        //        if (startNode == null)
        //        {
        //            startNode = new Node<T>(from);
        //        }

        //        States.Add(startNode);
        //    }

        //    if (to == null)
        //    {
        //        endNode = End;
        //    }
        //    else
        //    {
        //        endNode = States.FirstOrDefault(n => n.Value.SequenceEqual(to));

        //        if (endNode == null)
        //            endNode = new Node<T>(to);

        //        States.Add(endNode);
        //    }

        //    startNode.AddTransition(endNode);
        //}

        public Node<T> AddNode(T segment)
        {
            var node = GetNode(segment);

            if (node == null)
            {
                node = new Node<T>(this, segment);
                States.Add(segment, node);
            }


            return node;
        }

        public void AddTransition(Transition<T> transition)
        {
            AddTransition(transition.Chain, transition.EndState, transition.Weight);
        }

        public void AddTransition(Node<T> from, Node<T> to, int weight = 1)
        {
            AddTransition(from.ToChain(), to, weight);
        }

        public void AddTransition(NodeChain<T> from, Node<T> to, int weight = 1)
        {
            // TODO: Verify all nodes are present in model's sets

            var toNode = to.Value != null ? AddNode(to.Value) : to;
            var fromChain = new NodeChain<T>();

            foreach (var node in from)
            {
                if (node.Value == null)
                    fromChain.Add(node);
                else
                    fromChain.Add(AddNode(node.Value));
            }

            if (Transitions.ContainsKey(from))
            {
                if (Transitions[from].ContainsKey(to))
                {
                    Transitions[from][to].Weight += weight;
                }
                else
                {
                    var transition = new Transition<T>()
                    {
                        Chain = from,
                        EndState = to,
                        Weight = weight
                    };

                    Transitions[from].Add(to, transition);
                }
            }
            else
            {
                var transitions = new Dictionary<Node<T>, Transition<T>>
                {
                    {
                        to,
                        new Transition<T>()
                        {
                            Chain = from,
                            EndState = to,
                            Weight = weight
                        }
                    }
                };

                Transitions.Add(from, transitions);
            }
        }

        //public NodeChain<T> StartChain(Node<T> node)
        //{
        //    var chain = new NodeChain<T>(this);
        //    chain.AddLast(node);

        //    return chain;
        //}

        //public NodeChain<T> StartChain(IEnumerable<Node<T>> nodes)
        //{
        //    var chain = new NodeChain<T>(this);

        //    foreach (var node in nodes)
        //    {
        //        chain.AddLast(node);
        //    }

        //    return chain;
        //}

        public Node<T> GetNode(T segment)
        {
            if (segment == null)
                return null;

            return States.ContainsKey(segment) ? States[segment] : null;
        }

        public Dictionary<Node<T>, Transition<T>> GetTransitions(Node<T> node)
        {
            return GetTransitions(new List<Node<T>>() { node });
        }

        public Dictionary<Node<T>, Transition<T>> GetTransitions(IEnumerable<Node<T>> chain = null)
        {
            var transitions = new Dictionary<Node<T>, Transition<T>>();
            var sequence = chain.ToChain();

            if (Transitions.Count > 0)
            {
                if (sequence.Count == 0)
                {
                    transitions = Transitions[Node<T>.Start.ToChain()];
                }
                if (Transitions.ContainsKey(sequence))
                {
                    transitions = Transitions[sequence];
                }
                else
                {
                    int skip = 0;

                    while (transitions.Count == 0 && skip < sequence.Count)
                    {
                        var key = sequence.Skip(++skip).ToChain();
                        if (Transitions.ContainsKey(key))
                            transitions = Transitions[key];
                    }
                }
            }

            return transitions;
        }

        public void Parse(T[] input, int length)
        {
            var chain = new Queue<Node<T>>();
            chain.Enqueue(Node<T>.Start);

            if (input != null && input.Length > 0)
            {
                foreach (var item in input)
                {
                    var node = AddNode(item);
                    AddTransition(chain.ToChain(), node);
                    chain.Enqueue(node);

                    if (chain.Peek() == Node<T>.Start || chain.Count > length)
                    {
                        chain.Dequeue();
                    }
                }

                AddTransition(chain.ToChain(), Node<T>.End);
            }
        }

        public List<T> GenerateRandomChain()
        {
            var random = new Random();

            return GenerateRandomChain(random);
        }

        public List<T> GenerateRandomChain(Random random)
        {
            var randomWalk = new NodeChain<T>();

            Node<T> currentState = GetNext(randomWalk, random);

            while (currentState.Type != NodeType.End)
            {
                randomWalk.Add(currentState);
                currentState = GetNext(randomWalk, random);
            }

            return randomWalk.Select(s => s.Value).ToList();
        }

        public Node<T> GetNext(IEnumerable<Node<T>> chain, Random random)
        {
            var transitions = GetTransitions(chain);
            int total = transitions.Sum(t => t.Value.Weight) + 1;
            int pick = random.Next(total);

            foreach (var transition in transitions.Values)
            {
                pick -= transition.Weight;

                if (pick <= 0)
                {
                    return transition.EndState;
                }
            }

            return Node<T>.End;
        }
    }

    public enum NodeType { Start, End, Value }

    public class Node<T>
    {
        internal readonly Guid id;
        public NodeType Type { get; }
        public FiniteStateAutomoton<T> Parent { get; }
        public T Value { get; private set; }

        internal Node(FiniteStateAutomoton<T> parent, T value)
        {
            id = Guid.NewGuid();
            Parent = parent;
            Type = NodeType.Value;
            Value = value;
        }

        internal Node(NodeType type)
        {
            this.id = new Guid((int)type, 0, 0, new byte[8]);
            this.Type = type;
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

        public void UpdateValue(T value)
        {
            if (Value.Equals(value))
                return;

            if (Parent != null)
            {
                Parent.States.Remove(Value);
                Parent.States.Add(value, this);
            }

            Value = value;
        }

        //public void AddTransition(Node<T> to, int weight = 1)
        //{
        //    if (Transitions.ContainsKey(to))
        //    {
        //        Transitions[to] += weight;
        //    }
        //    else
        //    {
        //        Transitions.Add(to, weight);
        //    }
        //}

        //public Node<T> GetNext(Random random)
        //{
        //    int total = Transitions.Values.Sum();
        //    int pick = random.Next(total);

        //    foreach(var node in Transitions.Keys)
        //    {
        //        pick -= Transitions[node];

        //        if (pick <= 0)
        //        {
        //            return node;
        //        }
        //    }

        //    return End;
        //}

        public static bool operator ==(Node<T> left, Node<T> right)
        {
            if (left is null)
                return right is null;
            else
                return left.Equals(right);
        }

        public static bool operator !=(Node<T> left, Node<T> right)
        {
            return !(left.Equals(right));
        }

        public override bool Equals(object obj)
        {
            if (obj is Node<T>)
                return Equals((Node<T>)obj);
            else
                return false;
        }

        public bool Equals(Node<T> other)
        {
            if (other is null || Type != other.Type)
                return false;

            if (id == other.id)
                return true;
            else
                return false;
        }

        public override int GetHashCode()
        {
            return BitConverter.ToInt32(id.ToByteArray(), 0);
        }

        public override string ToString()
        {
            if (Type == NodeType.Value)
                return Value.ToString();
            else
                return $"[{Type.ToString()}]";
        }
    }

    public class NodeChain<T> : List<Node<T>>
    {
        public static bool operator== (NodeChain<T> left, NodeChain<T> right)
        {
            if (left is null)
                return right is null;

            return left.Equals(right);
        }

        public static bool operator!= (NodeChain<T> left, NodeChain<T> right)
        {
            return !(left.Equals(right));
        }

        public bool Equals(NodeChain<T> other)
        {
            if (other is null)
                return false;

            if (this.Count == other.Count)
            {
                if (Count == 1)
                {

                }

                for(int i = 0; i < Count; i++)
                {
                    if (this[i] != other[i])
                        return false;
                }
            }

            return true;
        }

        public override bool Equals(object obj)
        {
            return obj is NodeChain<T> ? Equals((NodeChain<T>)obj) : false;
        }

        public override int GetHashCode()
        {
            int thing = 0;

            foreach (Node<T> item in this)
            {
                thing += BitConverter.ToInt32(item.id.ToByteArray(), 0);
            }            

            return thing;
        }

        public override string ToString()
        {
            return ToString(string.Empty);
        }

        public string ToString(string separator)
        {
            var builder = new StringBuilder();
            int i = 0;

            foreach (var node in this)
            {
                builder.Append(node.ToString());

                if (++i < this.Count)
                    builder.Append(separator);
            }

            return builder.ToString();
        }

    }

    public static class NodeChainExtensions
    {
        public static NodeChain<T> ToChain<T>(this IEnumerable<Node<T>> list)
        {
            var chain = new NodeChain<T>();

            foreach (var item in list)
            {
                chain.Add(item);
            }

            return chain;
        }

        public static NodeChain<T> ToChain<T>(this Node<T> node)
        {
            var chain = new NodeChain<T>();
            chain.Add(node);
            return chain;
        }

    }

    public class TransitionTable<T> : Dictionary<List<Node<T>>, Dictionary<Node<T>, Transition<T>>>
    {

    }

    public class Transition<T>
    {
        public NodeChain<T> Chain;
        public Node<T> EndState;
        public int Weight;

        public FiniteStateAutomoton<T> ParentModel
        {
            get
            {
                return EndState.Parent;
            }
        }

        public void MergeTransition(Transition<T> mergee)
        {
            if (mergee == null)
                return;

            if (mergee.ParentModel != null && mergee.ParentModel.Transitions.ContainsKey(mergee.Chain))
            {
                if (mergee.ParentModel.Transitions[mergee.Chain].Remove(mergee.EndState))
                {
                    if (mergee.ParentModel.Transitions[mergee.Chain].Count == 0)
                        mergee.ParentModel.Transitions.Remove(mergee.Chain);
                }
            }

            Weight += mergee.Weight;
        }

        public static bool operator== (Transition<T> left, Transition<T> right)
        {
            if (left is null)
                return right is null;

            return left.Equals(right);
        }

        public static bool operator!= (Transition<T> left, Transition<T> right)
        {
            return !(left.Equals(right));
        }

        public bool Equals(Transition<T> other)
        {
            if (other == null)
                return false;

            return EndState == other.EndState && Chain.SequenceEqual(other.Chain);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Transition<T>);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    //public class Ngram
    //{
    //    private string[] ngram;
    //    private string value;

    //    public Ngram(int n = 1)
    //    {
    //        ngram = new string[n];
    //    }

    //    public Ngram(string gram)
    //    {
    //        ngram = gram.ToCharArray().Select(n => n.ToString()).ToArray();
    //    }

    //    public string Onset
    //    {
    //        get
    //        {
    //            return ngram.First();
    //        }
    //        set
    //        {
    //            ngram[0] = string.IsNullOrEmpty(value) ? ngram[0] = string.Empty : ngram[0] = value;
    //        }
    //    }

    //    public string Coda
    //    {
    //        get
    //        {
    //            return ngram.Last();
    //        }
    //        set
    //        {
    //            int i = ngram.Length - 1;
    //            ngram[i] = string.IsNullOrEmpty(value) ? ngram[i] = string.Empty : ngram[i] = value;
    //        }
    //    }

    //    public string this[int i]
    //    {
    //        get
    //        {
    //            return ngram[i];
    //        }
    //        set
    //        {
    //            ngram[i] = value;
    //        }
    //    }

    //    public string Value
    //    {
    //        get
    //        {
    //            if (string.IsNullOrEmpty(value))
    //                value = ToString();

    //            return value;
    //        }
    //    }

    //    public static List<string> Parse(string input, int n, string pattern = "")
    //    {
    //        var ngrams = new List<string>();

    //        if (!string.IsNullOrEmpty(input))
    //        {
    //            if (string.IsNullOrEmpty(pattern))
    //            {
    //                // Only include edges in bigrams or longer
    //                if (n > 1)
    //                    input = "_" + input + "_";

    //                for(int i = 0; i + n <= input.Length; i++)
    //                {
    //                    ngrams.Add(input.Substring(i, n));
    //                }
    //            }
    //        }

    //        return ngrams;
    //    }

    //    public override string ToString()
    //    {
    //        return ToString();
    //    }

    //    public string ToString(string separator = "", string edges = "_")
    //    {
    //        return ToString(separator, edges, edges);
    //    }

    //    public string ToString(string separator, string left, string right)
    //    {
    //        StringBuilder ngramstring = new StringBuilder();

    //        for (int n = 0; n < ngram.Length; n++)
    //        {
    //            if (n == 0)
    //            {
    //                ngramstring.Append(string.IsNullOrEmpty(ngram[n]) ? left : ngram[n]);
    //            }
    //            else 
    //            {
    //                ngramstring.Append(separator);

    //                if (n == ngram.Length - 1)
    //                {
    //                    ngramstring.Append(string.IsNullOrEmpty(ngram[n]) ? right : ngram[n]);
    //                }
    //                else
    //                {
    //                    ngramstring.Append(ngram[n]);
    //                }
    //            }
    //        }

    //        return ngramstring.ToString();
    //    }

    //    //public override bool Equals(object obj)
    //    //{
    //    //    if (obj is Ngram)
    //    //    {
    //    //        var nobj = obj as Ngram;

    //    //        return this.ToString() == nobj.ToString();
    //    //    }
    //    //    else
    //    //    {
    //    //        return false;
    //    //    }
    //    //}

    //}

    public class Lexicon : Dictionary<Morpheme, int>
    {
        public Lx.Orthography Orthography;

        public int UniqueWordCount
        {
            get
            {
                return Keys.Count;
            }
        }

        public Morpheme Add(List<Grapheme> graphemes, int weight = 1)
        {
            var graphemeChain = SegmentChain<Grapheme>.NewSegmentChain(graphemes);
            var thisWord = Keys.FirstOrDefault(w => w.GraphemeChain.Contains(graphemeChain));

            if (thisWord == null)
            {
                thisWord = new Morpheme(graphemeChain);
            }

            return thisWord;
        }

        public Morpheme Add(string word, int weight = 1)
        {
            if (String.IsNullOrEmpty(word))
                return null;

            var thisWord = Keys.FirstOrDefault(w => w.Graph == word);

            if (thisWord == null)
            {
                thisWord = new Morpheme(word);

                Add(thisWord, weight);
            }
            else
            {
                this[thisWord] += weight;
            }

            return thisWord;
        }

        //public new Morpheme Add(Morpheme morpheme, int weight = 1)
        //{
        //    Morpheme thisMorph = Keys.FirstOrDefault(m => m.Equals(morpheme));

        //    if (thisMorph == null)
        //    {
        //        thisMorph = morpheme;
        //        Add(thisMorph, weight);
        //    }
        //    else
        //    {
        //        this[thisMorph] += weight;
        //    }

        //    return thisMorph;
        //}

        //public void Merge(Morpheme target, Morpheme mergee)
        //{
        //    if (target == mergee)
        //        return;

        //    if (this.ContainsKey(target) && this.ContainsKey(mergee))
        //    {
        //        this[target] += this[mergee];

        //        if (mergee.Expressions != null)
        //        {
        //            foreach (var expression in mergee.Expressions)
        //            {
        //                expression.Where(m => m == mergee).Select(m => expression.IndexOf(m)).ToList().ForEach(i => expression[i] = target);

        //                if (target.Expressions == null)
        //                    target.Expressions = new HashSet<Expression>();

        //                target.Expressions.Add(expression);
        //            }
        //        }

        //        Remove(mergee);
        //    }
        //}

        //public void Merge(Morpheme target, IEnumerable<Morpheme> morphs)
        //{
        //    foreach (var morph in morphs)
        //    {
        //        Merge(target, morph);
        //    }
        //}

    }

    public class Expression : Segment//, ICompositeSegment<Expression>
    {
        public SegmentChain<Morpheme> Sequence { get; set; }
        public new SegmentChain<Expression> Composition;

        public override void Initialize()
        {
            Sequence = new SegmentChain<Morpheme>();
            Composition = new SegmentChain<Expression>();
        }

        public Expression(string rawExpression = "")
        {
            Initialize();
            Graph = rawExpression;
        }

        public enum GrammaticalityJudgement { Good, Marginal, Bad, Weird }
        public HashSet<string> Translations { get; set; }
        public GrammaticalityJudgement Judgement { get; set; }
        public Language Language { get; set; }

        public override string ToString()
        {
            //var expression = new StringBuilder();
            //Morpheme lastMorpheme = null;

            //foreach (var morpheme in this)
            //{
            //    if (lastMorpheme)
            //}

            return string.Join("", Sequence);
        }

        public SegmentChain<Expression> SetComposition(List<Expression> componentSegments)
        {
            Composition = SegmentChain<Expression>.NewSegmentChain(componentSegments);
            return Composition;
        }

        //internal void AddLast(Morpheme morph)
        //{
        //    Sequence.AddLast(morph);
        //}

        //public SegmentChain<Expression> GetComposition()
        //{
        //    return Composition;
        //}
    }

    /// <summary>
    /// A linked list of Expression objects; expected to form a larger discourse unit. 
    /// e.g. a paragraph, a dialogue, etc.
    /// </summary>
    /// 
    public class Discourse : Segment//, ICompositeSegment<Discourse>
    {
        public SegmentChain<Expression> Expressions { get; set; }
        public new SegmentChain<Discourse> Composition { get; set; }

        public override void Initialize()
        {
            Expressions = new SegmentChain<Expression>();
            Composition = new SegmentChain<Discourse>();
        }

        public Discourse()
        {
            Expressions = new SegmentChain<Expression>();
            Composition = new SegmentChain<Discourse>();
        }

        public SegmentChain<Discourse> GetComposition()
        {
            return Composition;
        }

        public SegmentChain<Discourse> SetComposition(List<Discourse> componentSegments)
        {
            Composition = SegmentChain<Discourse>.NewSegmentChain(componentSegments);
            return Composition;
        }

        public override string ToString()
        {
            return string.Join("\n", Expressions.Select(exp => exp.Graph).ToArray());
        }

        //internal void AddLast(Expression expression)
        //{
        //    Expressions.AddLast(expression);
        //}
    }

    public class Text : Segment
    {
        public SegmentChain<Discourse> Discourse { get; set; }
        public new SegmentChain<Text> Composition;

        //a set of Discourse units of Expressions
        public string Title { get; set; }
        public Lexicon Lexicon { get; set; }
        public Lexicon Paralexicon { get; set; }
        public int Count
        {
            get
            {
                return Discourse.Count;
            }
        }

        public override void Initialize()
        {
            Discourse = new SegmentChain<Discourse>();
            Composition = new SegmentChain<Text>();
        }

        public Text()
        {
            Discourse = new SegmentChain<Discourse>();
            Composition = new SegmentChain<Text>();
            Lexicon = new Lexicon();
            Paralexicon = new Lexicon
            {
                string.Empty
            };
        }

        public override string ToString()
        {
            var text = new StringBuilder();

            foreach (var paragraph in Discourse)
            {
                text.AppendLine(paragraph.ToString());
                text.AppendLine();
            }

            return text.ToString();
        }

        //internal void AddLast(Discourse paragraph)
        //{
        //    Discourse.AddLast(paragraph);
        //}

        internal void Clear()
        {
            if (Discourse != null)
                Discourse.Clear();

            if (Composition != null)
                Composition.Clear();
        }
    }

    /// <summary>
    /// A collection of Texts.
    /// </summary>
    public class Corpus : HashSet<Text>
    {
        //a set of Texts
        public string Title { get; set; }
        public string Description { get; set; }
    }


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

public static class Extensions
{
    public static T[] Slice<T>(this T[] source, int start)
    {


        if (source != null && start < source.Length)
        {
            if (start < 0)
                start = 0;

            T[] slice = new T[source.Length - start];

            for (int i = 0; i + start < source.Length; i++)
            {
                slice[i] = source[i + start];
            }

            return slice;
        }
        else
        {
            return null;
        }
    }

    public static T[] Slice<T>(this T[] source, int start, int length)
    {
        if (length < 0)
            return null;

        T[] slice = new T[length];

        for (int i = 0; i < length; i++)
        {
            slice[i] = source[i + start];
        }

        return slice;
    }

    public static int Overlap<T>(this T[] rightArray, T[] leftArray)
    {
        int index = 0;

        if (
            rightArray != null
            && leftArray != null
            && rightArray.Length > 0
            && leftArray.Length > 0
            //&& rightArray.Intersect(leftArray).ToArray().Length > 0
            )
        {
            for (int i = 0; i < rightArray.Length && i < leftArray.Length; i++)
            {
                var rightStub = rightArray.Slice(0, i + 1);
                var leftStub = leftArray.Slice(leftArray.Length - 1 - i);

                if (rightStub.SequenceEqual(leftStub))
                {
                    index = i + 1;
                }
            }
        }

        return index;
    }
}


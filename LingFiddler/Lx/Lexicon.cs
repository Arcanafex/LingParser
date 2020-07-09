using System;
using System.Collections.Generic;
using System.Linq;

namespace Lx
{
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
            //var thisWord = Keys.FirstOrDefault(w => w.GraphemeChain.Contains(graphemeChain));
            var thisWord = Keys.FirstOrDefault(w => w.Graph == graphemeChain.Graph);

            if (thisWord == null)
            {
                thisWord = new Morpheme(graphemeChain);
                Add(thisWord, weight);
            }
            else
            {
                this[thisWord] += weight;
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


}

